using UnityEngine;

namespace Game.Story {

    /// <summary>
    /// 剧情触发器：挂在场景物体上，触发剧情播放
    /// </summary>
    public class StoryTrigger : MonoBehaviour {
        [Header("剧情数据（右键 Create > Game > Story > Story Data 创建）")]
        public StoryData storyData;

        [Header("触发方式")]
        public bool triggerOnce = true;

        [Tooltip("触发时是否让角色隐藏（剧情结束后执行）")]
        public bool hideCharactersOnEnd = false;

        private bool _hasTriggered;

        void OnMouseDown() {
            TryTrigger();
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (!other.CompareTag("Player")) return;
            TryTrigger();
        }

        public void TryTrigger() {
            if (_hasTriggered && triggerOnce) return;
            if (StoryPlayer.Instance == null) {
                Debug.LogWarning("[StoryTrigger] StoryPlayer not found in scene.");
                return;
            }
            if (storyData == null) {
                Debug.LogWarning($"[StoryTrigger] No story data on {gameObject.name}.");
                return;
            }

            _hasTriggered = true;

            if (hideCharactersOnEnd) {
                StoryPlayer.Instance.OnStoryEnd += HideCharacters;
            }

            StoryPlayer.Instance.storyData = storyData;
            StoryPlayer.Instance.Play();
        }

        void HideCharacters() {
            StoryCharacterManager.Instance?.HideAllCharacters();
            StoryPlayer.Instance.OnStoryEnd -= HideCharacters;
        }

        public void ResetTrigger() {
            _hasTriggered = false;
        }
    }
}
