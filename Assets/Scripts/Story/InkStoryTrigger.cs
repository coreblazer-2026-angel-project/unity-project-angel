using UnityEngine;

namespace Game.Story {

    /// <summary>
    /// Ink 剧情触发器：挂在场景物体上，触发 Ink 剧情播放
    /// </summary>
    public class InkStoryTrigger : MonoBehaviour {
        [Header("Ink JSON Asset")]
        public TextAsset inkJsonAsset;

        [Header("从哪个 Knot 开始（留空从 start 开始）")]
        public string knot = "";

        [Header("触发方式")]
        public bool triggerOnce = true;

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
            if (InkStoryPlayer.Instance == null) {
                Debug.LogWarning("[InkStoryTrigger] InkStoryPlayer not found in scene.");
                return;
            }
            if (inkJsonAsset == null) {
                Debug.LogWarning($"[InkStoryTrigger] No ink JSON asset on {gameObject.name}.");
                return;
            }

            _hasTriggered = true;
            InkStoryPlayer.Instance.inkJsonAsset = inkJsonAsset;
            InkStoryPlayer.Instance.Play(knot);
        }

        public void ResetTrigger() {
            _hasTriggered = false;
        }
    }
}
