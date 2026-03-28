using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : ManagerBase<GridManager>
{

    [Header("基础设置")] 
    public float cellSize = 0.32f;
    
    [Header("Tilemap 引用")]
    public Tilemap gridmap;  

    [Header("地板（定位）")]
    [Tooltip("每格在槽位 Slot 下生成，与格心对齐；关卡若传入 floorPrefab 则优先使用关卡的。")]
    public GameObject floorPrefab;

    private GridNode[,] _grid;
    Transform _slotsRoot;

    public int Width { get; private set; }
    public int Height { get; private set; }

    void EnsureSlotsRoot()
    {
        if (_slotsRoot != null) return;
        var go = new GameObject("GridSlots");
        go.transform.SetParent(transform, false);
        _slotsRoot = go.transform;
    }

    void DestroyExistingSlots()
    {
        if (_grid == null) return;
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                GridCell slot = _grid[x, y].Slot;
                if (slot != null)
                    Destroy(slot.gameObject);
            }
        }
    }

    /// <summary>
    /// 初始化纯净的网格地图，并为每格生成定位槽位 <see cref="GridNode.Slot"/>。
    /// 若 <paramref name="floorPrefabOverride"/> 或 <see cref="floorPrefab"/> 非空，则在槽位下生成地板作为格心定位锚点。
    /// <paramref name="skipFloorEntirely"/> 为 true 时不生成地板（用于地板已烘焙到场景、仅保留槽位与逻辑时）。
    /// </summary>
    public void InitializeGrid(int width, int height, GameObject floorPrefabOverride = null, bool skipFloorEntirely = false)
    {
        DestroyExistingSlots();
        Width = width;
        Height = height;
        _grid = new GridNode[width, height];
        EnsureSlotsRoot();

        GameObject floorToUse = floorPrefabOverride != null ? floorPrefabOverride : floorPrefab;
        if (skipFloorEntirely)
            floorToUse = null;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                _grid[x, y] = new GridNode(pos);

                GameObject slotGo = new GameObject($"Slot_{x}_{y}");
                slotGo.transform.SetParent(_slotsRoot, false);
                slotGo.transform.position = GridToWorld(pos);
                GridCell slotCell = slotGo.AddComponent<GridCell>();
                slotCell.GridPosition = pos;
                _grid[x, y].Slot = slotCell;

                if (floorToUse != null)
                {
                    GameObject floorGo = Instantiate(floorToUse);
                    floorGo.transform.SetParent(slotGo.transform, false);
                    floorGo.transform.localPosition = Vector3.zero;
                    floorGo.transform.localRotation = Quaternion.identity;
                    floorGo.name = $"Floor_{x}_{y}";
                    _grid[x, y].Floor = floorGo;
                }
            }
        }
    }

    /// <summary>获取某格地板实例（未生成地板时为 null）。</summary>
    public GameObject GetFloorAt(Vector2Int pos)
    {
        if (!IsValidPosition(pos)) return null;
        return _grid[pos.x, pos.y].Floor;
    }

    /// <summary>获取该格的定位槽（未初始化或越界为 null）。</summary>
    public GridCell GetGridCellAt(Vector2Int pos)
    {
        if (!IsValidPosition(pos)) return null;
        return _grid[pos.x, pos.y].Slot;
    }

    /// <summary>
    /// 四邻格的定位槽，顺序：上、下、左、右；越界为 null。
    /// </summary>
    public GridCell[] GetNeighborCells(Vector2Int pos)
    {
        return new[]
        {
            GetGridCellAt(pos + Vector2Int.up),
            GetGridCellAt(pos + Vector2Int.down),
            GetGridCellAt(pos + Vector2Int.left),
            GetGridCellAt(pos + Vector2Int.right)
        };
    }

    /// <summary>
    /// 四邻格的地板实例（与 <see cref="GetNeighborCells"/> 顺序一致：上、下、左、右）。
    /// 越界或该格未挂地板（含烘焙场景未写入 <see cref="GridNode.Floor"/>）时为 null。
    /// </summary>
    public GameObject[] GetNeighborFloors(Vector2Int pos)
    {
        return new[]
        {
            GetFloorAt(pos + Vector2Int.up),
            GetFloorAt(pos + Vector2Int.down),
            GetFloorAt(pos + Vector2Int.left),
            GetFloorAt(pos + Vector2Int.right)
        };
    }

    /// <summary>
    /// 四邻格上的占用实体（顺序同上；空邻格或越界为 null）。
    /// </summary>
    public IGridEntity[] GetNeighborEntities(Vector2Int pos)
    {
        return new[]
        {
            GetEntity(pos + Vector2Int.up),
            GetEntity(pos + Vector2Int.down),
            GetEntity(pos + Vector2Int.left),
            GetEntity(pos + Vector2Int.right)
        };
    }

    #region 坐标转换
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        float centerOffset = cellSize / 2f;
        return new Vector3(
             cellSize * gridPos.x + centerOffset,
             cellSize * gridPos.y + centerOffset,
            0);
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x / cellSize),
            Mathf.FloorToInt(worldPos.y / cellSize));
    }

    /// <summary>将世界坐标转为网格坐标，并判断是否在已初始化网格范围内。</summary>
    public bool TryWorldToGrid(Vector3 worldPos, out Vector2Int gridPos)
    {
        gridPos = WorldToGrid(worldPos);
        return IsValidPosition(gridPos);
    }

    /// <summary>将网格坐标钳到 [0, Width) × [0, Height)。</summary>
    public Vector2Int ClampToGrid(Vector2Int pos)
    {
        if (Width <= 0 || Height <= 0) return pos;
        int x = Mathf.Clamp(pos.x, 0, Width - 1);
        int y = Mathf.Clamp(pos.y, 0, Height - 1);
        return new Vector2Int(x, y);
    }

    /// <summary>与 <see cref="GridToWorld"/> 相同，表示格中心的世界坐标。</summary>
    public Vector3 GetCellCenterWorld(Vector2Int gridPos)
    {
        return GridToWorld(gridPos);
    }
    #endregion

    #region 实体管理
    // 检查坐标是否越界
    public bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < Width && pos.y >= 0 && pos.y < Height;
    }

    // 在网格中放置实体
    public bool PlaceEntity(Vector2Int pos, IGridEntity entity)
    {
        if (!IsValidPosition(pos)) return false;
        
        GridNode node = _grid[pos.x, pos.y];
        if (!node.IsEmpty)
        {
            Debug.LogWarning($"坐标 {pos} 已经有物体了！");
            return false;
        }

        node.SetEntity(entity);
        entity.gameObject.transform.position = GridToWorld(pos); // 自动对齐世界坐标
        return true;
    }

    // 获取网格中的实体
    public IGridEntity GetEntity(Vector2Int pos)
    {
        if (!IsValidPosition(pos)) return null;
        return _grid[pos.x, pos.y].Entity;
    }

    // 移除网格中的实体
    public void RemoveEntity(Vector2Int pos)
    {
        if (IsValidPosition(pos))
        {
            _grid[pos.x, pos.y].ClearEntity();
        }
    }

    #endregion

    #region 格类型与邻格

    /// <summary>获取格子的逻辑类型；越界时返回 <see cref="global::CellType.Empty"/>。</summary>
    public CellType GetCellType(Vector2Int pos)
    {
        if (!IsValidPosition(pos)) return global::CellType.Empty;
        return _grid[pos.x, pos.y].CellType;
    }

    /// <summary>设置格子的逻辑类型；越界则忽略。</summary>
    public void SetCellType(Vector2Int pos, CellType cellType)
    {
        if (!IsValidPosition(pos)) return;
        _grid[pos.x, pos.y].CellType = cellType;
    }

    /// <summary>
    /// 四邻格类型，顺序与 <see cref="GridCell.GetAllNeighbors(Vector2Int)"/> 一致：上、下、左、右。
    /// 越界邻格视为 <see cref="global::CellType.Empty"/>。
    /// </summary>
    public CellType[] GetNeighborCellTypes(Vector2Int pos)
    {
        return new[]
        {
            GetCellType(pos + Vector2Int.up),
            GetCellType(pos + Vector2Int.down),
            GetCellType(pos + Vector2Int.left),
            GetCellType(pos + Vector2Int.right)
        };
    }

    #endregion
}