using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Story {

    /// <summary>
    /// 角色立绘组件
    /// 负责显示/隐藏立绘、切换表情（Sprite）
    /// </summary>
    public class StoryCharacter : MonoBehaviour {
        [Header("角色ID（用于StoryPlayer查找）")]
        public string characterId;

        [Header("表情列表")]
        public List<ExpressionEntry> expressions = new List<ExpressionEntry>();

        [Serializable]
        public class ExpressionEntry {
            public string name = "默认";
            public Sprite sprite;
        }

        [Header("默认表情（默认显示的Sprite）")]
        public Sprite defaultSprite;

        // 组件引用
        private Image _image;
        private string _currentExpression = "";

        void Awake() {
            // 确保有 RectTransform（在 Canvas 下时必须）
            RectTransform rect = GetComponent<RectTransform>();
            if (rect == null) {
                gameObject.AddComponent<RectTransform>();
            }

            _image = GetComponent<Image>();
            if (_image == null) {
                _image = gameObject.AddComponent<Image>();
            }
            _image.raycastTarget = false;

            // 默认隐藏
            gameObject.SetActive(false);
        }

        void Start() {
            // 显示默认表情
            if (defaultSprite != null) {
                _image.sprite = defaultSprite;
                _image.SetNativeSize();
                LimitSize();
            } else if (expressions.Count > 0 && expressions[0].sprite != null) {
                var first = expressions[0];
                _image.sprite = first.sprite;
                _image.SetNativeSize();
                LimitSize();
                _currentExpression = first.name;
            }
        }

        /// <summary>限制立绘最大宽度（像素）</summary>
        private void LimitSize(float maxWidth = 400f) {
            RectTransform rect = GetComponent<RectTransform>();
            if (rect == null) return;

            float currentWidth = rect.rect.width;
            if (currentWidth > maxWidth && currentWidth > 0) {
                float scale = maxWidth / currentWidth;
                rect.localScale = new Vector3(scale, scale, 1f);
            } else {
                rect.localScale = Vector3.one;
            }
        }

        /// <summary>显示立绘</summary>
        public void Show() {
            gameObject.SetActive(true);
        }

        /// <summary>隐藏立绘</summary>
        public void Hide() {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 切换表情
        /// 查找 name 匹配的表情并切换对应 Sprite
        /// </summary>
        public void SetExpression(string expressionName) {
            if (string.IsNullOrEmpty(expressionName)) return;

            ExpressionEntry entry = expressions.Find(e =>
                e.name.Equals(expressionName, StringComparison.OrdinalIgnoreCase));

            if (entry == null) {
                Debug.LogWarning($"[StoryCharacter] Expression '{expressionName}' not found for '{characterId}'.");
                return;
            }

            if (entry.sprite == null) {
                Debug.LogWarning($"[StoryCharacter] Expression '{expressionName}' sprite is null.");
                return;
            }

            _currentExpression = entry.name;
            _image.sprite = entry.sprite;
            _image.SetNativeSize();
            LimitSize();
            gameObject.SetActive(true);
        }
    }
}
