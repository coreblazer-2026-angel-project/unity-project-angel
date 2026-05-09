using UnityEngine;

namespace Game.Story {

    /// <summary>
    /// Plays an Ink story when the player is near this NPC and presses a key.
    /// Put compiled Ink json files under Assets/Resources/Story.
    /// </summary>
    public class StoryNpcTrigger : MonoBehaviour {
        [Header("玩家")]
        public Transform player;
        public float interactHorizontalRange = 1.6f;
        public float interactVerticalRange = 2.4f;
        public Vector2 interactCenterOffset = Vector2.zero;

        [Header("剧情")]
        [Tooltip("Examples: story1.json, Story/story1, or Assets/Resources/Story/story1.json")]
        public string storyFile = "story1.json";
        public string startKnot = "start";

        [Header("按键")]
        public KeyCode interactKey = KeyCode.F;
        public bool onlyTriggerOnce = false;

        bool _hasTriggered;

        void Start() {
            ResolvePlayer();
        }

        void Update() {
            ResolvePlayer();

            if (player == null || _hasTriggered)
                return;

            if (StoryAPI.IsPlaying)
                return;

            if (!IsPlayerNear())
                return;

            if (Input.GetKeyDown(interactKey)) {
                StoryAPI.PlayStory(storyFile, startKnot);
                if (onlyTriggerOnce)
                    _hasTriggered = true;
            }
        }

        bool IsPlayerNear() {
            Vector2 center = (Vector2)transform.position + interactCenterOffset;
            Vector2 playerPos = player.position;

            float dx = Mathf.Abs(playerPos.x - center.x);
            float dy = Mathf.Abs(playerPos.y - center.y);

            return dx <= interactHorizontalRange && dy <= interactVerticalRange;
        }

        void ResolvePlayer() {
            if (player != null)
                return;

            GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
            if (taggedPlayer != null) {
                player = taggedPlayer.transform;
                return;
            }

            SideScrollPlayer foundPlayer = FindObjectOfType<SideScrollPlayer>();
            if (foundPlayer != null)
                player = foundPlayer.transform;
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected() {
            Gizmos.color = Color.cyan;
            Vector3 center = transform.position + new Vector3(interactCenterOffset.x, interactCenterOffset.y, 0f);
            Gizmos.DrawWireCube(center, new Vector3(interactHorizontalRange * 2f, interactVerticalRange * 2f, 0.1f));
        }
#endif
    }
}
