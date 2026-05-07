using UnityEditor;
using UnityEngine;

namespace Game.Story {
    [CustomEditor(typeof(InkStoryPlayer))]
    public class InkStoryPlayerEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            InkStoryPlayer player = (InkStoryPlayer)target;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);

            GUI.enabled = Application.isPlaying && player.IsPlaying;
            if (GUILayout.Button("Stop Story")) {
                player.Stop();
            }
            GUI.enabled = Application.isPlaying && !player.IsPlaying;
            if (GUILayout.Button("Play Story")) {
                player.Play();
            }
            GUI.enabled = true;

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "Ink Tag 用法：\n" +
                "  角色发言:  player: 你好 #ch player #expr 开心\n" +
                "  旁白:     台词（无角色名前缀）\n" +
                "  隐藏立绘:  #ch -player\n" +
                "  跳转knot:  -> knot_name",
                MessageType.Info);
        }
    }
}
