using UnityEngine;

// 具体的元器件类（不再管网格是怎么生成的，只管自己）
public class GridCell : MonoBehaviour, IGridEntity
{
    // 实现接口要求
    public GridManager gridManager;
    public Vector2Int GridPosition { get; set; }
    
    
    //Gridmanager不关心这里
    public CellType type;            
    public int signalStrength;       
    public bool[] connections = new bool[4];
    
    
    public IGridEntity GetNeighbor(Vector2Int currentPos, Vector2Int direction)
    {
        Vector2Int neighborPos = currentPos + direction;
        return gridManager.GetEntity(neighborPos); 
    }
    
    // 查一圈房（
    public IGridEntity[] GetAllNeighbors(Vector2Int pos)
    {
        return new IGridEntity[]
        {
            gridManager.GetEntity(pos + Vector2Int.up),
            gridManager.GetEntity(pos + Vector2Int.down),
            gridManager.GetEntity(pos + Vector2Int.left),
            gridManager.GetEntity(pos + Vector2Int.right)
        };
    }
    
}