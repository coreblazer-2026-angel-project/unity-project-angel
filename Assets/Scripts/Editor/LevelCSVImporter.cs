using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

/// <summary>
/// 编辑器工具：从 CSV 文件导入关卡数据到 LevelData ScriptableObject。
/// 菜单路径：Tools/Level/从CSV导入关卡数据
/// </summary>
public class LevelCSVImporter : EditorWindow {

    TextAsset csvAsset;
    LevelData targetLevelData;
    bool createNewAsset = true;
    string newAssetName = "NewLevelData";

    [MenuItem("Tools/Level/从CSV导入关卡数据")]
    static void ShowWindow() {
        var window = GetWindow<LevelCSVImporter>("CSV 关卡导入");
        window.minSize = new Vector2(400, 250);
    }

    /// <summary>批量导入：从 Assets 内某个文件夹读取所有 CSV，逐个生成 LevelData asset</summary>
    [MenuItem("Tools/Level/批量导入CSV关卡")]
    static void BatchImport() {
        string folderPath = EditorUtility.OpenFolderPanel("选择 CSV 所在文件夹（必须在 Assets 内）", Application.dataPath, "");
        if (string.IsNullOrEmpty(folderPath)) return;
        if (!folderPath.StartsWith(Application.dataPath)) {
            EditorUtility.DisplayDialog("错误", "CSV 文件夹必须在 Assets 目录下", "确定");
            return;
        }

        string outputDir = EditorUtility.OpenFolderPanel("选择 LevelData 输出文件夹（必须在 Assets 内）", Application.dataPath, "");
        if (string.IsNullOrEmpty(outputDir)) return;
        if (!outputDir.StartsWith(Application.dataPath)) {
            EditorUtility.DisplayDialog("错误", "输出文件夹必须在 Assets 目录下", "确定");
            return;
        }

        string[] csvFiles = Directory.GetFiles(folderPath, "*.csv", SearchOption.TopDirectoryOnly);
        if (csvFiles.Length == 0) {
            EditorUtility.DisplayDialog("批量导入", "文件夹中没有 .csv 文件", "确定");
            return;
        }

        string outputRel = "Assets" + outputDir.Substring(Application.dataPath.Length).Replace('\\', '/');
        int success = 0;

        foreach (var csvPath in csvFiles) {
            string csvRel = "Assets" + csvPath.Substring(Application.dataPath.Length).Replace('\\', '/');
            var csvAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(csvRel);
            if (csvAsset == null) {
                Debug.LogWarning($"批量导入：无法加载 {csvRel}，跳过");
                continue;
            }

            var items = LevelCSVParser.Parse(csvAsset.text);
            if (items.Count == 0) {
                Debug.LogWarning($"批量导入：{csvRel} 解析为空，跳过");
                continue;
            }

            string assetName = Path.GetFileNameWithoutExtension(csvPath);
            string assetPath = $"{outputRel}/{assetName}.asset";

            var levelData = AssetDatabase.LoadAssetAtPath<LevelData>(assetPath);
            bool isNew = levelData == null;
            if (isNew) levelData = ScriptableObject.CreateInstance<LevelData>();

            levelData.csvData = csvAsset;
            levelData.useInlineItems = false;   // 默认运行时重新解析 CSV，CSV 改了不用重新导入
            levelData.items = items;
            InferLevelSize(levelData, items);

            if (isNew)
                AssetDatabase.CreateAsset(levelData, assetPath);
            else
                EditorUtility.SetDirty(levelData);

            success++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("批量导入完成", $"成功导入 {success}/{csvFiles.Length} 个关卡到\n{outputRel}", "确定");
    }

    void OnGUI() {
        GUILayout.Label("CSV 关卡数据导入", EditorStyles.boldLabel);
        GUILayout.Space(10);

        csvAsset = (TextAsset)EditorGUILayout.ObjectField("CSV 文件", csvAsset, typeof(TextAsset), false);

        GUILayout.Space(10);

        createNewAsset = EditorGUILayout.Toggle("创建新 Asset", createNewAsset);

        if (createNewAsset) {
            newAssetName = EditorGUILayout.TextField("Asset 名称", newAssetName);
        } else {
            targetLevelData = (LevelData)EditorGUILayout.ObjectField("目标 LevelData", targetLevelData, typeof(LevelData), false);
        }

        GUILayout.Space(20);

        EditorGUI.BeginDisabledGroup(csvAsset == null || (!createNewAsset && targetLevelData == null));
        if (GUILayout.Button("导入", GUILayout.Height(40))) {
            Import();
        }
        EditorGUI.EndDisabledGroup();

        GUILayout.Space(10);

        if (GUILayout.Button("打开示例 CSV 格式说明", GUILayout.Height(30))) {
            EditorUtility.DisplayDialog(
                "CSV 格式说明",
                "表头：elementId,cellType,x,y,signalStrength,requiredStrength,amplifyValue,activateThreshold,connections\n\n" +
                "示例：\n" +
                "0,PowerSource,2,4,3,0,0,0,\n" +
                "1,Wire,2,3,0,0,0,0,2\n" +
                "2,HopeLamp,2,2,0,1,0,0,\n\n" +
                "connections 列填写逗号分隔的 elementId",
                "确定"
            );
        }
    }

    void Import() {
        var items = LevelCSVParser.Parse(csvAsset.text);
        if (items.Count == 0) {
            EditorUtility.DisplayDialog("导入失败", "CSV 解析结果为空，请检查文件格式。", "确定");
            return;
        }

        LevelData levelData;

        if (createNewAsset) {
            // 创建新的 LevelData Asset
            string path = EditorUtility.SaveFilePanelInProject(
                "保存 LevelData Asset",
                newAssetName,
                "asset",
                "选择保存位置"
            );

            if (string.IsNullOrEmpty(path)) return;

            levelData = ScriptableObject.CreateInstance<LevelData>();
            levelData.csvData = csvAsset;
            levelData.useInlineItems = false;   // 运行时重新解析 CSV
            levelData.items = items;

            // 从数据推断关卡尺寸
            InferLevelSize(levelData, items);

            AssetDatabase.CreateAsset(levelData, path);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("导入成功", $"已创建关卡数据：{path}\n共导入 {items.Count} 个元件。", "确定");
        } else {
            // 更新现有 Asset
            levelData = targetLevelData;
            Undo.RecordObject(levelData, "Import Level CSV");

            levelData.csvData = csvAsset;
            levelData.useInlineItems = false;   // 运行时重新解析 CSV
            levelData.items = items;

            InferLevelSize(levelData, items);

            EditorUtility.SetDirty(levelData);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("导入成功", $"已更新关卡数据：{levelData.name}\n共导入 {items.Count} 个元件。", "确定");
        }

        EditorGUIUtility.PingObject(levelData);
        Selection.activeObject = levelData;
    }

    static void InferLevelSize(LevelData levelData, System.Collections.Generic.List<LevelItem> items) {
        if (items.Count == 0) return;

        int maxX = items.Max(i => i.position.x);
        int maxY = items.Max(i => i.position.y);

        // 留出一定边距
        levelData.width = Mathf.Max(levelData.width, maxX + 3);
        levelData.height = Mathf.Max(levelData.height, maxY + 3);
    }
}
