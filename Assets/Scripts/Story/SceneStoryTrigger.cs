using UnityEngine;

namespace Game.Story {

    /// <summary>
    /// 场景剧情触发器
    /// 挂在场景中的触发器（Collider2D）上，靠近时自动触发剧情
    /// 
    /// 使用方式：
    /// 1. 在场景中创建空物体，添加 Collider2D（设为 Trigger）
    /// 2. 添加此组件
    /// 3. 关联 Ink JSON Asset
    /// </summary>
    public class SceneStoryTrigger : MonoBehaviour {
        [Header("剧情资源")]
        [Tooltip("关联的 Ink JSON 资源")]
        public TextAsset inkJsonAsset;

        [Header("剧情配置")]
        [Tooltip("从哪个 Knot 开始")]
        public string knot = "";

        [Header("触发设置")]
        [Tooltip("触发一次后是否禁用")]
        public bool triggerOnce = true;

        [Tooltip("是否显示提示 UI")]
        public bool showHint = true;

        [Tooltip("提示文本")]
        public string hintText = "按 E 对话";

        [Header("游戏状态")]
        [Tooltip("触发时是否暂停游戏")]
        public bool pauseGame = true;

        // 内部状态
        private bool _isPlayerInRange;
        private bool _hasTriggered;
        private GameObject _hintUI;

        // 事件
        public System.Action<SceneStoryTrigger> OnTriggered;

        void Start() {
            if (showHint) {
                CreateHintUI();
            }
        }

        void Update() {
            if (_isPlayerInRange && !_hasTriggered) {
                if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0)) {
                    TriggerStory();
                }
            }
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (!other.CompareTag("Player")) return;

            _isPlayerInRange = true;
            ShowHint();
        }

        void OnTriggerExit2D(Collider2D other) {
            if (!other.CompareTag("Player")) return;

            _isPlayerInRange = false;
            HideHint();
        }

        /// <summary>
        /// 手动触发剧情（可被其他脚本调用）
        /// </summary>
        public void TriggerStory() {
            if (_hasTriggered && triggerOnce) return;
            if (inkJsonAsset == null) {
                Debug.LogWarning($"[SceneStoryTrigger] No ink JSON on {gameObject.name}");
                return;
            }
            if (StoryManager.Instance == null) {
                Debug.LogWarning("[SceneStoryTrigger] StoryManager not found in scene");
                return;
            }

            _hasTriggered = true;
            HideHint();

            OnTriggered?.Invoke(this);
            StoryManager.Instance.PlayStory(inkJsonAsset, knot);
        }

        /// <summary>
        /// 重置触发器状态
        /// </summary>
        public void ResetTrigger() {
            _hasTriggered = false;
        }

        void CreateHintUI() {
            // 创建简单的提示 UI
            var canvas = new GameObject("StoryHintCanvas");
            canvas.transform.SetParent(transform);
            canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            var hintObj = new GameObject("HintText");
            hintObj.transform.SetParent(canvas.transform);

            var rect = hintObj.AddComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, 50);

            var text = hintObj.AddComponent<UnityEngine.UI.Text>();
            text.text = hintText;
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;

            var outline = hintObj.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2, 2);

            _hintUI = hintObj;
            _hintUI.SetActive(false);
        }

        void ShowHint() {
            if (_hintUI != null && showHint && !_hasTriggered) {
                _hintUI.SetActive(true);
            }
        }

        void HideHint() {
            if (_hintUI != null) {
                _hintUI.SetActive(false);
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Test Trigger")]
        void TestTrigger() {
            TriggerStory();
        }
#endif
    }
}
