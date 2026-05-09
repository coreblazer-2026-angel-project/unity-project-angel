using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Story {

    public class StoryDialoguePanel : MonoBehaviour {
        [Header("面板根节点（Show/Hide会切换此对象的active）")]
        public GameObject root;

        [Header("说话者名字")]
        public Text speakerText;
        public TMP_Text speakerTextPro;

        [Header("对话文字")]
        public Text dialogueText;
        public TMP_Text dialogueTextPro;

        [Header("对话框背景（自适应高度用）")]
        public RectTransform backgroundRect;
        public float minHeight = 120f;
        public float paddingTopBottom = 40f;

        [Header("继续提示")]
        public Image continueIndicator;

        public void Show() {
            if (root != null) root.SetActive(true);
        }

        public void Hide() {
            if (root != null) root.SetActive(false);
        }

        public void SetSpeaker(string name) {
            if (speakerText != null) {
                speakerText.text = name ?? "";
                speakerText.gameObject.SetActive(!string.IsNullOrEmpty(name));
            }
            if (speakerTextPro != null) {
                speakerTextPro.text = name ?? "";
                speakerTextPro.gameObject.SetActive(!string.IsNullOrEmpty(name));
            }
        }

        public void SetText(string text) {
            if (dialogueText != null) dialogueText.text = text ?? "";
            if (dialogueTextPro != null) dialogueTextPro.text = text ?? "";
            ResizeBackground();
        }

        public void SetContinueIndicator(bool visible) {
            if (continueIndicator != null) continueIndicator.gameObject.SetActive(visible);
        }

        void ResizeBackground() {
            if (backgroundRect == null) return;

            float preferredH = 0f;
            if (dialogueTextPro != null) {
                dialogueTextPro.ForceMeshUpdate();
                preferredH = dialogueTextPro.renderedHeight;
            } else if (dialogueText != null) {
                var generator = dialogueText.cachedTextGenerator;
                var settings = dialogueText.GetGenerationSettings(dialogueText.rectTransform.rect.size);
                generator.Populate(dialogueText.text, settings);
                preferredH = generator.GetPreferredHeight(dialogueText.text, settings);
            }

            float targetH = Mathf.Max(minHeight, preferredH + paddingTopBottom);
            backgroundRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetH);
        }
    }
}
