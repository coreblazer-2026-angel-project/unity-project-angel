using System.Collections.Generic;
using UnityEngine;

public class GridManagerV2 : ManagerBase<GridManagerV2> {
    public float gridSize = 0.32f;
    public int row = 5;
    public int column = 5;
    [SerializeField] GridV2 gridPrefab;

    [Header("屏幕适配")]
    [Range(0.3f, 1f)]
    public float screenFillRatio = 0.85f;

    [Header("整体缩放倍率")]
    [Range(0.5f, 3f)]
    public float gridScale = 1.2f;

    [Header("居中偏移（相对于摄像机可视区域的百分比）")]
    [Tooltip("正值向右，负值向左")]
    [Range(-0.5f, 0.5f)]
    public float centerOffsetXRatio = 0.084f;
    [Tooltip("正值向上，负值向下")]
    [Range(-0.5f, 0.5f)]
    public float centerOffsetYRatio = -0.25f;

    private GridV2[,] grids;
    bool _levelLoaded;

    public float ScaledGridSize { get; private set; }

    // 网格左上角的世界偏移（tilemap 对齐用）
    public Vector3 GridOrigin { get; private set; }

    protected override void Awake() {
        base.Awake();
        ScaledGridSize = gridSize;
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

        column = width;
        row = height;
        grids = new GridV2[row, column];

        // 计算适配屏幕的 gridSize
        Camera cam = Camera.main;
        float camShiftX = 0f, camShiftY = 0f;
        if (cam != null && cam.orthographic) {
            float camH = cam.orthographicSize * 2f;
            float camW = camH * cam.aspect;
            float availW = camW * screenFillRatio;
            float availH = camH * screenFillRatio;
            ScaledGridSize = Mathf.Min(availW / column, availH / row) * gridScale;

            // 动态偏移（百分比 × 摄像机可视区域）
            camShiftX = camW * centerOffsetXRatio;
            camShiftY = camH * centerOffsetYRatio;
        } else {
            ScaledGridSize = gridSize;
        }

        // 居中 + 动态偏移
        float totalW = column * ScaledGridSize;
        float totalH = row * ScaledGridSize;
        float offsetX = -totalW / 2f + camShiftX;
        float offsetY = totalH / 2f + camShiftY;

        GridOrigin = new Vector3(offsetX - ScaledGridSize / 2f, offsetY - ScaledGridSize / 2f, 0f);

        for (int y = 0; y < row; ++y) {
            for (int x = 0; x < column; ++x) {
                GridV2 grid = Instantiate(gridPrefab, transform);
                grid.name = $"Grid_{x}_{y}";
                grid.transform.localPosition = new Vector3(
                    offsetX + x * ScaledGridSize,
                    offsetY - y * ScaledGridSize, 0);
                grid.x = x;
                grid.y = y;

                grid.holdObjects.Clear();
                grid.holdObject = null;
                grid.noPlace = false;

                grids[y, x] = grid;
            }
        }

        // 同步 tilemap 的 cellSize 和位置
        var em = ElectricManager.Instance;
        if (em != null) em.SyncTilemapGrid();
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
        if (grids == null) return null;
        if (x < 0 || x >= column || y < 0 || y >= row) return null;
        return grids[y, x];
    }
}
