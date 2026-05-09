using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Story {

    /// <summary>
    /// 剧情对话框面板
    /// 负责显示对话文字、说话者名字、继续提示
    /// </summary>
    public class StoryDialoguePanel : MonoBehaviour {
        [Header("面板根节点（Show/Hide会切换此对象的active）")]
        public GameObject root;

        [Header("说话者名字")]
        public Text speakerText;
        public TMP_Text speakerTextPro;

        [Header("对话文字")]
        public Text dialogueText;
        public TMP_Text dialogueTextPro;

        [Header("继续提示")]
        public Image continueIndicator;

        /// <summary>显示面板</summary>
        public void Show() {
            if (root != null) root.SetActive(true);
        }

        /// <summary>隐藏面板</summary>
        public void Hide() {
            if (root != null) root.SetActive(false);
        }

        /// <summary>设置说话者名字</summary>
        public void SetSpeaker(string name) {
            Debug.Log($"[StoryDialoguePanel] SetSpeaker called: name='{name}', speakerText={(speakerText != null ? "exists" : "null")}, speakerTextPro={(speakerTextPro != null ? "exists" : "null")}");
            if (speakerText != null) {
                speakerText.text = name ?? "";
                speakerText.gameObject.SetActive(!string.IsNullOrEmpty(name));
                Debug.Log($"[StoryDialoguePanel] SetSpeaker: speakerText.text='{speakerText.text}', gameObject.activeSelf={speakerText.gameObject.activeSelf}, root.activeSelf={(root != null ? root.activeSelf : "null")}");
            }
            if (speakerTextPro != null) {
                speakerTextPro.text = name ?? "";
                speakerTextPro.gameObject.SetActive(!string.IsNullOrEmpty(name));
                Debug.Log($"[StoryDialoguePanel] SetSpeaker: speakerTextPro.text='{speakerTextPro.text}', gameObject.activeSelf={speakerTextPro.gameObject.activeSelf}");
            }
            if (!string.IsNullOrEmpty(name)) {
                Debug.Log($"[StoryDialoguePanel] SetSpeaker: name is not empty — ensure root is active. root={(root != null ? root.activeSelf.ToString() : "null")}");
            }
        }

        /// <summary>设置对话文字</summary>
        public void SetText(string text) {
            if (dialogueText != null) dialogueText.text = text ?? "";
            if (dialogueTextPro != null) dialogueTextPro.text = text ?? "";
        }

        /// <summary>设置继续提示显示状态（打字完成后显示）</summary>
        public void SetContinueIndicator(bool visible) {
            if (continueIndicator != null) continueIndicator.gameObject.SetActive(visible);
        }
    }
}
