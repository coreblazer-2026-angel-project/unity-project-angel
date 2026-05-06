using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManagerV2 : ManagerBase<GridManagerV2> {
    public float gridSize = 0.32f;
    public int row = 5;
    public int column = 5;
    [SerializeField] GridV2 gridPrefab;

    private GridV2[,] grids;
    bool _levelLoaded;

    protected override void Awake() {
        base.Awake();
        CollectExistingGrids();
        SetGridVisible(false);
    }

    public void SetGridVisible(bool visible) {
        foreach (Transform child in transform)
            child.gameObject.SetActive(visible);
    }

    public void OnLevelLoaded() {
        _levelLoaded = true;
        SetGridVisible(true);
    }

    /// <summary>根据关卡尺寸构建 grid 数组。会销毁场景中已存在的 GridV2 子物体并重新生成。
    /// 通过 Instantiate(gridPrefab) 实例化预制体，保留预制体上挂载的所有组件。</summary>
    public void BuildGridForLevel(int width, int height) {
        if (width <= 0 || height <= 0) {
            Debug.LogWarning($"GridManagerV2.BuildGridForLevel: 无效尺寸 {width}x{height}，跳过");
            return;
        }

        if (gridPrefab == null) {
            Debug.LogError("GridManagerV2.BuildGridForLevel: gridPrefab 未配置，无法生成网格");
            return;
        }

        // 销毁现有的 GridV2 子物体
        var existing = new List<Transform>();
        foreach (Transform child in transform) existing.Add(child);
        foreach (var child in existing) {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }

        // 设置新尺寸并实例化预制体
        column = width;
        row = height;
        grids = new GridV2[row, column];

        for (int y = 0; y < row; ++y) {
            for (int x = 0; x < column; ++x) {
                GridV2 grid = Instantiate(gridPrefab, transform);
                grid.name = $"Grid_{x}_{y}";
                grid.transform.localPosition = new Vector3(x * gridSize, -y * gridSize, 0);
                grid.x = x;
                grid.y = y;
                grids[y, x] = grid;
            }
        }
    }

    void CollectExistingGrids() {
        grids = new GridV2[row, column];
        foreach (var cell in GetComponentsInChildren<GridV2>()) {
            if (cell.x >= 0 && cell.x < column && cell.y >= 0 && cell.y < row)
                grids[cell.y, cell.x] = cell;
        }
    }

    [ContextMenu("Generate Grids")]
    void GenerateGrids() {
        grids = new GridV2[row, column];

        for (int y = 0; y < row; ++y) {
            for (int x = 0; x < column; ++x) {
                Vector2 pos = new Vector2(x * gridSize, -y * gridSize);
                GridV2 grid = Instantiate(gridPrefab, pos, transform.rotation, transform);
                grid.name = $"Grid_{x}_{y}";
                grid.x = x;
                grid.y = y;
                grids[y, x] = grid;
            }
        }
    }

    public GridV2 GetGrid(int x, int y) {
        if (x < 0 || x >= column || y < 0 || y >= row) {
            Debug.LogWarning($"Grid out of range: ({x},{y})");
            return null;
        }
        return grids[y, x];
    }
}