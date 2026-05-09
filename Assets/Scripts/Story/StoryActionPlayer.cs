using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Story {

    /// <summary>
    /// 角色动作执行器
    /// 为角色立绘提供各种微动作效果：跳跃、摇晃、进入、离开等
    /// </summary>
    public class StoryActionPlayer : MonoBehaviour {
        [Header("目标图片")]
        public Image targetImage;

        [Header("缓动曲线")]
        public AnimationCurve defaultEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

        // 状态
        private bool _isPlaying = false;
        private Coroutine _currentCoroutine = null;

        // 默认位置（进入动画时记录）
        private Vector2 _defaultAnchoredPos;
        private Vector3 _defaultLocalScale = Vector3.one;
        private float _defaultRotation = 0f;

        void Awake() {
            if (targetImage == null) {
                targetImage = GetComponent<Image>();
            }
        }

        void Start() {
            RecordDefaultState();
        }

        /// <summary>
        /// 记录默认状态
        /// </summary>
        public void RecordDefaultState() {
            var rect = transform as RectTransform;
            if (rect != null) {
                _defaultAnchoredPos = rect.anchoredPosition;
            }
            _defaultLocalScale = transform.localScale;
            _defaultRotation = transform.localRotation.eulerAngles.z;
        }

        /// <summary>
        /// 执行动作
        /// </summary>
        public void Play(ActionParams action, Action onComplete = null) {
            Debug.Log($"[StoryActionPlayer] Play called: type={action.type}, intensity={action.intensity}, targetImage={(targetImage != null ? "exists" : "null")}");
            if (_currentCoroutine != null) {
                StopCoroutine(_currentCoroutine);
            }
            _currentCoroutine = StartCoroutine(PlayCoroutine(action, onComplete));
        }

        /// <summary>
        /// 执行动作序列
        /// </summary>
        public void PlaySequence(params ActionParams[] actions) {
            if (_currentCoroutine != null) {
                StopCoroutine(_currentCoroutine);
            }
            _currentCoroutine = StartCoroutine(PlaySequenceCoroutine(actions));
        }

        /// <summary>
        /// 停止当前动作
        /// </summary>
        public void Stop() {
            if (_currentCoroutine != null) {
                StopCoroutine(_currentCoroutine);
                _currentCoroutine = null;
            }
            _isPlaying = false;
            // 恢复默认状态，scale 固定为翻转方向，动画只改 position/rotation
            transform.localScale = new Vector3(-1f, 1f, 1f);
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        }

        /// <summary>
        /// 设置透明度
        /// </summary>
        public void SetAlpha(float alpha) {
            if (targetImage != null) {
                var color = targetImage.color;
                color.a = Mathf.Clamp01(alpha);
                targetImage.color = color;
            }
        }

        /// <summary>
        /// 获取透明度
        /// </summary>
        public float GetAlpha() {
            return targetImage?.color.a ?? 1f;
        }

        /// <summary>
        /// 是否正在播放
        /// </summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>
        /// 设置默认位置（用于进入/离开动画）
        /// </summary>
        public void SetDefaultPosition(Vector2 position) {
            _defaultAnchoredPos = position;
            var rect = transform as RectTransform;
            if (rect != null) {
                rect.anchoredPosition = position;
            }
        }

        // ==================== 动作协程 ====================

        IEnumerator PlayCoroutine(ActionParams action, Action onComplete) {
            _isPlaying = true;

            if (action.delay > 0) {
                yield return new WaitForSeconds(action.delay);
            }

            switch (action.type) {
                case CharacterActionType.Jump:
                    yield return JumpCoroutine(action.duration, action.intensity);
                    break;
                case CharacterActionType.Bounce:
                    yield return BounceCoroutine(action.duration, action.intensity);
                    break;
                case CharacterActionType.Shake:
                    yield return ShakeCoroutine(action.duration, action.intensity);
                    break;
                case CharacterActionType.FadeIn:
                    yield return FadeCoroutine(action.duration, 0f, 1f);
                    break;
                case CharacterActionType.FadeOut:
                    yield return FadeCoroutine(action.duration, 1f, 0f);
                    break;
                case CharacterActionType.EnterFromLeft:
                    yield return EnterFromDirection(true, action.duration);
                    break;
                case CharacterActionType.EnterFromRight:
                    yield return EnterFromDirection(false, action.duration);
                    break;
                case CharacterActionType.ExitToLeft:
                    yield return ExitToDirection(true, action.duration, () => {
                        SetAlpha(0f);
                    });
                    break;
                case CharacterActionType.ExitToRight:
                    yield return ExitToDirection(false, action.duration, () => {
                        SetAlpha(0f);
                    });
                    break;
                case CharacterActionType.Flash:
                    yield return FlashCoroutine(action.duration, action.intensity);
                    break;
                case CharacterActionType.Pulse:
                    yield return PulseCoroutine(action.duration, action.intensity);
                    break;
                case CharacterActionType.LeanLeft:
                    yield return LeanCoroutine(action.duration, -15f * action.intensity);
                    break;
                case CharacterActionType.LeanRight:
                    yield return LeanCoroutine(action.duration, 15f * action.intensity);
                    break;
            }

            _isPlaying = false;
            // 动画结束后恢复翻转方向，避免动画 scale 干扰角色朝向
            transform.localScale = new Vector3(-1f, 1f, 1f);
            onComplete?.Invoke();
            action.onComplete?.Invoke();
        }

        IEnumerator PlaySequenceCoroutine(ActionParams[] actions) {
            _isPlaying = true;

            foreach (var action in actions) {
                bool completed = false;
                ActionParams updatedAction = action;
                updatedAction.onComplete = () => completed = true;

                yield return PlayCoroutine(updatedAction, null);

                // 安全等待
                float timeout = 10f;
                float elapsed = 0f;
                while (!completed && elapsed < timeout) {
                    yield return null;
                    elapsed += Time.unscaledDeltaTime;
                }
            }

            _isPlaying = false;
        }

        // ==================== 具体动作实现 ====================

        /// <summary>跳跃动作 - 上下跳动</summary>
        IEnumerator JumpCoroutine(float duration, float intensity) {
            var rect = transform as RectTransform;
            if (rect == null) yield break;

            Vector2 startPos = rect.anchoredPosition;
            float jumpHeight = 30f * intensity;
            float halfDuration = duration / 2f;
            float elapsed = 0f;

            while (elapsed < duration) {
                float t = elapsed / halfDuration;
                if (t > 1f) t = 2f - t; // 反弹

                float height = Mathf.Sin(t * Mathf.PI) * jumpHeight;
                rect.anchoredPosition = startPos + Vector2.up * height;

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            rect.anchoredPosition = startPos;
        }

        /// <summary>弹跳动作 - 弹性缩放</summary>
        IEnumerator BounceCoroutine(float duration, float intensity) {
            Vector3 startScale = _defaultLocalScale;
            float maxScale = 1.15f * intensity;
            float elapsed = 0f;
            float t;

            while (elapsed < duration) {
                t = elapsed / duration;
                // 弹簧效果：先快速放大，再弹回来
                float scale;
                if (t < 0.3f) {
                    // 快速放大
                    scale = Mathf.Lerp(1f, maxScale, t / 0.3f);
                } else if (t < 0.5f) {
                    // 轻微过冲
                    scale = Mathf.Lerp(maxScale, 1.05f, (t - 0.3f) / 0.2f);
                } else {
                    // 衰减振荡
                    float decayT = (t - 0.5f) / 0.5f;
                    scale = Mathf.Lerp(1.05f, 1f, defaultEase.Evaluate(decayT));
                }

                transform.localScale = startScale * scale;
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            transform.localScale = startScale;
        }

        /// <summary>摇晃动作 - 左右摇摆</summary>
        IEnumerator ShakeCoroutine(float duration, float intensity) {
            Vector3 startRot = new Vector3(0, 0, _defaultRotation);
            float shakeAngle = 8f * intensity;
            float elapsed = 0f;
            float speed = 15f;

            while (elapsed < duration) {
                float decay = 1f - (elapsed / duration);
                float angle = Mathf.Sin(elapsed * speed) * shakeAngle * decay;
                transform.localRotation = Quaternion.Euler(startRot.x, startRot.y, startRot.z + angle);

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            transform.localRotation = Quaternion.Euler(startRot);
        }

        /// <summary>淡入淡出</summary>
        IEnumerator FadeCoroutine(float duration, float fromAlpha, float toAlpha) {
            float elapsed = 0f;
            SetAlpha(fromAlpha);

            while (elapsed < duration) {
                float t = defaultEase.Evaluate(elapsed / duration);
                SetAlpha(Mathf.Lerp(fromAlpha, toAlpha, t));

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            SetAlpha(toAlpha);
        }

        /// <summary>从屏幕外进入（不改变透明度）</summary>
        IEnumerator EnterFromDirection(bool fromLeft, float duration) {
            var rect = transform as RectTransform;
            if (rect == null) yield break;

            Vector2 startPos = _defaultAnchoredPos;
            float screenEdgeOffset = Screen.width * 0.6f;

            // 起始位置
            Vector2 enterPos = fromLeft
                ? startPos + Vector2.left * screenEdgeOffset
                : startPos + Vector2.right * screenEdgeOffset;

            rect.anchoredPosition = enterPos;
            SetAlpha(1f); // 确保可见

            float elapsed = 0f;
            while (elapsed < duration) {
                float t = defaultEase.Evaluate(elapsed / duration);
                rect.anchoredPosition = Vector2.Lerp(enterPos, startPos, t);

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            rect.anchoredPosition = startPos;
        }

        /// <summary>离开屏幕外（不改变透明度）</summary>
        IEnumerator ExitToDirection(bool toLeft, float duration, Action onHidden = null) {
            var rect = transform as RectTransform;
            if (rect == null) yield break;

            Vector2 startPos = rect.anchoredPosition;
            float screenEdgeOffset = Screen.width * 0.6f;

            Vector2 exitPos = toLeft
                ? startPos + Vector2.left * screenEdgeOffset
                : startPos + Vector2.right * screenEdgeOffset;

            float elapsed = 0f;
            while (elapsed < duration) {
                float t = defaultEase.Evaluate(elapsed / duration);
                rect.anchoredPosition = Vector2.Lerp(startPos, exitPos, t);

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            rect.anchoredPosition = exitPos;
            onHidden?.Invoke();
        }

        /// <summary>高亮闪烁 - 用于强调/震惊</summary>
        IEnumerator FlashCoroutine(float duration, float intensity) {
            if (targetImage == null) yield break;

            Color originalColor = targetImage.color;
            Color flashColor = Color.Lerp(originalColor, Color.white, 0.5f * intensity);
            float elapsed = 0f;
            bool toWhite = true;

            while (elapsed < duration) {
                Color target = toWhite ? flashColor : originalColor;
                targetImage.color = Color.Lerp(targetImage.color, target, Time.unscaledDeltaTime * 10f);

                elapsed += Time.unscaledDeltaTime;
                if (elapsed >= duration / 2f && toWhite) {
                    toWhite = false;
                }

                yield return null;
            }

            targetImage.color = originalColor;
        }

        /// <summary>脉冲效果 - 缩放振荡</summary>
        IEnumerator PulseCoroutine(float duration, float intensity) {
            Vector3 startScale = _defaultLocalScale;
            float maxScale = 1.2f * intensity;
            float elapsed = 0f;
            float speed = 8f;

            while (elapsed < duration) {
                float t = elapsed / duration;
                float decay = 1f - t;
                float pulse = (Mathf.Sin(elapsed * speed) * 0.5f + 0.5f) * decay;
                float scale = Mathf.Lerp(1f, maxScale, pulse);

                transform.localScale = startScale * scale;
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            transform.localScale = startScale;
        }

        /// <summary>倾斜动作</summary>
        IEnumerator LeanCoroutine(float duration, float targetAngle) {
            float startAngle = _defaultRotation;
            float elapsed = 0f;

            while (elapsed < duration) {
                float t = defaultEase.Evaluate(elapsed / duration);
                float angle = Mathf.Lerp(startAngle, startAngle + targetAngle, t);
                transform.localRotation = Quaternion.Euler(0, 0, angle);

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            transform.localRotation = Quaternion.Euler(0, 0, startAngle + targetAngle);
        }
    }
}
