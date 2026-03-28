#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 在编辑模式下把 LevelData 的地板按网格铺到当前场景并写入场景资源，退出 Play 后仍存在（需保存场景）。
/// </summary>
public static class GridFloorSceneBaker {

    [MenuItem("Tools/Grid/将地板烘焙到当前场景（编辑模式）", false, 100)]
    static void BakeFloorToScene() {
        if (Application.isPlaying) {
            EditorUtility.DisplayDialog("提示", "请先退出 Play 模式。", "确定");
            return;
        }

        var levelData = Selection.activeObject as LevelData;
        if (levelData == null) {
            EditorUtility.DisplayDialog("提示", "请在 Project 中选中一个 LevelData 资源。", "确定");
            return;
        }

        if (levelData.floorPrefab == null) {
            EditorUtility.DisplayDialog("提示", "该 LevelData 的 floorPrefab 为空。", "确定");
            return;
        }

        GridManager gm = Object.FindObjectOfType<GridManager>();
        float cellSize = gm != null ? gm.cellSize : 0.32f;

        GameObject root = GameObject.Find("BakedFloorGrid");
        if (root != null) {
            if (!EditorUtility.DisplayDialog("覆盖", "场景中已存在 BakedFloorGrid，是否删除子物体后重新生成？", "是", "取消"))
                return;
            for (int i = root.transform.childCount - 1; i >= 0; i--)
                Undo.DestroyObjectImmediate(root.transform.GetChild(i).gameObject);
        } else {
            root = new GameObject("BakedFloorGrid");
            if (gm != null)
                root.transform.SetParent(gm.transform, false);
            Undo.RegisterCreatedObjectUndo(root, "Create BakedFloorGrid");
        }

        for (int x = 0; x < levelData.width; x++) {
            for (int y = 0; y < levelData.height; y++) {
                Vector2Int gridPos = new Vector2Int(x, y);
                Vector3 world = GridToWorld(gridPos, cellSize);
                GameObject go = InstantiateFloor(levelData.floorPrefab, root.transform);
                go.transform.position = world;
                go.name = $"Floor_{x}_{y}";
                Undo.RegisterCreatedObjectUndo(go, "Bake Floor");
            }
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("完成",
            $"已生成 {levelData.width}×{levelData.height} 块地板到场景。\n请 Ctrl+S 保存场景。\n\n若希望运行时不再动态生成第二份地板：在该 LevelData 上取消勾选「Spawn Runtime Floor」。",
            "确定");
    }

    [MenuItem("Tools/Grid/将地板烘焙到当前场景（编辑模式）", true)]
    static bool ValidateBake() {
        return !Application.isPlaying && Selection.activeObject is LevelData;
    }

    static GameObject InstantiateFloor(GameObject prefab, Transform parent) {
        if (PrefabUtility.IsPartOfPrefabAsset(prefab))
            return (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        return (GameObject)Object.Instantiate(prefab, parent);
    }

    static Vector3 GridToWorld(Vector2Int gridPos, float cellSize) {
        float o = cellSize * 0.5f;
        return new Vector3(cellSize * gridPos.x + o, cellSize * gridPos.y + o, 0f);
    }
}
#endif
