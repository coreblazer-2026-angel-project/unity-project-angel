using UnityEditor;
using UnityEngine;

public class CreateSampleStory {
    [MenuItem("Game/Story/Generate Sample Story")]
    public static void Generate() {
        // 1. Create StoryCharacter prefab in scene
        var go = new GameObject("SampleCharacter");
        go.transform.position = new Vector3(0, -200, 0);

        var sc = go.AddComponent<Game.Story.StoryCharacter>();
        sc.characterId = "player";

        // Load sprites by GUID
        sc.expressions = new System.Collections.Generic.List<Game.Story.StoryCharacter.ExpressionEntry> {
            MakeEntry("默认",     "69a7a4bb4f2d77642b8528deedfd9809"),
            MakeEntry("开心",     "ebb9cb7ea7d780742bda7c8aa16209f5"),
            MakeEntry("开心闭眼", "a47cc1167bd079f4a8521f272263c7dc"),
            MakeEntry("难过",     "7a22f556a1ab26146a3b980f9c4e89eb"),
            MakeEntry("难过闭眼", "475dda7db5fbe92458564f2174cdaf81"),
            MakeEntry("无语",     "47f1a00d86b192e4abf5d104b7c9f2dd"),
            MakeEntry("生气",     "a6b96236142ca2d4994eb6382f44bd6a"),
            MakeEntry("生气闭眼", "55d8d541e46704049ac73700b8df058e"),
            MakeEntry("惊慌",     "b3627af4b1fff0c40a5d170a5ddec0c1"),
            MakeEntry("惊讶",     "70f659f9af428c0418534217e6eee9c0"),
        };

        sc.defaultSprite = sc.expressions[0].sprite;

        Selection.activeGameObject = go;
        Debug.Log("[CreateSampleStory] Sample character created: SampleCharacter");

        // 2. Create StoryData ScriptableObject
        var data = ScriptableObject.CreateInstance<Game.Story.StoryData>();
        data.lines = new System.Collections.Generic.List<Game.Story.StoryLine> {
            MakeLine("player", "惊讶",   "天啊！这是在做什么？"),
            MakeLine("player", "开心",   "看来这次的主题很有趣！"),
            MakeLine("player", "默认",   "这个游戏真有意思，我很期待完全版！"),
            MakeLine("player", "开心闭眼", "哈哈，这次的翻译真有意思！"),
            MakeLine("player", "默认",   "这次的主角设计真精彩，每个表情都特别生动。"),
            MakeLine("player", "惊讶",   "这是什么！我被说服了！"),
            MakeLine("player", "无语",   "……我还是先看看吧。"),
            MakeLine("player", "难过",   "只是看看这个小故事就好了……"),
            MakeLine("player", "难过闭眼", "这个情节真让人感动……"),
            MakeLine("player", "生气",   "可恶！怎么能这样！"),
            MakeLine("player", "生气闭眼", "哦，我就是这样了，怎么怎！"),
            MakeLine("player", "惊慌",   "稍稍稍稍……这是怎么回事！"),
            MakeLine("player", "默认",   "终于算是开始了……我们的故事。"),
        };

        string dir = "Assets/ScriptableObjects";
        if (!AssetDatabase.IsValidFolder(dir)) {
            AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
        }

        AssetDatabase.CreateAsset(data, $"{dir}/SampleStory.asset");
        AssetDatabase.SaveAssets();
        Debug.Log($"[CreateSampleStory] SampleStory.asset created at {dir}/SampleStory.asset");

        // 3. Create StoryDialoguePanel structure as a prefab stub (scene object)
        var panelRoot = new GameObject("SampleDialoguePanel");
        var dlg = panelRoot.AddComponent<Game.Story.StoryDialoguePanel>();

        var rootObj = new GameObject("Root");
        rootObj.transform.SetParent(panelRoot.transform);
        var rootImg = rootObj.AddComponent<UnityEngine.UI.Image>();
        rootImg.color = new Color(0, 0, 0, 0.7f);
        rootImg.rectTransform.anchorMin = new Vector2(0, 0);
        rootImg.rectTransform.anchorMax = new Vector2(1, 0.35f);
        rootImg.rectTransform.sizeDelta = Vector2.zero;
        rootImg.rectTransform.anchoredPosition = Vector2.zero;

        var vl = rootObj.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        vl.padding = new RectOffset(20, 20, 15, 15);
        vl.spacing = 8;

        var sizeFitter = rootObj.AddComponent<UnityEngine.UI.ContentSizeFitter>();
        sizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
        sizeFitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

        var speakerObj = new GameObject("SpeakerName");
        speakerObj.transform.SetParent(rootObj.transform);
        var speakerTxt = speakerObj.AddComponent<UnityEngine.UI.Text>();
        speakerTxt.fontSize = 22;
        speakerTxt.fontStyle = FontStyle.Bold;
        speakerTxt.color = Color.yellow;
        speakerTxt.supportRichText = true;
        speakerTxt.alignment = TextAnchor.MiddleLeft;
        var speakerFitter = speakerObj.AddComponent<UnityEngine.UI.ContentSizeFitter>();
        speakerFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
        speakerFitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;

        var dialogueObj = new GameObject("DialogueText");
        dialogueObj.transform.SetParent(rootObj.transform);
        var dialTxt = dialogueObj.AddComponent<UnityEngine.UI.Text>();
        dialTxt.fontSize = 20;
        dialTxt.color = Color.white;
        dialTxt.supportRichText = true;
        dialTxt.alignment = TextAnchor.UpperLeft;
        dialTxt.resizeTextForBestFit = true;
        dialTxt.resizeTextMinSize = 14;
        dialTxt.resizeTextMaxSize = 40;
        dialTxt.rectTransform.sizeDelta = new Vector2(800, 0);

        var dialFitter = dialogueObj.AddComponent<UnityEngine.UI.ContentSizeFitter>();
        dialFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
        dialFitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

        dlg.root = rootObj;
        dlg.speakerText = speakerTxt;
        dlg.dialogueText = dialTxt;

        // Add Canvas if needed (but root must be under Canvas)
        var canvas = Object.FindObjectOfType<UnityEngine.Canvas>();
        if (canvas == null) {
            var canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<UnityEngine.Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        panelRoot.transform.SetParent(canvas.transform, false);
        rootObj.transform.SetParent(panelRoot.transform, false);
        panelRoot.transform.localPosition = new Vector3(0, -200, 0);

        // Move Root back under Panel
        rootObj.transform.SetParent(panelRoot.transform, false);
        rootObj.transform.SetSiblingIndex(0);

        Selection.activeGameObject = panelRoot;
        Debug.Log("[CreateSampleStory] SampleDialoguePanel created in scene under Canvas");

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("示例剧情生成完成",
            "已创建：\n" +
            "- SampleCharacter (StoryCharacter) — 在 Hierarchy\n" +
            "- SampleDialoguePanel — 在 Hierarchy\n" +
            "- SampleStory.asset — 在 Assets/ScriptableObjects/\n\n" +
            "接下来：\n" +
            "1. 在场景中创建一个 StoryPlayer 并注册角色和面板\n" +
            "2. 创建 StoryTrigger 并挂上 SampleStory.asset\n" +
            "3. 把 StoryCharacter 和 StoryDialoguePanel 拖入 StoryPlayer 的对应列表",
            "OK");
    }

    static Game.Story.StoryCharacter.ExpressionEntry MakeEntry(string name, string guid) {
        var entry = new Game.Story.StoryCharacter.ExpressionEntry();
        entry.name = name;
        var path = AssetDatabase.GUIDToAssetPath(guid);
        if (!string.IsNullOrEmpty(path)) {
            entry.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        } else {
            Debug.LogWarning($"[CreateSampleStory] Could not load sprite with GUID {guid}");
        }
        return entry;
    }

    static Game.Story.StoryLine MakeLine(string character, string expression, string text) {
        return new Game.Story.StoryLine {
            character = character,
            expression = expression,
            text = text
        };
    }
}
