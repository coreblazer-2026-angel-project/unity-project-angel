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

        [Header("对话框背景（留空则自动查找文字的父级 Image）")]
        public RectTransform backgroundRect;
        public float minHeight = 120f;
        public float paddingTopBottom = 60f;

        [Header("继续提示")]
        public Image continueIndicator;

        RectTransform _textRect;

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

            // 等一帧让文字先布局完再算高度
            if (_textRect == null) {
                if (dialogueTextPro != null) _textRect = dialogueTextPro.rectTransform;
                else if (dialogueText != null) _textRect = dialogueText.rectTransform;
            }
            if (_textRect != null) {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_textRect);
            }

            ResizeBackground();
        }

        public void SetContinueIndicator(bool visible) {
            if (continueIndicator != null) continueIndicator.gameObject.SetActive(visible);
        }

        void ResizeBackground() {
            // 自动查找：文字往上找第一个带 Image 的父物体
            if (backgroundRect == null && _textRect != null) {
                var parent = _textRect.parent;
                while (parent != null) {
                    if (parent.GetComponent<Image>() != null) {
                        backgroundRect = parent as RectTransform;
                        break;
                    }
                    parent = parent.parent;
                }
            }

            if (backgroundRect == null) return;

            float preferredH = 0f;
            if (dialogueTextPro != null) {
                dialogueTextPro.ForceMeshUpdate();
                preferredH = dialogueTextPro.GetPreferredValues(
                    dialogueTextPro.text,
                    dialogueTextPro.rectTransform.rect.width,
                    0f).y;
            } else if (dialogueText != null) {
                var settings = dialogueText.GetGenerationSettings(dialogueText.rectTransform.rect.size);
                preferredH = dialogueText.cachedTextGeneratorForLayout.GetPreferredHeight(dialogueText.text, settings) / dialogueText.pixelsPerUnit;
            }

            float targetH = Mathf.Max(minHeight, preferredH + paddingTopBottom);
            backgroundRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetH);
        }
    }
}
