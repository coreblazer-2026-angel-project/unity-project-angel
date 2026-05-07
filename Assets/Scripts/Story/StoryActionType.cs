using System;
using UnityEngine;

namespace Game.Story {

    /// <summary>
    /// 角色动作类型枚举
    /// </summary>
    public enum CharacterActionType {
        None,
        // 基础动作
        Jump,           // 跳跃（上下跳动）
        Bounce,         // 弹性缩放
        Shake,          // 左右摇晃
        FadeIn,         // 淡入
        FadeOut,        // 淡出
        // 移动动作
        EnterFromLeft,  // 从屏幕左侧进入
        EnterFromRight, // 从屏幕右侧进入
        ExitToLeft,     // 向左离开屏幕
        ExitToRight,    // 向右离开屏幕
        // 特效动作
        Flash,          // 高亮闪烁（强调）
        Pulse,          // 脉冲缩放
        LeanLeft,       // 向左倾斜
        LeanRight,      // 向右倾斜
    }

    /// <summary>
    /// 动作参数结构
    /// </summary>
    [Serializable]
    public struct ActionParams {
        public CharacterActionType type;
        public float duration;      // 动作持续时间（秒）
        public float intensity;      // 动作强度/幅度
        public float delay;         // 延迟执行时间（秒）
        public bool loop;           // 是否循环
        public Action onComplete;   // 完成回调

        public static ActionParams Default => new ActionParams {
            type = CharacterActionType.None,
            duration = 0.5f,
            intensity = 1f,
            delay = 0f,
            loop = false
        };

        /// <summary>快速创建跳跃动作</summary>
        public static ActionParams Jump(float intensity = 1f) => new ActionParams {
            type = CharacterActionType.Jump,
            duration = 0.4f,
            intensity = intensity,
            delay = 0f,
            loop = false
        };

        /// <summary>快速创建弹跳动作</summary>
        public static ActionParams Bounce(float intensity = 1f) => new ActionParams {
            type = CharacterActionType.Bounce,
            duration = 0.5f,
            intensity = intensity,
            delay = 0f,
            loop = false
        };

        /// <summary>快速创建摇晃动作</summary>
        public static ActionParams Shake(float intensity = 1f) => new ActionParams {
            type = CharacterActionType.Shake,
            duration = 0.3f,
            intensity = intensity,
            delay = 0f,
            loop = false
        };

        /// <summary>快速创建淡入动作</summary>
        public static ActionParams FadeIn(float duration = 0.3f) => new ActionParams {
            type = CharacterActionType.FadeIn,
            duration = duration,
            intensity = 1f,
            delay = 0f,
            loop = false
        };

        /// <summary>快速创建淡出动作</summary>
        public static ActionParams FadeOut(float duration = 0.3f) => new ActionParams {
            type = CharacterActionType.FadeOut,
            duration = duration,
            intensity = 1f,
            delay = 0f,
            loop = false
        };

        /// <summary>快速创建从左侧进入</summary>
        public static ActionParams EnterFromLeft(float duration = 0.5f) => new ActionParams {
            type = CharacterActionType.EnterFromLeft,
            duration = duration,
            intensity = 1f,
            delay = 0f,
            loop = false
        };

        /// <summary>快速创建从右侧进入</summary>
        public static ActionParams EnterFromRight(float duration = 0.5f) => new ActionParams {
            type = CharacterActionType.EnterFromRight,
            duration = duration,
            intensity = 1f,
            delay = 0f,
            loop = false
        };

        /// <summary>快速创建向左离开</summary>
        public static ActionParams ExitToLeft(float duration = 0.5f) => new ActionParams {
            type = CharacterActionType.ExitToLeft,
            duration = duration,
            intensity = 1f,
            delay = 0f,
            loop = false
        };

        /// <summary>快速创建向右离开</summary>
        public static ActionParams ExitToRight(float duration = 0.5f) => new ActionParams {
            type = CharacterActionType.ExitToRight,
            duration = duration,
            intensity = 1f,
            delay = 0f,
            loop = false
        };

        /// <summary>快速创建高亮闪烁</summary>
        public static ActionParams Flash() => new ActionParams {
            type = CharacterActionType.Flash,
            duration = 0.15f,
            intensity = 1f,
            delay = 0f,
            loop = false
        };

        /// <summary>快速创建脉冲效果</summary>
        public static ActionParams Pulse(float intensity = 1f) => new ActionParams {
            type = CharacterActionType.Pulse,
            duration = 0.6f,
            intensity = intensity,
            delay = 0f,
            loop = false
        };

        /// <summary>快速创建左倾</summary>
        public static ActionParams LeanLeft(float duration = 0.3f) => new ActionParams {
            type = CharacterActionType.LeanLeft,
            duration = duration,
            intensity = 1f,
            delay = 0f,
            loop = false
        };

        /// <summary>快速创建右倾</summary>
        public static ActionParams LeanRight(float duration = 0.3f) => new ActionParams {
            type = CharacterActionType.LeanRight,
            duration = duration,
            intensity = 1f,
            delay = 0f,
            loop = false
        };
    }
}
