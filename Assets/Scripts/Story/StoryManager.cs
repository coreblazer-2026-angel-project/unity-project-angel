using UnityEngine;
using System;

namespace Game.Story {

    /// <summary>
    /// 剧情系统管理器
    /// 作为剧情模块的入口点，提供统一的 API 给其他系统调用
    ///
    /// 使用方式（一行代码播放剧情）：
    ///   StoryManager.Play("SampleStory");                    // 播放默认 start 节点
    ///   StoryManager.Play("SampleStory", "chapter2");       // 播放指定节点
    ///   StoryManager.Play("SampleStory", OnStoryEnd);       // 带回调
    ///
    /// 或者通过实例：
    ///   StoryManager.Instance.Play("SampleStory");
    ///
    /// 运行时如果 StoryManager 不存在，会自动实例化 StoryManager.prefab
    /// </summary>
    public class StoryManager : MonoBehaviour {
        public static StoryManager Instance { get; private set; }

        [Header("核心组件（自动查找）")]
        [SerializeField] private InkStoryPlayer _storyPlayer;
        [SerializeField] private StoryDialoguePanel _dialoguePanel;
        [SerializeField] private StoryChoicePanel _choicePanel;
        [SerializeField] private StoryCharacterManager _characterManager;

        [Header("剧情配置")]
        [SerializeField] private bool _pauseGameDuringStory = true;

        [Header("自动实例化配置")]
        [Tooltip("运行时如果不存在，是否自动实例化 prefab（留空则不自动实例化）")]
        [SerializeField] private StoryManager _prefabForAutoInstantiate;

        private static StoryManager PrefabForAutoInstantiate => Instance?._prefabForAutoInstantiate;
        private static bool _autoInstantiateEnabled = false;

        // 事件
        public event Action OnStoryStarted;
        public event Action OnStoryEnded;

        // 状态
        public bool IsPlaying => _storyPlayer != null && _storyPlayer.IsPlaying;

        void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeComponents();
        }

        void OnDestroy() {
            if (Instance == this) Instance = null;
        }

        void InitializeComponents() {
            // 查找或创建子组件
            if (_storyPlayer == null) {
                _storyPlayer = GetComponent<InkStoryPlayer>();
                if (_storyPlayer == null) _storyPlayer = gameObject.AddComponent<InkStoryPlayer>();
            }

            if (_dialoguePanel == null) {
                _dialoguePanel = FindObjectOfType<StoryDialoguePanel>();
            }

            if (_choicePanel == null) {
                _choicePanel = FindObjectOfType<StoryChoicePanel>();
            }

            if (_characterManager == null) {
                _characterManager = FindObjectOfType<StoryCharacterManager>();
            }

            // 订阅事件
            if (_storyPlayer != null) {
                _storyPlayer.OnStoryStart += HandleStoryStart;
                _storyPlayer.OnStoryEnd += HandleStoryEnd;
            }
        }

        // ==================== 静态便捷 API ====================

        /// <summary>
        /// 一行代码播放剧情（最简方式）
        /// 自动在 Resources/Story/ 目录下查找文件
        /// </summary>
        /// <param name="storyFileName">剧情文件名（不含扩展名），如 "SampleStory"</param>
        /// <param name="knot">起始节点（可选，默认 "start"）</param>
        public static void Play(string storyFileName, string knot = "") {
            EnsureInstance().PlayStoryInternal(storyFileName, knot, null);
        }

        /// <summary>
        /// 一行代码播放剧情（带回调）
        /// </summary>
        /// <param name="storyFileName">剧情文件名（不含扩展名）</param>
        /// <param name="knot">起始节点（可选，默认 "start"）</param>
        /// <param name="onComplete">剧情结束回调（可选）</param>
        public static void Play(string storyFileName, string knot, Action onComplete) {
            EnsureInstance().PlayStoryInternal(storyFileName, knot, onComplete);
        }

        /// <summary>
        /// 一行代码播放剧情（带开始回调和结束回调）
        /// </summary>
        /// <param name="storyFileName">剧情文件名（不含扩展名）</param>
        /// <param name="onStart">剧情开始回调（可选）</param>
        /// <param name="onComplete">剧情结束回调（可选）</param>
        public static void Play(string storyFileName, Action onStart, Action onComplete) {
            EnsureInstance().PlayStoryInternal(storyFileName, "", onComplete, onStart);
        }

        /// <summary>
        /// 一行代码播放剧情（指定节点 + 回调）
        /// </summary>
        /// <param name="storyFileName">剧情文件名（不含扩展名）</param>
        /// <param name="knot">起始节点</param>
        /// <param name="onStart">剧情开始回调（可选）</param>
        /// <param name="onComplete">剧情结束回调（可选）</param>
        public static void Play(string storyFileName, string knot, Action onStart, Action onComplete) {
            EnsureInstance().PlayStoryInternal(storyFileName, knot, onComplete, onStart);
        }

        /// <summary>
        /// 停止当前剧情
        /// </summary>
        public static void Stop() {
            Instance?.StopStory();
        }

        /// <summary>
        /// 继续剧情（模拟点击/空格）
        /// </summary>
        public static void Continue() {
            Instance?.DoContinue();
        }

        /// <summary>
        /// 选择选项
        /// </summary>
        public static void SelectChoice(int index) {
            Instance?.DoSelectChoice(index);
        }

        /// <summary>
        /// 显示角色
        /// </summary>
        public static void ShowCharacter(string characterId, string expression = "",
                float screenWidthPercent = 0.4f, float screenBottomPercent = 0.15f) {
            Instance?.DoShowCharacter(characterId, expression, screenWidthPercent, screenBottomPercent);
        }

        /// <summary>
        /// 隐藏角色
        /// </summary>
        public static void HideCharacter(string characterId = "") {
            Instance?.DoHideCharacter(characterId);
        }

        /// <summary>
        /// 检查剧情文件是否存在
        /// </summary>
        public static bool StoryExists(string fileName) {
            return StoryLoader.Instance.Exists(fileName);
        }

        /// <summary>
        /// 设置运行时自动实例化（启用后，即使场景中没有 StoryManager，也会自动创建）
        /// </summary>
        /// <param name="enabled">是否启用</param>
        /// <param name="prefab">实例化用的 prefab（可选，从 Instance 获取）</param>
        public static void SetAutoInstantiate(bool enabled, StoryManager prefab = null) {
            _autoInstantiateEnabled = enabled;
            if (prefab != null && Instance != null) {
                Instance._prefabForAutoInstantiate = prefab;
            }
        }

        /// <summary>
        /// 确保 StoryManager 实例存在
        /// </summary>
        static StoryManager EnsureInstance() {
            if (Instance != null) return Instance;

            if (_autoInstantiateEnabled && PrefabForAutoInstantiate != null) {
                var go = Instantiate(PrefabForAutoInstantiate.gameObject);
                return Instance;
            }

            // 尝试查找场景中的
            var existing = FindObjectOfType<StoryManager>();
            if (existing != null) return existing;

            // 尝试从 Resources 加载
            var prefab = Resources.Load<StoryManager>("Story/StoryManager");
            if (prefab != null) {
                var go = Instantiate(prefab.gameObject);
                return Instance;
            }

            // 尝试从 Resources 加载更通用的路径
            prefab = Resources.Load<StoryManager>("StoryManager");
            if (prefab != null) {
                var go = Instantiate(prefab.gameObject);
                return Instance;
            }

            Debug.LogError("[StoryManager] No StoryManager found in scene and cannot auto-instantiate. " +
                "Please add StoryManager.prefab to your scene or call SetAutoInstantiate(true, prefab).");
            return null;
        }

        // ==================== 实例方法（供静态方法调用） ====================

        void PlayStoryInternal(string storyFileName, string knot, Action onComplete, Action onStart = null) {
            if (_storyPlayer == null) {
                Debug.LogError("[StoryManager] InkStoryPlayer not found!");
                return;
            }

            // 加载剧情文件
            TextAsset inkJson = StoryLoader.Instance.LoadInkJson(storyFileName);
            if (inkJson == null) {
                Debug.LogError($"[StoryManager] Failed to load story: '{storyFileName}'");
                return;
            }

            // 设置回调
            _pendingOnStart = onStart;
            _pendingOnComplete = onComplete;

            _storyPlayer.inkJsonAsset = inkJson;

            if (_pauseGameDuringStory) {
                Time.timeScale = 0f;
            }

            _storyPlayer.Play(knot);
        }

        private Action _pendingOnStart;
        private Action _pendingOnComplete;

        void HandleStoryStart() {
            _pendingOnStart?.Invoke();
            OnStoryStarted?.Invoke();
            _pendingOnStart = null;
        }

        void HandleStoryEnd() {
            Time.timeScale = 1f;
            _pendingOnComplete?.Invoke();
            OnStoryEnded?.Invoke();
            _pendingOnComplete = null;
        }

        /// <summary>
        /// 直接使用 TextAsset 播放剧情（实例方法）
        /// </summary>
        /// <param name="inkJson">Ink JSON 资源</param>
        /// <param name="knot">起始节点（可选）</param>
        public void PlayStory(TextAsset inkJson, string knot = "") {
            if (_storyPlayer == null) {
                Debug.LogError("[StoryManager] InkStoryPlayer not found!");
                return;
            }

            if (inkJson != null) {
                _storyPlayer.inkJsonAsset = inkJson;
            }

            if (_pauseGameDuringStory) {
                Time.timeScale = 0f;
            }

            _storyPlayer.Play(knot);
        }

        void StopStory() {
            if (_storyPlayer != null) {
                _storyPlayer.Stop();
            }
            Time.timeScale = 1f;
        }

        void DoContinue() {
            if (_storyPlayer != null) {
                _storyPlayer.OnInteract();
            }
        }

        void DoSelectChoice(int index) {
            if (_storyPlayer != null) {
                _storyPlayer.SelectChoice(index);
            }
        }

        void DoShowCharacter(string characterId, string expression,
                float screenWidthPercent, float screenBottomPercent) {
            _characterManager?.ShowCharacter(characterId, expression, screenWidthPercent, screenBottomPercent);
        }

        void DoHideCharacter(string characterId) {
            if (string.IsNullOrEmpty(characterId)) {
                _characterManager?.HideAllCharacters();
            } else {
                _characterManager?.HideCharacter(characterId);
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Test Play Story (Static API)")]
        void TestPlayStoryStatic() {
            Play("SampleStory");
        }

        [ContextMenu("Test Play Story with Callback")]
        void TestPlayStoryWithCallback() {
            Play("SampleStory", () => Debug.Log("Story started!"), () => Debug.Log("Story ended!"));
        }

        [ContextMenu("Test Play Story with Knot")]
        void TestPlayStoryWithKnot() {
            Play("SampleStory", "check_glitter");
        }
#endif
    }
}
