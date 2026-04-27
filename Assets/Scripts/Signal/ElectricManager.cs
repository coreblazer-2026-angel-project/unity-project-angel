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

    [Header("预览 Tilemap 层")]
    [Tooltip("专门用于预览电线的 Tilemap 层（半透明）")]
    public Tilemap previewTilemap;

    [Header("元件 Tilemap 层")]
    [Tooltip("专门用于非电线元件的 Tilemap 层")]
    public Tilemap elementTilemap;

    [Header("元件 Tile 配置")]
    [Tooltip("非电线元件的 CellType → Tile 映射（未激活/激活）")]
    public List<ElementTileEntry> elementTileEntries;
    public Dictionary<CellType, ElementTileEntry> elementTileDict = new();

    [Serializable]
    public struct ElementTileEntry {
        public CellType type;
        public TileBase tile;
        public TileBase poweredTile;
    }

    Grid _tilemapGrid;

    protected override void Awake() {
        base.Awake();
        foreach (ElectricPrefabEntry electricPrefabEntry in prefabEntries) {
            if (electricPrefabEntry.prefab == null) {
                Debug.LogWarning($"ElectricManager: prefabEntries 中 CellType [{electricPrefabEntry.type}] 的预制体为空，已跳过。");
                continue;
            }
            prefabDict[electricPrefabEntry.type] = electricPrefabEntry.prefab;
        }
        foreach (ElementTileEntry entry in elementTileEntries) {
            elementTileDict[entry.type] = entry;
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

        if (elementTilemap != null && elementTilemap.layoutGrid != null) {
            elementTilemap.layoutGrid.cellSize = new Vector3(gs, gs, 1f);
            elementTilemap.layoutGrid.transform.position = new Vector3(-gs / 2f, -gs / 2f, 0f);
        }

        if (previewTilemap != null && previewTilemap.layoutGrid != null) {
            previewTilemap.layoutGrid.cellSize = new Vector3(gs, gs, 1f);
            previewTilemap.layoutGrid.transform.position = new Vector3(-gs / 2f, -gs / 2f, 0f);
        }
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
        if (wireTilemap.GetTile(cellPos) != target) {
            wireTilemap.SetTile(cellPos, target);
            RefreshNeighborTiles(cellPos);
        }
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

    public bool HasElementTile(CellType type) {
        return elementTileDict.ContainsKey(type);
    }

    public void SetElementTile(int x, int y, CellType type, bool powered) {
        if (elementTilemap == null) return;
        if (!elementTileDict.TryGetValue(type, out var entry)) return;

        TileBase tile = powered && entry.poweredTile != null ? entry.poweredTile : entry.tile;
        if (tile == null) return;

        Vector3Int cellPos = GetTilePos(x, y);
        elementTilemap.SetTile(cellPos, tile);
    }

    public void ClearElementTile(int x, int y) {
        if (elementTilemap == null) return;
        Vector3Int cellPos = GetTilePos(x, y);
        elementTilemap.SetTile(cellPos, null);
    }

    // ---------- 预览电线 Tilemap 操作 ----------

    public void SetPreviewWireTile(int x, int y) {
        if (previewTilemap == null) return;
        Vector3Int cellPos = GetTilePos(x, y);
        previewTilemap.SetTile(cellPos, wireTileUnpowered);
        RefreshPreviewNeighborTiles(cellPos);
    }

    public void ClearPreviewTile(int x, int y) {
        if (previewTilemap == null) return;
        Vector3Int cellPos = GetTilePos(x, y);
        previewTilemap.SetTile(cellPos, null);
        RefreshPreviewNeighborTiles(cellPos);
    }

    public void ClearAllPreviewTiles() {
        if (previewTilemap == null) return;
        previewTilemap.ClearAllTiles();
    }

    void RefreshPreviewNeighborTiles(Vector3Int cellPos) {
        previewTilemap.RefreshTile(cellPos);
        previewTilemap.RefreshTile(cellPos + Vector3Int.up);
        previewTilemap.RefreshTile(cellPos + Vector3Int.down);
        previewTilemap.RefreshTile(cellPos + Vector3Int.left);
        previewTilemap.RefreshTile(cellPos + Vector3Int.right);
    }

    // ----------

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

        Queue<(ElectricElementBase current, ElectricElementBase from)> queue = new();
        HashSet<ElectricElementBase> visited = new();
        List<ElectricElementBase> toActivate = new();
        bool allValid = true;

        // 追踪 CrossConnector 各轴向是否已处理，防止循环
        Dictionary<CrossConnector, bool> ccHorizontalProcessed = new();
        Dictionary<CrossConnector, bool> ccVerticalProcessed = new();

        queue.Enqueue((powerSource, null));
        visited.Add(powerSource);
        powerSource.intensity = powerSource.workIntensity;

        // 第一遍遍历：收集所有可能激活的元件，检查是否有 intensity <= 0
        while (queue.Count > 0) {
            var (cur, from) = queue.Dequeue();

            // 检查当前元件是否满足激活条件
            if (cur.intensity <= 0) {
                allValid = false;
            }

            // 只有 intensity >= workIntensity 才加入待激活列表
            if (cur.intensity >= cur.workIntensity) {
                toActivate.Add(cur);
            }

            // CrossConnector 特殊处理：按来源轴向隔离信号
            if (cur is CrossConnector cc) {
                bool isFromHorizontal = from != null && from.bindGrid != null && cc.bindGrid != null
                    && from.bindGrid.y == cc.bindGrid.y;
                bool isFromVertical = from != null && from.bindGrid != null && cc.bindGrid != null
                    && from.bindGrid.x == cc.bindGrid.x;

                if (isFromHorizontal) {
                    if (ccHorizontalProcessed.ContainsKey(cc) && ccHorizontalProcessed[cc]) continue;
                    ccHorizontalProcessed[cc] = true;
                } else if (isFromVertical) {
                    if (ccVerticalProcessed.ContainsKey(cc) && ccVerticalProcessed[cc]) continue;
                    ccVerticalProcessed[cc] = true;
                } else {
                    continue;
                }

                foreach (var next in cc.neighborElements) {
                    if (visited.Contains(next)) continue;

                    bool nextIsHorizontal = next.bindGrid != null && cc.bindGrid != null
                        && next.bindGrid.y == cc.bindGrid.y;
                    bool nextIsVertical = next.bindGrid != null && cc.bindGrid != null
                        && next.bindGrid.x == cc.bindGrid.x;

                    // 只向同轴方向的邻居传播
                    if ((isFromHorizontal && nextIsHorizontal) || (isFromVertical && nextIsVertical)) {
                        visited.Add(next);
                        next.intensity = Mathf.Max(0, cur.intensity - 1);
                        Debug.Log($"{next.GetType().Name} Intensity = {next.intensity} CalcIntensity = {Mathf.Max(0, cur.intensity - 1)} CurIntensity = {cur.intensity} Grid = {next.bindGrid.x},{next.bindGrid.y}");
                        queue.Enqueue((next, cur));
                    }
                }
                continue;
            }

            // 非电线元件且不传播信号的元件（SignalAmplifier 也会继续传播）
            if (cur is not PowerSource && cur is not Wire && cur is not SignalAmplifier)
                continue;

            foreach (var next in cur.neighborElements) {
                if (visited.Contains(next)) continue;

                visited.Add(next);
                int outgoingIntensity = Mathf.Max(0, cur.intensity - 1);
                if (cur is SignalAmplifier amp && cur.intensity >= cur.workIntensity) {
                    outgoingIntensity += amp.boostValue;
                }
                next.intensity = outgoingIntensity;
                Debug.Log($"{next.GetType().Name} Intensity = {next.intensity} CalcIntensity = {outgoingIntensity} CurIntensity = {cur.intensity} Grid = {next.bindGrid.x},{next.bindGrid.y}");
                queue.Enqueue((next, cur));
            }
        }

        // 如果所有元件都有效（没有 intensity <= 0），则统一激活
        if (allValid) {
            foreach (var elem in toActivate) {
                elem.Activate();
            }
        }
        // 否则所有元件保持 Deactive 状态（已在开头重置）

        // 检查所有灯：相邻有激活电线则点亮，否则熄灭
        foreach (var element in ElectricElements.Values) {
            if (element is Light light) {
                bool hasActiveWire = false;
                foreach (var neighbor in light.neighborElements) {
                    if (neighbor is Wire && neighbor.intensity > 0) {
                        hasActiveWire = true;
                        break;
                    }
                }
                if (hasActiveWire) {
                    light.Activate();
                } else {
                    light.Deactive();
                }
            }
        }
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
