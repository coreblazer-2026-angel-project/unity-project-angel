using System;
using System.Collections;
using UnityEngine;

namespace Game.Story {

    /// <summary>
    /// 剧情文件加载器
    /// 根据文件名加载 Ink JSON Asset
    /// </summary>
    public class StoryLoader {
        private static StoryLoader _instance;
        public static StoryLoader Instance => _instance ??= new StoryLoader();

        private StoryLoader() { }

        /// <summary>
        /// 根据文件名加载 Ink JSON Asset
        /// 自动在 Resources/Story 目录下查找
        /// </summary>
        /// <param name="fileName">文件名（不含扩展名），如 "SampleStory"</param>
        /// <returns>加载的 TextAsset，失败返回 null</returns>
        public TextAsset LoadInkJson(string fileName) {
            if (string.IsNullOrEmpty(fileName)) {
                Debug.LogWarning("[StoryLoader] File name is empty!");
                return null;
            }

            string path = $"Story/{fileName}";
            TextAsset asset = Resources.Load<TextAsset>(path);

            if (asset == null) {
                // 尝试直接加载
                asset = Resources.Load<TextAsset>(fileName);
            }

            if (asset == null) {
                Debug.LogWarning($"[StoryLoader] Ink JSON not found: '{path}' or '{fileName}'. " +
                    "Make sure the file is in Resources/Story/ folder.");
            }

            return asset;
        }

        /// <summary>
        /// 检查剧情文件是否存在
        /// </summary>
        public bool Exists(string fileName) {
            return LoadInkJson(fileName) != null;
        }
    }
}
