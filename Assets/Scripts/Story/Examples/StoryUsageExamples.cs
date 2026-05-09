using UnityEngine;

namespace Game.Story.Examples {

    /// <summary>
    /// 剧情系统使用示例
    /// 展示 StoryManager API 的各种用法
    /// </summary>
    public class StoryUsageExamples : MonoBehaviour {

        [Header("测试用剧情文件")]
        [SerializeField] private string testStoryFile = "SampleStory";

        void OnGUI() {
            GUILayout.BeginArea(new Rect(10, 10, 400, 600));
            GUILayout.Label("=== StoryManager API 示例 ===");
            GUILayout.Space(10);

            // 检查 StoryManager 是否存在
            bool hasManager = StoryManager.Instance != null;
            GUILayout.Label($"StoryManager 状态: {(hasManager ? "已就绪" : "未找到")}");
            GUILayout.Space(10);

            // 示例 1: 最简单的一行代码
            GUILayout.Label("--- 示例 1: 最简单的一行代码 ---");
            GUILayout.Label("StoryManager.Play(\"SampleStory\");");
            if (GUILayout.Button("播放剧情 (一行代码)")) {
                StoryManager.Play(testStoryFile);
            }

            GUILayout.Space(10);

            // 示例 2: 指定起始节点
            GUILayout.Label("--- 示例 2: 指定起始节点 ---");
            GUILayout.Label("StoryManager.Play(\"SampleStory\", \"check_glitter\");");
            if (GUILayout.Button("播放指定节点剧情")) {
                StoryManager.Play(testStoryFile, "check_glitter");
            }

            GUILayout.Space(10);

            // 示例 3: 带回调
            GUILayout.Label("--- 示例 3: 带开始和结束回调 ---");
            GUILayout.Label("StoryManager.Play(\"SampleStory\", OnStart, OnEnd);");
            if (GUILayout.Button("播放剧情 (带回调)")) {
                StoryManager.Play(testStoryFile,
                    () => Debug.Log("[示例] 剧情开始！"),
                    () => Debug.Log("[示例] 剧情结束！"));
            }

            GUILayout.Space(10);

            // 示例 4: 停止剧情
            GUILayout.Label("--- 示例 4: 停止剧情 ---");
            GUILayout.Label("StoryManager.Stop();");
            if (GUILayout.Button("停止当前剧情")) {
                StoryManager.Stop();
            }

            GUILayout.Space(10);

            // 示例 5: 继续剧情
            GUILayout.Label("--- 示例 5: 继续剧情 (模拟点击) ---");
            GUILayout.Label("StoryManager.Continue();");
            if (GUILayout.Button("继续 (下一句)")) {
                StoryManager.Continue();
            }

            GUILayout.Space(10);

            // 示例 6: 角色控制
            GUILayout.Label("--- 示例 6: 手动控制角色 ---");
            GUILayout.Label("StoryManager.ShowCharacter(\"主角\", \"开心\");");
            if (GUILayout.Button("显示角色")) {
                StoryManager.ShowCharacter("主角", "主角开心", 0.4f, 0.15f);
            }
            if (GUILayout.Button("隐藏角色")) {
                StoryManager.HideCharacter();
            }

            GUILayout.Space(10);

            // 示例 7: 检查剧情文件是否存在
            GUILayout.Label("--- 示例 7: 检查文件存在 ---");
            bool exists = StoryManager.StoryExists(testStoryFile);
            GUILayout.Label($"SampleStory.json 存在: {exists}");

            GUILayout.Space(20);

            // 其他信息
            GUILayout.Label("=== 使用提示 ===");
            GUILayout.Label("1. 确保剧情 JSON 文件在 Resources/Story/ 目录");
            GUILayout.Label("2. Ink 文件编辑后会自动编译生成 JSON");
            GUILayout.Label("3. 剧情播放时游戏会自动暂停");
            GUILayout.Label("4. 点击/空格/回车可继续对话");

            GUILayout.EndArea();
        }

        // 回调方法示例
        void OnStoryStart() {
            Debug.Log("剧情开始了！");
        }

        void OnStoryEnd() {
            Debug.Log("剧情结束了！");
        }
    }
}
