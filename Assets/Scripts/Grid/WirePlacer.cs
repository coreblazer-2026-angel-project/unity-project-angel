using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 鼠标左键放置 / 删除电线（完全基于 GridManagerV2 / GridV2）。
/// Start 时自动将 Tilemap Grid 的 cellSize 和位置与 GridManagerV2 对齐。
/// </summary>
public class WirePlacer : MonoBehaviour
{
    [Header("Tilemap")]
    [Tooltip("专门用于电线的 Tilemap 层；RuleTile 自动渲染四邻连接图形")]
    public Tilemap wireTilemap;
    [Tooltip("通电状态的 RuleTile")]
    public TileBase wireTilePowered;
    [Tooltip("不通电状态的 RuleTile")]
    public TileBase wireTileUnpowered;

    [Header("相机（留空则自动取 Camera.main）")]
    public Camera cam;

    int _wireCount;
    Grid _tilemapGrid;

    Camera Cam => cam != null ? cam : Camera.main;

    void Start()
    {
        SyncTilemapGrid();
    }

    /// <summary>
    /// 扫描所有 GridV2 格子，为非 Wire 元件（如电源）在 wireTilemap 上放置 Tile，
    /// 使 RuleTile 能感知它们的存在并正确渲染连接方向。
    /// 应在关卡加载完成后调用。
    /// </summary>
    /// <summary>占位用的 Tile（取通电版，透明不渲染，仅供 RuleTile 感知邻居）。</summary>
    TileBase PlaceholderTile => wireTilePowered != null ? wireTilePowered : wireTileUnpowered;

    public void SyncElementTiles()
    {
        if (wireTilemap == null || PlaceholderTile == null) return;
        var gmv2 = GridManagerV2.Instance;
        if (gmv2 == null) return;

        for (int y = 0; y < gmv2.row; y++)
        {
            for (int x = 0; x < gmv2.column; x++)
            {
                GridV2 cell = gmv2.GetGrid(x, y);
                if (cell == null || cell.holdObject == null) continue;
                if (cell.holdObject.GetComponent<Wire>() != null) continue;

                Vector3Int cellPos = new Vector3Int(x, -y, 0);
                wireTilemap.SetTile(cellPos, PlaceholderTile);
                wireTilemap.SetTileFlags(cellPos, TileFlags.None);
                wireTilemap.SetColor(cellPos, Color.clear);
            }
        }
    }

    /// <summary>
    /// 自动将 Tilemap 的 Grid 组件 cellSize 与 GridManagerV2.gridSize 同步，
    /// 并设置 Grid 原点偏移使 Tilemap cell 中心与 GridV2 世界位置对齐。
    /// </summary>
    void SyncTilemapGrid()
    {
        if (wireTilemap == null) return;
        _tilemapGrid = wireTilemap.layoutGrid;
        if (_tilemapGrid == null) return;

        float gs = GridManagerV2.Instance != null ? GridManagerV2.Instance.gridSize : 0.32f;

        _tilemapGrid.cellSize = new Vector3(gs, gs, 1f);

        // Tilemap cell (0,0) 的中心在世界 (gs/2, gs/2)，但 GridV2(0,0) 在世界 (0,0)
        // 偏移 Grid 原点使两者对齐
        _tilemapGrid.transform.position = new Vector3(-gs / 2f, -gs / 2f, 0f);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            HandleClick();
    }

    void HandleClick()
    {
        if (Cam == null) return;

        Vector3 world = Cam.ScreenToWorldPoint(Input.mousePosition);
        world.z = 0f;

        var gmv2 = GridManagerV2.Instance;
        if (gmv2 == null) return;

        Vector2Int gridPos = WorldToGrid(world, gmv2.gridSize);
        GridV2 cell = gmv2.GetGrid(gridPos.x, gridPos.y);
        if (cell == null) return;

        if (cell.holdObject != null && cell.holdObject.GetComponent<Wire>() != null)
            DeleteWire(cell);
        else if (cell.holdObject == null)
            TryPlaceWire(cell);
    }

    // ── 放置 ─────────────────────────────────────────────────────────────────

    void TryPlaceWire(GridV2 cell)
    {
        cell.PutElement(CellType.Wire);
        _wireCount++;

        if (cell.holdObject != null)
        {
            foreach (var sr in cell.holdObject.GetComponentsInChildren<SpriteRenderer>())
                sr.enabled = false;
        }

        SetWireTile(cell, wireTileUnpowered);
        Propagate();
    }

    // ── 删除 ─────────────────────────────────────────────────────────────────

    void DeleteWire(GridV2 cell)
    {
        if (cell.holdObject != null)
        {
            var elem = cell.holdObject.GetComponent<ElectricElementBase>();
            elem?.Remove();
        }

        _wireCount = Mathf.Max(0, _wireCount - 1);
        SetWireTile(cell, null);
        Propagate();
    }

    // ── 传播 ─────────────────────────────────────────────────────────────────

    void Propagate()
    {
        ElectricManager.Instance?.BeginSimulate();
        RefreshWireTiles();
    }

    /// <summary>
    /// 遍历所有格子，根据 Wire 的 intensity 切换 Tile：
    /// intensity &gt; 0 → wireTilePowered，否则 → wireTileUnpowered。
    /// </summary>
    void RefreshWireTiles()
    {
        if (wireTilemap == null) return;
        var gmv2 = GridManagerV2.Instance;
        if (gmv2 == null) return;

        for (int y = 0; y < gmv2.row; y++)
        {
            for (int x = 0; x < gmv2.column; x++)
            {
                GridV2 cell = gmv2.GetGrid(x, y);
                if (cell == null || cell.holdObject == null) continue;

                var wire = cell.holdObject.GetComponent<Wire>();
                if (wire == null) continue;

                Vector3Int cellPos = new Vector3Int(x, -y, 0);
                TileBase target = wire.intensity > 0 ? wireTilePowered : wireTileUnpowered;
                if (wireTilemap.GetTile(cellPos) != target)
                    wireTilemap.SetTile(cellPos, target);
            }
        }
    }

    // ── 坐标 ─────────────────────────────────────────────────────────────────

    static Vector2Int WorldToGrid(Vector3 world, float gridSize)
    {
        int gx = Mathf.RoundToInt(world.x / gridSize);
        int gy = Mathf.RoundToInt(-world.y / gridSize);
        return new Vector2Int(gx, gy);
    }

    void SetWireTile(GridV2 cell, TileBase tile)
    {
        if (wireTilemap == null) return;
        Vector3Int cellPos = new Vector3Int(cell.x, -cell.y, 0);
        wireTilemap.SetTile(cellPos, tile);

        // 强制刷新自身及四邻，确保 RuleTile 重新评估连接
        wireTilemap.RefreshTile(cellPos);
        wireTilemap.RefreshTile(cellPos + Vector3Int.up);
        wireTilemap.RefreshTile(cellPos + Vector3Int.down);
        wireTilemap.RefreshTile(cellPos + Vector3Int.left);
        wireTilemap.RefreshTile(cellPos + Vector3Int.right);
    }
}

