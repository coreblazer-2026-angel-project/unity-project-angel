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