using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Story {

    /// <summary>
    /// 角色立绘数据组件（只存储表情配置，配合 StoryCharacterManager 使用）
    /// </summary>
    public class StoryCharacter : MonoBehaviour {
        [Header("角色ID（与 Ink #ch tag 对应）")]
        public string characterId;

        [Header("显示名字（对话框中的说话者名）")]
        public string displayName;

        [Header("默认表情立绘")]
        public Sprite defaultSprite;

        [Header("表情列表")]
        public List<ExpressionEntry> expressions = new List<ExpressionEntry>();

        [Serializable]
        public class ExpressionEntry {
            public string name = "默认";
            public Sprite sprite;
        }
    }
}