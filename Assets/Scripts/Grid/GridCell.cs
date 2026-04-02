using UnityEngine;

// 具体的元器件类（不再管网格是怎么生成的，只管自己）
public class GridCell : MonoBehaviour, IGridEntity {
    public Vector2Int GridPosition { get; set; }

    public GameObject HoldObject { get; set; }

    /// <summary>槽位指向当前格内占用的实体（仅定位槽与元件互相持有之一侧）。</summary>
    public IGridEntity OccupantEntity { get; set; }

    /// <summary>元件指向其所在定位槽（仅当本对象为占用物且已注册到 GridNode 时有效）。</summary>
    public GridCell OccupyingSlot { get; set; }



    public void InitByLevelItem(LevelItem levelItem) {

    }


    public IGridEntity GetNeighbor(Vector2Int currentPos, Vector2Int direction) {
        Vector2Int neighborPos = currentPos + direction;
        return GridManager.Instance.GetEntity(neighborPos);
    }

    /// <summary>
    /// 四邻定位槽，顺序：上、下、左、右；越界为 null（与 <see cref="GridManager.GetNeighborCells"/> 一致）。
    /// </summary>
    public GridCell[] GetNeighborCells(Vector2Int pos) {
        return GridManager.Instance.GetNeighborCells(pos);
    }

    /// <inheritdoc cref="GetNeighborCells(Vector2Int)"/>
    public GridCell[] GetNeighborCells() {
        return GetNeighborCells(ResolveNeighborQueryPosition());
    }

    /// <summary>
    /// 四邻格的地板（顺序：上、下、左、右），委托 <see cref="GridManager.GetNeighborFloors"/>。
    /// </summary>
    public GameObject[] GetNeighborFloors(Vector2Int pos) {
        return GridManager.Instance.GetNeighborFloors(pos);
    }

    /// <inheritdoc cref="GetNeighborFloors(Vector2Int)"/>
    public GameObject[] GetNeighborFloors() {
        return GetNeighborFloors(ResolveNeighborQueryPosition());
    }

    /// <summary>
    /// 四邻格上的占用实体（顺序：上、下、左、右），委托 <see cref="GridManager.GetNeighborEntities"/>。
    /// </summary>
    public IGridEntity[] GetNeighborEntities(Vector2Int pos) {
        return GridManager.Instance.GetNeighborEntities(pos);
    }

    /// <inheritdoc cref="GetNeighborEntities(Vector2Int)"/>
    public IGridEntity[] GetNeighborEntities() {
        return GetNeighborEntities(ResolveNeighborQueryPosition());
    }

    // 查一圈房（邻格占用物，供 Signal 等不改脚本时仍可用 GetAllNeighbors）
    public IGridEntity[] GetAllNeighbors(Vector2Int pos) {
        return GridManager.Instance.GetNeighborEntities(pos);
    }

    public IGridEntity[] GetAllNeighbors() {
        return GetAllNeighbors(ResolveNeighborQueryPosition());
    }

    /// <summary>四邻格类型，顺序与 <see cref="GetAllNeighbors(Vector2Int)"/> 一致：上、下、左、右。</summary>
    public CellType[] GetNeighborCellTypes(Vector2Int pos) {
        return GridManager.Instance.GetNeighborCellTypes(pos);
    }

    /// <inheritdoc cref="GetNeighborCellTypes(Vector2Int)"/>
    public CellType[] GetNeighborCellTypes() {
        return GetNeighborCellTypes(ResolveNeighborQueryPosition());
    }

    Vector2Int ResolveNeighborQueryPosition() {
        var gm = GridManager.Instance;
        if (gm != null && gm.IsValidPosition(GridPosition) && ReferenceEquals(gm.GetEntity(GridPosition), this)) {
            return GridPosition;
        }
        if (gm != null) {
            return gm.WorldToGrid(transform.position);
        }
        return Vector2Int.zero;
    }
}