using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 鼠标左键放置 / 删除电线（完全基于 GridManagerV2 / GridV2）。
///
/// 放置流程：
///   屏幕→世界→GridV2 → 校验四项 → GridV2.PutElement（创建 Wire + BindToGrid 建边）
///   → wireTilemap.SetTile（RuleTile 自动连接）→ Propagate
///
/// 删除流程（左键点击已有电线）：
///   ElectricElementBase.Remove（清邻接边 + Destroy）→ SetTile null → Propagate
/// </summary>
public class WirePlacer : MonoBehaviour
{
    [Header("关卡配置")]
    [Tooltip("用于读取 wireLimit；可为空（不限数量）")]
    public LevelData levelData;

    [Header("Tilemap")]
    [Tooltip("专门用于电线的 Tilemap 层；RuleTile 自动渲染四邻连接图形")]
    public Tilemap wireTilemap;
    [Tooltip("电线 RuleTile 资源")]
    public TileBase wireTile;

    [Header("相机（留空则自动取 Camera.main）")]
    public Camera cam;

    int _wireCount;

    Camera Cam => cam != null ? cam : Camera.main;

    // ══════════════════════════════════════════════════════════════════════════
    //  Unity 生命周期
    // ══════════════════════════════════════════════════════════════════════════

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            HandleClick();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  点击入口
    // ══════════════════════════════════════════════════════════════════════════

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

    // ══════════════════════════════════════════════════════════════════════════
    //  放置
    // ══════════════════════════════════════════════════════════════════════════

    void TryPlaceWire(GridV2 cell)
    {
        // ① + ③：邻格必须有电源，或有 intensity > 0 的电线
        if (!HasActiveSourceNeighbor(cell))
        {
            Debug.Log("WirePlacer: 必须从电源输出口或已激活的电线延伸（邻格信号强度 > 0）");
            return;
        }

        // ②：wireLimit
        int limit = levelData != null ? levelData.wireLimit : 0;
        if (limit > 0 && _wireCount >= limit)
        {
            Debug.Log($"WirePlacer: 电线数量已达上限 {limit}");
            return;
        }

        // ④：环路检测（DFS 在 GridV2 邻接图上，临时将 cell 视为 Wire）
        if (HasLoop(cell))
        {
            Debug.Log("WirePlacer: 检测到环路，放置取消");
            return;
        }

        // 建立信号图（Wire + BindToGrid 建边 + ElectricManager 登记）
        cell.PutElement(CellType.Wire);
        _wireCount++;

        // 视觉层（RuleTile 自动处理四邻连接图形）
        SetWireTile(cell, wireTile);

        Propagate();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  删除
    // ══════════════════════════════════════════════════════════════════════════

    void DeleteWire(GridV2 cell)
    {
        if (cell.holdObject != null)
        {
            var elem = cell.holdObject.GetComponent<ElectricElementBase>();
            elem?.Remove(); // 清邻接边 + holdObject=null + Destroy
        }

        _wireCount = Mathf.Max(0, _wireCount - 1);

        SetWireTile(cell, null);

        Propagate();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  校验 ① + ③：邻格激活源
    // ══════════════════════════════════════════════════════════════════════════

    bool HasActiveSourceNeighbor(GridV2 cell)
    {
        GridV2[] neighbors = cell.GetAllNeighbors();
        foreach (var n in neighbors)
        {
            if (n == null || n.holdObject == null) continue;

            if (n.holdObject.GetComponent<PowerSource>() != null)
                return true;

            var elem = n.holdObject.GetComponent<ElectricElementBase>();
            if (elem != null && elem.intensity > 0)
                return true;
        }
        return false;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  校验 ④：DFS 无向图环路检测
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 将 <paramref name="newCell"/> 视为已放置电线，
    /// 在 "电源 + 电线" 子图上做无向 DFS，检测回边（环路）。
    /// </summary>
    bool HasLoop(GridV2 newCell)
    {
        var visited = new HashSet<GridV2>();
        return Dfs(newCell, null, visited, newCell);
    }

    bool Dfs(GridV2 cur, GridV2 parent,
             HashSet<GridV2> visited, GridV2 newCell)
    {
        visited.Add(cur);

        GridV2[] neighbors = cur.GetAllNeighbors();
        foreach (var next in neighbors)
        {
            if (next == null) continue;

            bool connectable = (next == newCell)
                || (next.holdObject != null
                    && (next.holdObject.GetComponent<Wire>() != null
                        || next.holdObject.GetComponent<PowerSource>() != null));

            if (!connectable) continue;

            if (!visited.Contains(next))
            {
                if (Dfs(next, cur, visited, newCell)) return true;
            }
            else if (next != parent)
            {
                return true;
            }
        }
        return false;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  传播
    // ══════════════════════════════════════════════════════════════════════════

    void Propagate()
    {
        ElectricManager.Instance?.BeginSimulate();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  坐标工具
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 世界坐标 → V2 网格坐标。<br/>
    /// V2 坐标系：worldX = gridX * gridSize，worldY = -gridY * gridSize
    /// </summary>
    static Vector2Int WorldToGrid(Vector3 world, float gridSize)
    {
        int gx = Mathf.RoundToInt(world.x / gridSize);
        int gy = Mathf.RoundToInt(-world.y / gridSize);
        return new Vector2Int(gx, gy);
    }

    /// <summary>
    /// V2 网格坐标 → 世界坐标（与 GridManagerV2.GenerateGrids 一致）。
    /// </summary>
    static Vector3 GridToWorld(int gx, int gy, float gridSize)
    {
        return new Vector3(gx * gridSize, -gy * gridSize, 0f);
    }

    void SetWireTile(GridV2 cell, TileBase tile)
    {
        if (wireTilemap == null) return;
        float gs = GridManagerV2.Instance != null ? GridManagerV2.Instance.gridSize : 1f;
        Vector3 worldPos = GridToWorld(cell.x, cell.y, gs);
        Vector3Int cellPos = wireTilemap.WorldToCell(worldPos);
        wireTilemap.SetTile(cellPos, tile);
    }
}
