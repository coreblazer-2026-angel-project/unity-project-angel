using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Story {

    /// <summary>
    /// 角色立绘管理器
    /// 统一管理所有角色的立绘显示，支持多角色同屏
    /// </summary>
    public class StoryCharacterManager : MonoBehaviour {
        public static StoryCharacterManager Instance { get; private set; }

        [Header("角色预设")]
        public List<CharacterPreset> presets = new List<CharacterPreset>();

        [Serializable]
        public class CharacterPreset {
            public string characterId;
            public RectTransform mount;
            [NonSerialized] public Image image;
            [NonSerialized] public AspectRatioFitter aspectFitter;
        }

        // 屏幕尺寸
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
                SetupAllMounts();
            }
        }

        void SetupAllMounts() {
            foreach (var preset in presets) SetupMount(preset);
        }

        void SetupMount(CharacterPreset preset) {
            if (preset.mount == null) return;

            var rect = preset.mount.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0);
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

            if (preset.image == null) {
                preset.image = preset.mount.GetComponent<Image>();
                if (preset.image == null) preset.image = preset.mount.gameObject.AddComponent<Image>();
            }
            preset.image.raycastTarget = false;
            preset.image.enabled = false;

            if (preset.aspectFitter == null) {
                preset.aspectFitter = preset.mount.GetComponent<AspectRatioFitter>();
                if (preset.aspectFitter == null) preset.aspectFitter = preset.mount.gameObject.AddComponent<AspectRatioFitter>();
            }
            preset.aspectFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
        }

        /// <summary>显示角色立绘</summary>
        /// <param name="characterId">角色ID</param>
        /// <param name="expressionName">表情名称</param>
        /// <param name="heightPercent">立绘高度占屏幕高度的比例（0.0-1.0），默认 0.8</param>
        /// <param name="bottomOffset">底部偏移（像素），默认 20</param>
        /// <param name="horizontalOffset">水平偏移（像素），默认 0</param>
        public void ShowCharacter(string characterId, string expressionName,
                float heightPercent = 0.8f,
                float bottomOffset = 20f,
                float horizontalOffset = 0f) {

            var preset = presets.Find(p => p.characterId == characterId);
            if (preset == null) return;

            if (preset.mount == null) return;

            // 检查 mount 是否已被销毁（通过尝试访问 transform）
            try {
                var _ = preset.mount.transform;
            } catch (MissingReferenceException) {
                return;
            }

            // 布局 mount - 基于高度的大小计算
            float targetHeight = Screen.height * heightPercent;
            var rect = preset.mount.GetComponent<RectTransform>();
            if (rect == null) {
                Debug.LogWarning($"[StoryCharacterManager] Mount has no RectTransform: '{characterId}'");
                return;
            }
            rect.anchorMax = new Vector2(0.5f, 0);
            rect.pivot = new Vector2(0.5f, 0);
            rect.anchoredPosition = new Vector2(horizontalOffset, bottomOffset);
            rect.sizeDelta = new Vector2(0, targetHeight);

            // 查找 Sprite
            Sprite sprite = null;
            var allChars = FindObjectsOfType<StoryCharacter>();
            foreach (var sc in allChars) {
                if (sc.characterId != characterId) continue;
                foreach (var e in sc.expressions) {
                    if (e.name.Equals(expressionName, StringComparison.OrdinalIgnoreCase)) {
                        sprite = e.sprite;
                        break;
                    }
                }
                if (sprite != null) break;
            }

            // fallback 到 defaultSprite
            if (sprite == null) {
                foreach (var sc in allChars) {
                    if (sc.characterId == characterId && sc.defaultSprite != null) {
                        sprite = sc.defaultSprite;
                        break;
                    }
                }
            }

            if (sprite == null) {
                Debug.LogWarning($"[StoryCharacterManager] Sprite not found: {characterId}/{expressionName}");
                return;
            }

            ApplySprite(preset, sprite);
        }

        void ApplySprite(CharacterPreset preset, Sprite sprite) {
            if (preset.image == null || sprite == null) return;
            preset.image.sprite = sprite;
            preset.image.enabled = true;
            // 水平反转
            var rt = preset.image.rectTransform;
            rt.localScale = new Vector3(-1, 1, 1);
            if (preset.aspectFitter != null)
                preset.aspectFitter.aspectRatio = sprite.rect.width / sprite.rect.height;
        }

        /// <summary>隐藏角色</summary>
        public void HideCharacter(string characterId) {
            var preset = presets.Find(p => p.characterId == characterId);
            if (preset?.image != null) preset.image.enabled = false;
        }

        /// <summary>隐藏所有角色</summary>
        public void HideAllCharacters() {
            foreach (var p in presets)
                if (p.image != null) p.image.enabled = false;
        }

        /// <summary>获取当前显示的角色预设</summary>
        public CharacterPreset GetActivePreset() {
            foreach (var p in presets) {
                if (p.image != null && p.image.enabled) {
                    return p;
                }
            }
            return null;
        }

        /// <summary>获取指定ID的角色预设</summary>
        public CharacterPreset GetPreset(string characterId) {
            return presets.Find(p => p.characterId == characterId);
        }
    }
}
