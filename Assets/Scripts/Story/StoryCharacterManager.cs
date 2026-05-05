using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

namespace Game.Story {

    /// <summary>
    /// 角色立绘管理器
    /// 统一管理所有角色的立绘显示，支持多角色同屏
    /// </summary>
    public class StoryCharacterManager : MonoBehaviour {
        public static StoryCharacterManager Instance { get; private set; }

        [Header("多角色预设（每个预设对应一个 RawImage）")]
        public List<CharacterPreset> presets = new List<CharacterPreset>();

        [Serializable]
        public class CharacterPreset {
            public string characterId;
            public RectTransform mount;          // RawImage 挂在哪
            public RawImage rawImage;
            public AspectRatioFitter aspectFitter;
        }

        // 运行时的可读写纹理缓存
        private Dictionary<string, Texture2D> _textureCache = new Dictionary<string, Texture2D>();
        private HashSet<string> _pendingReadbacks = new HashSet<string>();
        private HashSet<string> _visibleCharacters = new HashSet<string>();

        // 屏幕尺寸记录
        private int _lastScreenWidth = -1;
        private int _lastScreenHeight = -1;

        void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start() {
            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;
            SetupAllMounts();
        }

        void Update() {
            if (Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight) {
                _lastScreenWidth = Screen.width;
                _lastScreenHeight = Screen.height;
                UpdateAllMounts();
            }
        }

        void SetupAllMounts() {
            foreach (var preset in presets) {
                SetupMount(preset);
            }
        }

        void SetupMount(CharacterPreset preset) {
            if (preset.mount == null) return;

            var rect = preset.mount.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

            if (preset.rawImage == null) {
                preset.rawImage = preset.mount.GetComponent<RawImage>();
                if (preset.rawImage == null) {
                    preset.rawImage = preset.mount.gameObject.AddComponent<RawImage>();
                }
            }
            preset.rawImage.raycastTarget = false;
            preset.rawImage.enabled = false;

            if (preset.aspectFitter == null) {
                preset.aspectFitter = preset.mount.GetComponent<AspectRatioFitter>();
                if (preset.aspectFitter == null) {
                    preset.aspectFitter = preset.mount.gameObject.AddComponent<AspectRatioFitter>();
                }
            }
            preset.aspectFitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
        }

        void UpdateAllMounts() {
            foreach (var preset in presets) {
                if (preset.mount == null) continue;
                var rect = preset.mount.GetComponent<RectTransform>();
                rect.ForceUpdateRectTransforms();
            }
        }

        /// <summary>显示角色立绘</summary>
        public void ShowCharacter(string characterId, string expressionName, float screenWidthPercent, float screenBottomPercent, float horizontalOffset) {
            var preset = presets.Find(p => p.characterId == characterId);
            if (preset == null || preset.rawImage == null) {
                Debug.LogWarning($"[StoryCharacterManager] Preset not found for '{characterId}'.");
                return;
            }

            // 设置尺寸
            float targetWidth = Screen.width * screenWidthPercent;
            var rect = preset.mount.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0);
            rect.anchorMax = new Vector2(0.5f, 0);
            rect.pivot = new Vector2(0.5f, 0);
            rect.sizeDelta = new Vector2(targetWidth, 0);
            float bottomOffset = Screen.height * screenBottomPercent;
            float hOffset = Screen.width * horizontalOffset;
            rect.anchoredPosition = new Vector2(hOffset, bottomOffset);

            // 加载纹理
            string cacheKey = characterId + "/" + expressionName;
            if (_textureCache.TryGetValue(cacheKey, out var cached)) {
                ApplyTexture(preset, cached);
            } else if (!_pendingReadbacks.Contains(cacheKey)) {
                LoadTextureAsync(preset, characterId, expressionName, cacheKey);
            }
        }

        /// <summary>隐藏所有角色</summary>
        public void HideAllCharacters() {
            foreach (var preset in presets) {
                if (preset.rawImage != null) preset.rawImage.enabled = false;
            }
            _visibleCharacters.Clear();
        }

        /// <summary>隐藏角色</summary>
        public void HideCharacter(string characterId) {
            var preset = presets.Find(p => p.characterId == characterId);
            if (preset != null && preset.rawImage != null) {
                preset.rawImage.enabled = false;
            }
        }

        void LoadTextureAsync(CharacterPreset preset, string characterId, string expressionName, string cacheKey) {
            // 查找对应 StoryCharacter 上的 Sprite
            var characterObj = FindCharacterObject(characterId);
            Sprite sprite = null;
            if (characterObj != null) {
                var sc = characterObj.GetComponent<StoryCharacter>();
                if (sc != null) {
                    sprite = sc.expressions.Find(e =>
                        e.name.Equals(expressionName, StringComparison.OrdinalIgnoreCase)).sprite;
                }
            }

            if (sprite == null || sprite.texture == null) {
                Debug.LogWarning($"[StoryCharacterManager] Sprite not found: {characterId}/{expressionName}");
                return;
            }

            _pendingReadbacks.Add(cacheKey);

            int width = (int)sprite.rect.width;
            int height = (int)sprite.rect.height;
            int x = (int)sprite.rect.x;
            int y = (int)sprite.rect.y;

            // AsyncGPUReadback：GPU 直接读回数据，无需 Readable
            Action<UnityEngine.Rendering.AsyncGPUReadbackRequest> onComplete =
                request => {
                    _pendingReadbacks.Remove(cacheKey);
                    if (request.hasError || !request.done) {
                        Debug.LogError($"[StoryCharacterManager] GPU readback failed for {cacheKey}");
                        return;
                    }

                    var data = request.GetData<byte>();
                    if (data.Length == 0) {
                        Debug.LogError($"[StoryCharacterManager] Empty readback for {cacheKey}");
                        return;
                    }

                    // 创建 RGBA32 纹理
                    var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
                    tex.filterMode = FilterMode.Bilinear;
                    tex.wrapMode = TextureWrapMode.Clamp;

                    // request 里的数据是 BGRA，需要转 RGBA
                    // 先把 BGRA 转成 RGBA
                    byte[] pixels = new byte[data.Length];
                    for (int i = 0; i < data.Length; i += 4) {
                        pixels[i]     = data[i + 2]; // R <- B
                        pixels[i + 1] = data[i + 1]; // G <- G
                        pixels[i + 2] = data[i];     // B <- R
                        pixels[i + 3] = data[i + 3]; // A <- A
                    }
                    tex.LoadRawTextureData(pixels);
                    tex.Apply();

                    _textureCache[cacheKey] = tex;
                    ApplyTextureOnMainThread(preset, tex);
                };

            AsyncGPUReadback.Request(sprite.texture, 0,
                request => {
                    _pendingReadbacks.Remove(cacheKey);
                    if (request.hasError || !request.done) {
                        Debug.LogError($"[StoryCharacterManager] GPU readback failed for {cacheKey}");
                        return;
                    }

                    var data = request.GetData<byte>();
                    if (data.Length == 0) {
                        Debug.LogError($"[StoryCharacterManager] Empty readback for {cacheKey}");
                        return;
                    }

                    var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
                    tex.filterMode = FilterMode.Bilinear;
                    tex.wrapMode = TextureWrapMode.Clamp;

                    byte[] pixels = new byte[data.Length];
                    for (int i = 0; i < data.Length; i += 4) {
                        pixels[i]     = data[i + 2];
                        pixels[i + 1] = data[i + 1];
                        pixels[i + 2] = data[i];
                        pixels[i + 3] = data[i + 3];
                    }
                    tex.LoadRawTextureData(pixels);
                    tex.Apply();

                    _textureCache[cacheKey] = tex;
                    ApplyTextureOnMainThread(preset, tex);
                });
        }

        void ApplyTextureOnMainThread(CharacterPreset preset, Texture2D tex) {
            // 确保在主线程执行
            if (Application.isPlaying) {
                ApplyTexture(preset, tex);
            }
        }

        void ApplyTexture(CharacterPreset preset, Texture2D tex) {
            if (preset.rawImage == null || tex == null) return;
            preset.rawImage.texture = tex;
            preset.rawImage.uvRect = new Rect(0, 1, 1, -1); // 翻转 Y
            preset.rawImage.enabled = true;
            if (preset.aspectFitter != null) {
                preset.aspectFitter.aspectRatio = (float)tex.width / tex.height;
            }
        }

        GameObject FindCharacterObject(string characterId) {
            var player = FindObjectOfType<StoryCharacter>();
            if (player != null && player.characterId == characterId) return player.gameObject;

            // 查找子对象
            foreach (var sc in FindObjectsOfType<StoryCharacter>()) {
                if (sc.characterId == characterId) return sc.gameObject;
            }
            return null;
        }

        void OnDestroy() {
            // 释放缓存纹理
            foreach (var kv in _textureCache) {
                if (kv.Value != null) Destroy(kv.Value);
            }
            _textureCache.Clear();
        }
    }

    /// <summary>
    /// 角色立绘组件（简化版，配合 StoryCharacterManager 使用）
    /// 只负责存储表情配置，不再直接管理渲染
    /// </summary>
    public class StoryCharacter : MonoBehaviour {
        [Header("角色ID")]
        public string characterId;

        [Header("表情列表")]
        public List<ExpressionEntry> expressions = new List<ExpressionEntry>();

        [Serializable]
        public class ExpressionEntry {
            public string name = "默认";
            public Sprite sprite;
        }
    }
}
