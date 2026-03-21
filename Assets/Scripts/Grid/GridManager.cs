using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance{get; private set;}

    

    [Header("基础设置")] 
    public float cellSize = 0.32f;
    
    [Header("Tilemap 引用")]
    public Tilemap wiremap;  //画电线的tilemap
    
    public Tilemap gridmap;  //画背景的tilemap
    
    [Header("单元格预制体")]   //这里放单元格类型，wall什么的
    public GameObject[] cellPrefabs;
    
    
    private GridCell[,] _gridCells;   //逻辑网格数组
    private Dictionary<Vector2Int, GridCell> _gridObjects;   //坐标到物体的映射
    private int _width;   //当前关卡宽
    private int _height;  //当前关卡高


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        _gridObjects = new Dictionary<Vector2Int, GridCell>();
    }
    
    /// <summary>
    /// 网格坐标转世界坐标（让单元格中心对齐坐标）
    /// </summary>
    /// <param name="gridPos">网格坐标（x/y整数）</param>
    /// <returns>世界坐标（带0.32f尺寸偏移）</returns>
    public Vector3 GridToWorld(Vector2Int gridPos)    //网格坐标->世界坐标
    {
        float centerOffset = cellSize / 2f;
        return new Vector3(
             cellSize * gridPos.x + centerOffset,
             cellSize * gridPos.y + centerOffset,
            0);
        
    }

    /// <summary>
    /// 世界坐标转网格坐标（取整）
    /// </summary>
    /// <param name="worldPos">世界坐标</param>
    /// <returns>网格坐标（Vector2Int，整数）</returns>
    public Vector2Int WorldToGrid(Vector3 worldPos)    //世界坐标->网格坐标
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x / cellSize),
            Mathf.FloorToInt(worldPos.y / cellSize));
    }

    public Vector3 WorldToTileCell(Vector3 worldPos)
    {
        return gridmap.WorldToCell(worldPos);
    }
    
    

    private void ClearGrid()   //清理网格
    {
        if (_gridObjects == null) return;
        foreach (var obj in _gridObjects.Values)
        {
            if (obj != null)
            {
                Destroy(obj.gameObject);
            }
        }
        _gridObjects.Clear();
        _gridCells = null;
        _width = 0;
        _height = 0;
    }


    private void InitGrid(int width, int height)
    {
        _width = width;
        _height = height;
        _gridCells = new GridCell[_width, _height];
        
        
    }

    private void SpawnCell(LevelItem item)
    {
        if (item.position.x < 0 || item.position.y < 0 ||
            item.position.x >= _width || item.position.y >= _height
            )
        {
            Debug.Log($"坐标{item.position}超出范围！");
        }

        int prefabIndex = (int)item.type;
        if (prefabIndex < 0 || prefabIndex >= cellPrefabs.Length || cellPrefabs[prefabIndex] == null)
        {
            Debug.Log($"未配置{item.type}的单元格预制体");
            return;
        }

        Vector3 worldPos = GridToWorld(item.position);
        GameObject go = Instantiate(cellPrefabs[prefabIndex], 
                                    worldPos, 
                                    Quaternion.identity,  
                                    transform);
        go.name = $"{item.type}_{item.position.x}_{item.position.y}";   //一个勉强好认的名字
        
        GridCell cell = go.GetComponent<GridCell>();

        if (cell == null)
        {
            Debug.Log($"{go.name}缺少组件！");
            Destroy(go);
            return;
        }
        
        cell.position = item.position;
        cell.type = item.type;
        cell.signalStrength = item.signalStrength;
        
        
        _gridCells[item.position.x, item.position.y] = cell;

        if (_gridObjects.ContainsKey(item.position))
        {
            _gridObjects.Remove(item.position);
        }
        _gridObjects.Add(item.position, cell);
    }
    /// <summary>
    /// 加载指定关卡数据
    /// </summary>
    /// <param name="levelData">关卡数据（ScriptableObject实例）</param>
    public void LoadLevel(LevelData levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("关卡数据不能为空！");
            return;
        }
        ClearGrid();
        InitGrid(levelData.width, levelData.height);
        foreach (var item in levelData.items)
        {
            SpawnCell(item);
        }
        Debug.Log($"关卡加载完成！宽：{_width}，高：{_height}，单元格数量：{_gridObjects.Count}");

    }
    
    
    

}
