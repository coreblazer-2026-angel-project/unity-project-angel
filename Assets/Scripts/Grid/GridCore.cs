using UnityEngine;

// 所有想要放在网格里的元件，都必须实现这个接口！！！！！
public interface IGridEntity {
    Vector2Int GridPosition { get; set; }
    GameObject gameObject { get; } // 方便获取实体对应的游戏物体
    GameObject HoldObject { get; set; }
}

public class GridNode {
    public Vector2Int Position { get; private set; }
    public IGridEntity Entity { get; private set; } // 当前格子上放的元器件（可以为空）

    /// <summary>该格的定位槽位（与元件分离；用于邻格查询）。</summary>
    public GridCell Slot { get; set; }

    /// <summary>该格的地板实例（挂在槽位下作格心定位；可为空）。</summary>
    public GameObject Floor { get; set; }

    /// <summary>逻辑格类型（与 <see cref="CellType"/> 枚举一致）。</summary>
    public CellType CellType { get; set; }

    public bool IsEmpty => Entity == null;

    public GridNode(Vector2Int position) {
        Position = position;
        Entity = null;
        CellType = global::CellType.Empty;
    }

    public void SetEntity(IGridEntity entity) {
        Entity = entity;
        if (entity != null) {
            entity.GridPosition = this.Position;
            if (Slot != null)
                Slot.OccupantEntity = entity;
            if (entity is GridCell gc)
                gc.OccupyingSlot = Slot;
        }
    }

    public void ClearEntity() {
        if (Entity is GridCell gc)
            gc.OccupyingSlot = null;
        if (Slot != null)
            Slot.OccupantEntity = null;
        Entity = null;
        CellType = global::CellType.Empty;
    }
}