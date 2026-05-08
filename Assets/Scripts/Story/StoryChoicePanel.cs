using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using InkChoice = Ink.Runtime.Choice;

namespace Game.Story {

    /// <summary>
    /// 选项面板：显示在对话框下方的选项列表
    /// </summary>
    public class StoryChoicePanel : MonoBehaviour {
        [Header("根节点（Show/Hide 切换此对象）")]
        public GameObject root;

        [Header("选项列表容器（VerticalLayoutGroup）")]
        public GameObject choiceContainer;

        [Header("选项预设（每个选项使用的 Button 预设）")]
        public Button choiceButtonPrefab;

        [Header("选项文本（支持 Text 或 TMP）")]
        public Text choiceText;
        public TMP_Text choiceTextPro;

        [Header("布局")]
        [Tooltip("每个选项之间的间距")]
        public float spacing = 8f;

        void Start() {
            Hide();
        }

        /// <summary>显示选项</summary>
        public void Show(List<InkChoice> choices, InkStoryPlayer player) {
            if (root == null) return;
            root.SetActive(true);
            ClearChoices();

            for (int i = 0; i < choices.Count; i++) {
                CreateChoiceButton(choices[i], i, player);
            }
        }

        /// <summary>隐藏选项</summary>
        public void Hide() {
            if (root != null) root.SetActive(false);
            ClearChoices();
        }

        void ClearChoices() {
            if (choiceContainer == null) return;
            foreach (Transform child in choiceContainer.transform) {
                Destroy(child.gameObject);
            }
        }

        void CreateChoiceButton(InkChoice choice, int index, InkStoryPlayer player) {
            Button btn = Instantiate(choiceButtonPrefab, choiceContainer.transform);
            btn.onClick.RemoveAllListeners();
            int capturedIndex = index;
            btn.onClick.AddListener(() => player.SelectChoice(capturedIndex));

            // 设置选项文字
            var texts = btn.GetComponentsInChildren<UnityEngine.UI.Text>();
            var tmpTexts = btn.GetComponentsInChildren<TMP_Text>();
            if (texts.Length > 0) texts[0].text = choice.text;
            else if (tmpTexts.Length > 0) tmpTexts[0].text = choice.text;
        }
    }
}
