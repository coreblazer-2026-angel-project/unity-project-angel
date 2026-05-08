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

        [Header("动态角色资源")]
        [Tooltip("Resources 下的角色 prefab 文件夹。Prefab 上挂 StoryCharacter。")]
        public string characterResourceFolder = "Story/Characters";

        [Tooltip("找不到角色 mount 时自动创建一个 UI mount。")]
        public bool autoCreateMissingMount = true;

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
            preset.aspectFitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
        }

        /// <summary>显示角色立绘</summary>
        public void ShowCharacter(string characterId, string expressionName,
                float screenWidthPercent = 0.4f,
                float screenBottomPercent = 0.15f,
                float horizontalOffset = 0f) {

            var preset = presets.Find(p => p.characterId == characterId);
            if (preset == null) {
                if (!autoCreateMissingMount) {
                    Debug.LogWarning($"[StoryCharacterManager] Preset not found: '{characterId}'");
                    return;
                }

                preset = CreatePreset(characterId);
            }

            // 布局 mount
            float targetWidth = Screen.width * screenWidthPercent;
            var rect = preset.mount.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0);
            rect.anchorMax = new Vector2(0.5f, 0);
            rect.pivot = new Vector2(0.5f, 0);
            rect.sizeDelta = new Vector2(targetWidth, 0);
            rect.anchoredPosition = new Vector2(
                Screen.width * horizontalOffset,
                Screen.height * screenBottomPercent);

            Sprite sprite = FindSprite(characterId, expressionName);

            if (sprite == null) {
                Debug.LogWarning($"[StoryCharacterManager] Sprite not found: {characterId}/{expressionName}");
                return;
            }

            ApplySprite(preset, sprite);
        }

        CharacterPreset CreatePreset(string characterId) {
            var mountObj = new GameObject($"StoryCharacterMount_{characterId}", typeof(RectTransform));
            RectTransform parent = transform as RectTransform;
            if (parent == null) {
                Canvas canvas = FindObjectOfType<Canvas>();
                parent = canvas != null ? canvas.transform as RectTransform : null;
            }

            mountObj.transform.SetParent(parent != null ? parent : transform, false);

            var preset = new CharacterPreset {
                characterId = characterId,
                mount = mountObj.GetComponent<RectTransform>()
            };
            presets.Add(preset);
            SetupMount(preset);
            return preset;
        }

        Sprite FindSprite(string characterId, string expressionName) {
            Sprite sprite = FindSpriteInCharacters(FindObjectsOfType<StoryCharacter>(), characterId, expressionName);
            if (sprite != null)
                return sprite;

            GameObject[] prefabs = Resources.LoadAll<GameObject>(characterResourceFolder);
            for (int i = 0; i < prefabs.Length; i++) {
                StoryCharacter character = prefabs[i].GetComponent<StoryCharacter>();
                if (character == null)
                    character = prefabs[i].GetComponentInChildren<StoryCharacter>(true);

                sprite = FindSpriteInCharacter(character, characterId, expressionName);
                if (sprite != null)
                    return sprite;
            }

            return null;
        }

        Sprite FindSpriteInCharacters(StoryCharacter[] characters, string characterId, string expressionName) {
            for (int i = 0; i < characters.Length; i++) {
                Sprite sprite = FindSpriteInCharacter(characters[i], characterId, expressionName);
                if (sprite != null)
                    return sprite;
            }
            return null;
        }

        Sprite FindSpriteInCharacter(StoryCharacter character, string characterId, string expressionName) {
            if (character == null || character.characterId != characterId)
                return null;

            for (int i = 0; i < character.expressions.Count; i++) {
                var expression = character.expressions[i];
                if (expression.sprite == null)
                    continue;

                if (expression.name.Equals(expressionName, StringComparison.OrdinalIgnoreCase))
                    return expression.sprite;
            }

            return character.defaultSprite;
        }

        void ApplySprite(CharacterPreset preset, Sprite sprite) {
            if (preset.image == null || sprite == null) return;
            preset.image.sprite = sprite;
            preset.image.enabled = true;
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
