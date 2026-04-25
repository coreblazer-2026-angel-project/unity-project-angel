using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ElectricManager : ManagerBase<ElectricManager> {
    public PowerSource powerSource;
    public int curId = 0;
    [Serializable]
    public struct ElectricPrefabEntry {
        public CellType type;
        public GameObject prefab;
    }
    public List<ElectricPrefabEntry> prefabEntries;
    public Dictionary<CellType, GameObject> prefabDict = new();
    public Dictionary<int, ElectricElementBase> ElectricElements = new();

    [Header("Tilemap 配置")]
    [Tooltip("专门用于电线的 Tilemap 层")]
    public Tilemap wireTilemap;
    [Tooltip("通电状态的 RuleTile")]
    public TileBase wireTilePowered;
    [Tooltip("不通电状态的 RuleTile")]
    public TileBase wireTileUnpowered;

    Grid _tilemapGrid;

    protected override void Awake() {
        base.Awake();
        foreach (ElectricPrefabEntry electricPrefabEntry in prefabEntries) {
            prefabDict.Add(electricPrefabEntry.type, electricPrefabEntry.prefab);
        }
        SyncTilemapGrid();
    }

    void SyncTilemapGrid() {
        if (wireTilemap == null) return;
        _tilemapGrid = wireTilemap.layoutGrid;
        if (_tilemapGrid == null) return;

        float gs = GridManagerV2.Instance != null ? GridManagerV2.Instance.gridSize : 0.32f;
        _tilemapGrid.cellSize = new Vector3(gs, gs, 1f);
        _tilemapGrid.transform.position = new Vector3(-gs / 2f, -gs / 2f, 0f);
    }

    public Vector3Int GetTilePos(int x, int y) => new Vector3Int(x, -y, 0);

    public void SetWireTile(int x, int y, TileBase tile) {
        if (wireTilemap == null) return;
        Vector3Int cellPos = GetTilePos(x, y);
        wireTilemap.SetTile(cellPos, tile);
        RefreshNeighborTiles(cellPos);
    }

    public void RefreshWireTile(int x, int y, bool powered) {
        if (wireTilemap == null) return;
        Vector3Int cellPos = GetTilePos(x, y);
        TileBase target = powered ? wireTilePowered : wireTileUnpowered;
        if (wireTilemap.GetTile(cellPos) != target)
            wireTilemap.SetTile(cellPos, target);
    }

    void RefreshNeighborTiles(Vector3Int cellPos) {
        wireTilemap.RefreshTile(cellPos);
        wireTilemap.RefreshTile(cellPos + Vector3Int.up);
        wireTilemap.RefreshTile(cellPos + Vector3Int.down);
        wireTilemap.RefreshTile(cellPos + Vector3Int.left);
        wireTilemap.RefreshTile(cellPos + Vector3Int.right);
    }

    public void ClearTile(int x, int y) {
        if (wireTilemap == null) return;
        Vector3Int cellPos = GetTilePos(x, y);
        wireTilemap.SetTile(cellPos, null);
        RefreshNeighborTiles(cellPos);
    }

    void Start() {

    }

    void Update() {

    }

    [ContextMenu("Begin Simulate")]
    public void BeginSimulate() {
        if (powerSource == null) {
            Debug.Log("PowerSource Not Selected!");
            return;
        }

        // 先重置所有元件
        foreach (var element in ElectricElements.Values) {
            element.intensity = 0;
            element.Deactive();
        }

        Queue<ElectricElementBase> queue = new();
        HashSet<ElectricElementBase> visited = new();
        List<ElectricElementBase> toActivate = new();
        bool allValid = true;

        queue.Enqueue(powerSource);
        visited.Add(powerSource);
        powerSource.intensity = powerSource.workIntensity;

        // 第一遍遍历：收集所有可能激活的元件，检查是否有 intensity <= 0
        while (queue.Count > 0) {
            var cur = queue.Dequeue();

            // 检查当前元件是否满足激活条件
            if (cur.intensity <= 0) {
                allValid = false;
            }

            // 只有 intensity >= workIntensity 才加入待激活列表
            if (cur.intensity >= cur.workIntensity) {
                toActivate.Add(cur);
            }

            foreach (var next in cur.neighborElements) {
                if (visited.Contains(next)) continue;

                visited.Add(next);
                next.intensity = Mathf.Max(0, cur.intensity - 1);
                Debug.Log($"{next.GetType().Name} Intensity = {next.intensity} CalcIntensity = {Mathf.Max(0, cur.intensity - 1)} CurIntensity = {cur.intensity} Grid = {next.bindGrid.x},{next.bindGrid.y}");
                queue.Enqueue(next);
            }
        }

        // 如果所有元件都有效（没有 intensity <= 0），则统一激活
        if (allValid) {
            foreach (var elem in toActivate) {
                elem.Activate();
            }
        }
        // 否则所有元件保持 Deactive 状态（已在开头重置）
    }

    void RefreshAllWireTiles() {
        if (wireTilemap == null) return;
        var gmv2 = GridManagerV2.Instance;
        if (gmv2 == null) return;

        for (int y = 0; y < gmv2.row; y++) {
            for (int x = 0; x < gmv2.column; x++) {
                GridV2 cell = gmv2.GetGrid(x, y);
                if (cell == null || cell.holdObject == null) continue;

                var wire = cell.holdObject.GetComponent<Wire>();
                if (wire == null) continue;

                RefreshWireTile(x, y, wire.intensity > 0);
            }
        }
    }

    public void AddElement(ElectricElementBase electricElement) {
        electricElement.ID = curId;
        ElectricElements.Add(electricElement.ID, electricElement);
        ++curId;
    }

    public void RemoveElement(ElectricElementBase electricElementBase) {
        ElectricElements.Remove(electricElementBase.ID);
        Destroy(electricElementBase.gameObject);
    }

    class UnionFind {
        private int[] parent;

        public UnionFind(int n) {
            parent = new int[n];
            for (int i = 0; i < n; i++)
                parent[i] = i;
        }

        public int Find(int x) {
            if (parent[x] != x)
                parent[x] = Find(parent[x]);
            return parent[x];
        }

        public bool Union(int a, int b) {
            int rootA = Find(a);
            int rootB = Find(b);

            if (rootA == rootB) {
                return false;
            }

            parent[rootA] = rootB;
            return true;
        }
    }

    public bool CheckRing() {
        UnionFind uf = new UnionFind(curId);

        foreach (var kv in ElectricElements) {
            var element = kv.Value;

            foreach (var neighbor in element.neighborElements) {
                if (element.ID < neighbor.ID) {
                    if (!uf.Union(element.ID, neighbor.ID)) {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}
