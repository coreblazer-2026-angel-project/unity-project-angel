using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using InkChoice = Ink.Runtime.Choice;
using InkStory = Ink.Runtime.Story;

namespace Game.Story {

    /// <summary>
    /// 基于 Ink 的剧情播放器
    /// 读取 Ink JSON Asset，驱动对话框和立绘
    ///
    /// Ink 语法约定：
    ///   独立 tag 行:  #ch player   （显示/切换角色）
    ///                 #expr 惊喜    （切换表情）
    ///                 #hide player  （隐藏角色）
    ///                 #choice       （强制显示选项）
    ///   角色发言:     player: 台词
    ///   旁白:         台词（无角色名前缀）
    ///   选项:         * 选项文字
    ///   跳转:         -> knot_name
    /// </summary>
    public class InkStoryPlayer : MonoBehaviour {
        public static InkStoryPlayer Instance { get; private set; }

        [Header("Ink JSON Asset（由 Ink 插件自动编译生成）")]
        public TextAsset inkJsonAsset;

        [Header("UI")]
        public StoryDialoguePanel dialoguePanel;
        public StoryChoicePanel choicePanel;

        [Header("打字机（设为0禁用）")]
        public float typewriterSpeed = 0.04f;

        // Runtime state
        private InkStory _story;
        private bool _isTyping;
        private Coroutine _typewriterCoroutine;
        private string _currentFullText;
        private bool _waitingForChoice;
        private bool _actionPlaying;
        private StoryActionPlayer _currentActionPlayer;
        private List<InkChoice> _currentChoices = new List<InkChoice>();

        public event Action OnStoryStart;
        public event Action OnStoryEnd;

        public bool IsPlaying { get; private set; }

        void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void ResolveReferences() {
            if (dialoguePanel == null) dialoguePanel = FindObjectOfType<StoryDialoguePanel>();
            if (choicePanel == null) choicePanel = FindObjectOfType<StoryChoicePanel>();
        }

        void Update() {
            if (!IsPlaying) return;
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {
                OnInteract();
            }
        }

        void OnGUI() {
            Event e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.T && !IsPlaying) {
                Debug.Log("[InkStoryPlayer] Play button pressed");
                Play();
            }
        }

        /// <summary>开始剧情</summary>
        public void Play(string knot = "") {
            if (inkJsonAsset == null) {
                Debug.LogError("[InkStoryPlayer] inkJsonAsset is not assigned!");
                return;
            }

            ResolveReferences();
            StopTypewriter();

            try {
                _story = new InkStory(inkJsonAsset.text);
            } catch (Exception ex) {
                Debug.LogError($"[InkStoryPlayer] Failed to parse Ink JSON: {ex.Message}");
                return;
            }

            IsPlaying = true;
            _waitingForChoice = false;
            _currentChoices.Clear();
            OnStoryStart?.Invoke();

            if (dialoguePanel != null) dialoguePanel.Show();

            string targetKnot = string.IsNullOrEmpty(knot) ? "start" : knot;
            _story.ChoosePathString(targetKnot);

            Debug.Log("[InkStoryPlayer] Story started.");
            Advance();
        }

        /// <summary>停止剧情</summary>
        public void Stop() {
            IsPlaying = false;
            _waitingForChoice = false;
            _currentChoices.Clear();
            StopTypewriter();
            if (dialoguePanel != null) dialoguePanel.Hide();
            if (choicePanel != null) choicePanel.Hide();
            StoryCharacterManager.Instance?.HideAllCharacters();
            Debug.Log("[InkStoryPlayer] Story ended.");
            OnStoryEnd?.Invoke();
        }

        /// <summary>点击/空格/回车 继续</summary>
        public void OnInteract() {
            if (_waitingForChoice) return;
            if (_isTyping) {
                SkipTypewriter();
            } else if (_actionPlaying) {
                SkipCurrentAction();
            } else {
                Advance();
            }
        }

        /// <summary>跳过当前正在播放的动作</summary>
        private void SkipCurrentAction() {
            if (_currentActionPlayer != null) {
                _currentActionPlayer.Stop();
                _actionPlaying = false;
                _currentActionPlayer = null;
                Debug.Log("[InkStoryPlayer] Action skipped by user.");
            }
        }

        /// <summary>选择选项</summary>
        public void SelectChoice(int index) {
            if (!_waitingForChoice || index < 0 || index >= _currentChoices.Count) return;
            _waitingForChoice = false;
            if (choicePanel != null) choicePanel.Hide();
            _story.ChooseChoiceIndex(index);
            Advance();
        }

        void Advance() {
            if (_actionPlaying) return;
            if (!_story.canContinue) {
                if (_story.currentChoices.Count > 0) {
                    ShowChoices(_story.currentChoices);
                    return;
                }
                Stop();
                return;
            }

            string rawLine = _story.Continue().Trim();
            ProcessLine(rawLine);
        }

        void ProcessLine(string line) {
            if (string.IsNullOrEmpty(line)) {
                // 空行直接前进，但限制递归深度防止无限循环
                if (_story.canContinue) {
                    _story.Continue();
                    ProcessLine(_story.currentText?.Trim() ?? "");
                }
                return;
            }

            string speaker = "";
            string text = line;

            // Ink JSON 的文本内容以 ^ 开头，需要去掉
            if (text.StartsWith("^")) text = text.Substring(1);

            // 解析 speaker: 格式
            if (line.StartsWith("^") || text.Contains(":")) {
                string raw = line.StartsWith("^") ? line.Substring(1) : line;
                if (raw.Contains(":")) {
                    int idx = raw.IndexOf(':');
                    string head = raw.Substring(0, idx).Trim();
                    if (!head.StartsWith("->") && !head.StartsWith("*")) {
                        speaker = head;
                        text = raw.Substring(idx + 1).Trim();
                    }
                }
            }

            Debug.Log($"[InkStoryPlayer] ProcessLine: raw='{line}', speaker='{speaker}', text='{text}', dialoguePanel={(dialoguePanel != null ? "exists" : "null")}");
            Debug.Log($"[InkStoryPlayer] SetSpeaker: speakerText={(dialoguePanel?.speakerText != null ? "exists" : "null")}, speakerTextPro={(dialoguePanel?.speakerTextPro != null ? "exists" : "null")}");

            // 读取当前行的 Ink tag（用于驱动立绘/表情）
            var tags = _story.currentTags ?? new List<string>();
            string characterId = "";
            string exprName = "";

            // 解析 #ch 标签，支持格式: #ch 角色ID, 高度比例, 底部偏移(像素), 水平偏移(像素)
            float heightPercent = 0.8f;
            float bottomOffset = 20f;
            float horizontalOffset = 0f;

            foreach (var tag in tags) {
                if (tag.StartsWith("ch ")) {
                    characterId = tag.Substring(3).Trim();
                    string[] parts = characterId.Split(',');
                    if (parts.Length >= 1) characterId = parts[0].Trim();
                    if (parts.Length >= 2 && float.TryParse(parts[1].Trim(), out float h)) heightPercent = h;
                    if (parts.Length >= 3 && float.TryParse(parts[2].Trim(), out float b)) bottomOffset = b;
                    if (parts.Length >= 4 && float.TryParse(parts[3].Trim(), out float ho)) horizontalOffset = ho;
                }
                else if (tag.StartsWith("expr ")) exprName = tag.Substring(5).Trim();
                else if (tag.StartsWith("action ")) {
                    string actionStr = tag.Substring(7).Trim();
                    Debug.Log($"[InkStoryPlayer] ProcessLine: found action tag '{actionStr}'");
                    ProcessActionTag(actionStr);
                }
                else if (tag == "choice") {
                    ShowChoices(_story.currentChoices);
                    return;
                }
                else if (tag == "hide") {
                    string id = tag.Length > 5 ? tag.Substring(5).Trim() : "";
                    if (!string.IsNullOrEmpty(id)) StoryCharacterManager.Instance?.HideCharacter(id);
                    if (_story.canContinue) {
                        _story.Continue();
                        ProcessLine(_story.currentText?.Trim() ?? "");
                    } else if (_story.currentChoices.Count > 0) {
                        ShowChoices(_story.currentChoices);
                    } else {
                        Stop();
                    }
                    return;
                }
            }

            // 跳过纯 tag 行（继续下一行）
            if (IsPureTagLine(line, speaker, text)) {
                if (_story.canContinue) {
                    _story.Continue();
                    ProcessLine(_story.currentText?.Trim() ?? "");
                } else if (_story.currentChoices.Count > 0) {
                    ShowChoices(_story.currentChoices);
                } else {
                    Stop();
                }
                return;
            }

            // 隐藏角色（#-player）
            if (characterId.StartsWith("-")) {
                string id = characterId.Substring(1);
                StoryCharacterManager.Instance?.HideCharacter(id);
                if (_story.canContinue) {
                    _story.Continue();
                    ProcessLine(_story.currentText?.Trim() ?? "");
                } else if (_story.currentChoices.Count > 0) {
                    ShowChoices(_story.currentChoices);
                } else {
                    Stop();
                }
                return;
            }

            // 显示/切换角色
            if (!string.IsNullOrEmpty(characterId)) {
                Debug.Log($"[InkStoryPlayer] ProcessLine: calling ShowCharacter('{characterId}', '{exprName}')");
                StoryCharacterManager.Instance?.ShowCharacter(characterId, exprName, heightPercent, bottomOffset, horizontalOffset);
                // Ink 用 #ch tag 标识说话者，从角色ID查到 display name 作为 speaker
                if (string.IsNullOrEmpty(speaker)) {
                    speaker = StoryCharacterManager.Instance?.GetCharacterDisplayName(characterId) ?? characterId;
                }
            }

            // 显示文字
            _currentFullText = text;
            if (dialoguePanel != null) {
                dialoguePanel.SetSpeaker(speaker);
                dialoguePanel.SetText("");
                dialoguePanel.SetContinueIndicator(false);
            }

            StartTypewriter();
        }

        void StartTypewriter() {
            if (typewriterSpeed <= 0f) {
                if (dialoguePanel != null) dialoguePanel.SetText(_currentFullText);
                _isTyping = false;
                return;
            }
            StopTypewriter();
            _typewriterCoroutine = StartCoroutine(Typewriter());
        }

        void StopTypewriter() {
            if (_typewriterCoroutine != null) {
                StopCoroutine(_typewriterCoroutine);
                _typewriterCoroutine = null;
            }
            _isTyping = false;
        }

        void SkipTypewriter() {
            StopTypewriter();
            if (dialoguePanel != null) {
                dialoguePanel.SetText(_currentFullText);
                dialoguePanel.SetContinueIndicator(true);
            }
        }

        IEnumerator Typewriter() {
            _isTyping = true;
            string display = "";
            if (dialoguePanel != null) dialoguePanel.SetText("");

            foreach (char c in _currentFullText) {
                display += c;
                if (dialoguePanel != null) dialoguePanel.SetText(display);
                yield return new WaitForSecondsRealtime(typewriterSpeed);
            }

            _isTyping = false;
            _typewriterCoroutine = null;
            if (dialoguePanel != null) dialoguePanel.SetContinueIndicator(true);
        }

        void ShowChoices(List<InkChoice> choices) {
            Debug.Log($"[InkStoryPlayer] ShowChoices called with {choices.Count} choices");
            _waitingForChoice = true;
            _currentChoices = choices;

            if (dialoguePanel != null) {
                dialoguePanel.SetText("");
                dialoguePanel.SetSpeaker("");
                dialoguePanel.SetContinueIndicator(false);
            }

            if (choicePanel != null) {
                choicePanel.Show(choices, this);
            } else {
                for (int i = 0; i < choices.Count; i++) {
                    Debug.Log($"[Choice {i + 1}] {choices[i].text}");
                }
            }
        }

        /// <summary>判断一行是否为纯 tag 行（没有实质对话内容）</summary>
        bool IsPureTagLine(string rawLine, string speaker, string text) {
            var pureTagPattern = new Regex(@"^#\w+(\s+\S+)?$");
            if (pureTagPattern.IsMatch(rawLine)) return true;
            if (speaker.Length > 0 && speaker[0] == '#') return true;
            if (string.IsNullOrEmpty(text) || text.Trim() == "#" + speaker) return true;
            return false;
        }

        string[] GetActiveCharacterIds() {
            var ids = new System.Collections.Generic.List<string>();
            if (StoryCharacterManager.Instance == null) return ids.ToArray();
            foreach (var p in StoryCharacterManager.Instance.presets) {
                if (p.image != null && p.image.enabled) ids.Add(p.characterId);
            }
            return ids.ToArray();
        }

        /// <summary>
        /// 处理动作标签
        /// 格式: #action jump, #action jump_3, #action enter_left, #action flash_0.5
        /// 支持: jump, bounce, shake, flash, pulse, enter_left, enter_right, exit_left, exit_right, lean_left, lean_right, fadein, fadeout
        /// </summary>
        void ProcessActionTag(string actionStr) {
            if (StoryCharacterManager.Instance == null) {
                Debug.LogWarning("[InkStoryPlayer] StoryCharacterManager.Instance is null!");
                return;
            }

            // 解析动作名称和参数
            // 支持两种格式：
            //   jump, bounce, flash, shake       → 单动作名
            //   bounce_2, flash_0.5            → 动作名_强度
            //   lean_left, enter_right          → 复合动作名（不拆开）
            string[] parts = actionStr.Split('_');
            string actionName = parts[0].ToLower().Trim();
            float intensity = 1f;

            if (parts.Length >= 2) {
                // 如果第二个下划线部分是数字，就是强度；否则是复合动作名
                if (float.TryParse(parts[1], out float parsed)) {
                    intensity = parsed;
                } else if (parts.Length == 2) {
                    // 复合动作名（如 lean_left），还原为完整名称
                    actionName = actionStr.ToLower().Trim();
                }
                // parts.Length > 2 时忽略额外部分
            }

            // 获取当前显示的角色 preset
            var preset = StoryCharacterManager.Instance.GetActivePreset();
            Debug.Log($"[InkStoryPlayer] ProcessActionTag: '{actionName}', preset={(preset != null ? preset.characterId : "null")}, image={(preset?.image != null ? "exists" : "null")}");
            if (preset?.image == null) {
                Debug.LogWarning($"[InkStoryPlayer] No active character to perform action: {actionName}. " +
                    $"Active presets: {string.Join(", ", GetActiveCharacterIds())}");
                return;
            }

            // 确保角色有 StoryActionPlayer 组件
            var actionPlayer = preset.image.GetComponent<StoryActionPlayer>();
            if (actionPlayer == null) {
                actionPlayer = preset.image.gameObject.AddComponent<StoryActionPlayer>();
                actionPlayer.targetImage = preset.image;
            }

            // 根据动作名称执行对应动作
            ActionParams action = ActionParams.Default;
            action.intensity = intensity;

            switch (actionName) {
                // 基础动作
                case "jump":
                    action.type = CharacterActionType.Jump;
                    action.duration = 0.4f;
                    break;
                case "bounce":
                    action.type = CharacterActionType.Bounce;
                    action.duration = 0.5f;
                    break;
                case "shake":
                    action.type = CharacterActionType.Shake;
                    action.duration = 0.3f;
                    break;
                case "flash":
                    action.type = CharacterActionType.Flash;
                    action.duration = 0.15f;
                    break;
                case "pulse":
                    action.type = CharacterActionType.Pulse;
                    action.duration = 0.6f;
                    break;
                // 移动动作
                case "enter_left":
                    action.type = CharacterActionType.EnterFromLeft;
                    action.duration = 0.5f;
                    break;
                case "enter_right":
                    action.type = CharacterActionType.EnterFromRight;
                    action.duration = 0.5f;
                    break;
                case "exit_left":
                    action.type = CharacterActionType.ExitToLeft;
                    action.duration = 0.5f;
                    break;
                case "exit_right":
                    action.type = CharacterActionType.ExitToRight;
                    action.duration = 0.5f;
                    break;
                // 倾斜动作
                case "lean":
                case "lean_left":
                case "tilt_left":
                    action.type = CharacterActionType.LeanLeft;
                    action.duration = 0.3f;
                    break;
                case "lean_right":
                case "tilt_right":
                    action.type = CharacterActionType.LeanRight;
                    action.duration = 0.3f;
                    break;
                // 淡入淡出
                case "fadein":
                case "fade_in":
                    action.type = CharacterActionType.FadeIn;
                    action.duration = 0.3f;
                    break;
                case "fadeout":
                case "fade_out":
                    action.type = CharacterActionType.FadeOut;
                    action.duration = 0.3f;
                    break;
                default:
                    Debug.LogWarning($"[InkStoryPlayer] Unknown action: {actionName}");
                    return;
            }

            Debug.Log($"[InkStoryPlayer] Play action: {action.type}, intensity={intensity}");
            _actionPlaying = true;
            _currentActionPlayer = actionPlayer;
            actionPlayer.Play(action, () => {
                _actionPlaying = false;
                _currentActionPlayer = null;
            });
        }

        public InkStory GetStory() => _story;
    }
}
