using UnityEngine;

namespace Game.Story {

    /// <summary>
    /// Simple static entry point for gameplay scripts.
    /// </summary>
    public static class StoryAPI {
        public static bool IsPlaying => StoryManager.Instance != null && StoryManager.Instance.IsPlaying;

        public static void PlayStory(string storyResourcePath, string knot = "start") {
            if (StoryManager.Instance == null) {
                Debug.LogError("[StoryAPI] StoryManager is not in the scene. Add StoryManager prefab before calling PlayStory.");
                return;
            }

            StoryManager.Instance.PlayStory(storyResourcePath, knot);
        }

        public static void StopStory() {
            StoryManager.Instance?.StopStory();
        }
    }
}
