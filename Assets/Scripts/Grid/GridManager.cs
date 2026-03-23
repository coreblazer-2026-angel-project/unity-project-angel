using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("基础设置")] 
    public float cellSize = 0.32f;
    
    [Header("Tilemap 引用")]
    public Tilemap gridmap;  

    private GridNode[,] _grid;
    public int Width { get; private set; }
    public int Height { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// 初始化纯净的网格地图
    /// </summary>
    public void InitializeGrid(int width, int height)
    {
        Width = width;
        Height = height;
        _grid = new GridNode[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                _grid[x, y] = new GridNode(new Vector2Int(x, y));
            }
        }
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
}