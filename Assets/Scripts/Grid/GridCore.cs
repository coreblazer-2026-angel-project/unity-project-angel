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

    public bool IsEmpty => Entity == null;

    public GridNode(Vector2Int position) {
        Position = position;
        Entity = null;
    }

    public void SetEntity(IGridEntity entity) {
        Entity = entity;
        if (entity != null) {
            entity.GridPosition = this.Position;
        }
    }

    public void ClearEntity() {
        Entity = null;
    }
}