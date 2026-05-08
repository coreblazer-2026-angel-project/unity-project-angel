using UnityEngine;
using System;

namespace Game.Story {

    /// <summary>
    /// 剧情系统管理器
    /// 作为剧情模块的入口点，提供统一的 API 给其他系统调用
    /// 
    /// 使用方式：
    /// 1. 将 StoryManager.prefab 拖入场景（只需一个）
    /// 2. 配置 InkStoryPlayer 的默认剧情资源
    /// 3. 其他脚本通过 StoryManager API 触发剧情
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

            if (_storyPlayer != null) {
                _storyPlayer.dialoguePanel = _dialoguePanel;
                _storyPlayer.choicePanel = _choicePanel;
            }

            // 订阅事件
            if (_storyPlayer != null) {
                _storyPlayer.OnStoryStart += HandleStoryStart;
                _storyPlayer.OnStoryEnd += HandleStoryEnd;
            }
        }

        // ==================== 公共 API ====================

        /// <summary>
        /// 开始剧情
        /// </summary>
        /// <param name="inkJson">Ink JSON 资源（可选，为空使用默认配置）</param>
        /// <param name="knot">从哪个节点开始（可选，默认 start）</param>
        public void PlayStory(TextAsset inkJson = null, string knot = "") {
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

        /// <summary>
        /// 用 Resources 路径开始剧情。
        /// 示例：PlayStory("Story/story1", "start")。
        /// 传入 "story1.json" 时会自动去掉 .json 后缀。
        /// </summary>
        public void PlayStory(string storyResourcePath, string knot = "") {
            if (string.IsNullOrWhiteSpace(storyResourcePath)) {
                PlayStory((TextAsset)null, knot);
                return;
            }

            string path = NormalizeResourcePath(storyResourcePath);
            TextAsset inkJson = LoadStoryResource(path);
            if (inkJson == null) {
                Debug.LogError($"[StoryManager] Story resource not found: {storyResourcePath}. Put the ink json under Assets/Resources/Story and call PlayStory(\"{path}\", \"{knot}\").");
                return;
            }

            PlayStory(inkJson, knot);
        }

        TextAsset LoadStoryResource(string path) {
            TextAsset inkJson = Resources.Load<TextAsset>(path);
            if (inkJson != null)
                return inkJson;

            if (!path.Contains("/"))
                inkJson = Resources.Load<TextAsset>($"Story/{path}");

            return inkJson;
        }

        string NormalizeResourcePath(string path) {
            path = path.Replace('\\', '/').Trim();

            const string resourcesPrefix = "Resources/";
            int resourcesIndex = path.IndexOf(resourcesPrefix, StringComparison.OrdinalIgnoreCase);
            if (resourcesIndex >= 0)
                path = path.Substring(resourcesIndex + resourcesPrefix.Length);

            if (path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                path = path.Substring("Assets/".Length);

            if (path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                path = path.Substring(0, path.Length - ".json".Length);

            return path;
        }

        /// <summary>
        /// 停止剧情
        /// </summary>
        public void StopStory() {
            if (_storyPlayer != null) {
                _storyPlayer.Stop();
            }

            Time.timeScale = 1f;
        }

        /// <summary>
        /// 继续剧情（模拟点击）
        /// </summary>
        public void Continue() {
            if (_storyPlayer != null) {
                _storyPlayer.OnInteract();
            }
        }

        /// <summary>
        /// 选择选项
        /// </summary>
        public void SelectChoice(int index) {
            if (_storyPlayer != null) {
                _storyPlayer.SelectChoice(index);
            }
        }

        /// <summary>
        /// 显示指定角色立绘
        /// </summary>
        public void ShowCharacter(string characterId, string expression = "", 
                float screenWidthPercent = 0.4f, float screenBottomPercent = 0.15f) {
            _characterManager?.ShowCharacter(characterId, expression, screenWidthPercent, screenBottomPercent);
        }

        /// <summary>
        /// 隐藏角色立绘
        /// </summary>
        public void HideCharacter(string characterId = "") {
            if (string.IsNullOrEmpty(characterId)) {
                _characterManager?.HideAllCharacters();
            } else {
                _characterManager?.HideCharacter(characterId);
            }
        }

        // ==================== 事件处理 ====================

        void HandleStoryStart() {
            OnStoryStarted?.Invoke();
        }

        void HandleStoryEnd() {
            Time.timeScale = 1f;
            OnStoryEnded?.Invoke();
        }

#if UNITY_EDITOR
        [ContextMenu("Test Play Story")]
        void TestPlayStory() {
            PlayStory();
        }
#endif
    }
}
