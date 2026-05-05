using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Story {

    /// <summary>
    /// 剧情对话条目
    /// character: 角色ID（对应 StoryCharacter.characterId），留空表示旁白
    /// expression: 表情名称，留空表示不切换表情
    /// text: 对话或旁白文字
    /// </summary>
    [Serializable]
    public class StoryLine {
        public string character = "";
        public string expression = "";
        [TextArea(1, 4)]
        public string text = "";
    }

    /// <summary>
    /// 剧情数据：包含一个完整的对话序列
    /// </summary>
    [CreateAssetMenu(fileName = "NewStory", menuName = "Game/Story/Story Data")]
    public class StoryData : ScriptableObject {
        public List<StoryLine> lines = new List<StoryLine>();
    }

    /// <summary>
    /// 剧情播放器
    /// 管理对话流程、打字机效果、点击继续、表情切换
    /// </summary>
    public class StoryPlayer : MonoBehaviour {
        public static StoryPlayer Instance { get; private set; }

        [Header("剧情数据")]
        public StoryData storyData;

        [Header("UI")]
        public StoryDialoguePanel dialoguePanel;

        [Header("角色")]
        public List<StoryCharacter> characters = new List<StoryCharacter>();

        [Header("打字机")]
        [Tooltip("每个字符的等待秒数")]
        public float typewriterSpeed = 0.04f;

        // 内部状态
        private int _currentIndex = -1;
        private bool _isTyping;
        private Coroutine _typewriterCoroutine;
        private StoryLine _currentLine;

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

        void Update() {
            if (!IsPlaying) return;

            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {
                OnInteract();
            }
        }

        /// <summary>开始剧情（自动从头播放）</summary>
        public void Play() {
            if (storyData == null || storyData.lines.Count == 0) {
                Debug.LogWarning("[StoryPlayer] No story data assigned or story is empty.");
                return;
            }

            IsPlaying = true;
            _currentIndex = -1;
            OnStoryStart?.Invoke();

            if (dialoguePanel != null) dialoguePanel.Show();

            Advance();
        }

        /// <summary>停止并关闭剧情</summary>
        public void Stop() {
            IsPlaying = false;
            if (_typewriterCoroutine != null) {
                StopCoroutine(_typewriterCoroutine);
                _typewriterCoroutine = null;
            }
            if (dialoguePanel != null) dialoguePanel.Hide();
            OnStoryEnd?.Invoke();
        }

        /// <summary>用户点击/空格/回车</summary>
        public void OnInteract() {
            if (_isTyping) {
                SkipTypewriter();
            } else {
                Advance();
            }
        }

        void Advance() {
            _currentIndex++;
            if (_currentIndex >= storyData.lines.Count) {
                Stop();
                return;
            }

            _currentLine = storyData.lines[_currentIndex];
            DisplayLine(_currentLine);
        }

        void DisplayLine(StoryLine line) {
            // 切换角色表情
            if (!string.IsNullOrEmpty(line.character)) {
                var ch = FindCharacter(line.character);
                if (ch != null) {
                    if (!string.IsNullOrEmpty(line.expression)) {
                        ch.SetExpression(line.expression);
                    } else {
                        ch.Show();
                    }
                }
            }

            // 显示文字
            if (dialoguePanel != null) {
                dialoguePanel.SetText(line.text);
                dialoguePanel.SetSpeaker(string.IsNullOrEmpty(line.character) ? "" : line.character);
            }

            // 开始打字机
            StartTypewriter();
        }

        void StartTypewriter() {
            if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
            _typewriterCoroutine = StartCoroutine(Typewriter());
        }

        void SkipTypewriter() {
            if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
            _isTyping = false;
            _typewriterCoroutine = null;
            if (dialoguePanel != null) dialoguePanel.SetText(_currentLine.text);
        }

        IEnumerator Typewriter() {
            _isTyping = true;
            string fullText = _currentLine.text;
            string display = "";

            if (dialoguePanel != null) dialoguePanel.SetText("");

            foreach (char c in fullText) {
                display += c;
                if (dialoguePanel != null) dialoguePanel.SetText(display);
                yield return new WaitForSeconds(typewriterSpeed);
            }

            _isTyping = false;
            _typewriterCoroutine = null;
        }

        StoryCharacter FindCharacter(string id) {
            return characters.Find(c => c.characterId == id);
        }
    }
}
