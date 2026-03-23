using UnityEngine;

// 具体的元器件类（不再管网格是怎么生成的，只管自己）
public class GridCell : MonoBehaviour, IGridEntity
{
    // 实现接口要求
    public Vector2Int GridPosition { get; set; }
    
    
    //Gridmanager不关心这里
    public CellType type;            
    public int signalStrength;       
    public bool[] connections = new bool[4];
    
}