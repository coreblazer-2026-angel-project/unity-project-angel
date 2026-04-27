using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CrossConnector : ElectricElementBase, IDraggable {
    [Header("可拖拽")]
    public bool draggable = true;

    public bool CanDrag => draggable;

    DragManager _cachedDragManager;

    protected override void Start() {
        // 基类 ElectricElementBase.Start() 会统一放置隐形电线瓦片
        base.Start();

        // 自动注册到 DragManager
        _cachedDragManager = FindFirstObjectByType<DragManager>();
        _cachedDragManager?.AddDraggable(gameObject);
    }

    void OnDestroy() {
        _cachedDragManager?.RemoveDraggable(gameObject);
    }

    public void OnDragStart(Vector3 worldPos) {
        // 拖拽时显示 SpriteRenderer，让用户能看到被拖动的物件
        if (spriteRenderer != null) spriteRenderer.enabled = true;
    }

    public void OnDragging(Vector3 worldPos) {
    }

    public void OnDragEnd(Vector3 worldPos) {
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        SnapToGridAndMove();
    }

    public override void Remove() {
        // 基类 ElectricElementBase.Remove() 会统一处理隐形电线瓦片和元件瓦片的清除
        base.Remove();
    }

    /// <summary>
    /// CrossConnector 只与 Wire 连接，实现水平/垂直电线的交叉隔离
    /// </summary>
    public override bool CanConnectTo(ElectricElementBase other) {
        return other is Wire;
    }

    public override void Activate() {
        base.Activate();
    }

    public override void Deactive() {
        base.Deactive();
        spriteRenderer.color = Color.gray;
    }

    // ---------- 拖拽后吸附换格子 ----------

    void SnapToGridAndMove() {
        var gmv2 = GridManagerV2.Instance;
        if (gmv2 == null || bindGrid == null) return;

        float gs = gmv2.gridSize;
        Vector3 pos = transform.position;
        int gx = Mathf.RoundToInt(pos.x / gs);
        int gy = Mathf.RoundToInt(-pos.y / gs);

        GridV2 targetCell = gmv2.GetGrid(gx, gy);

        // 目标无效或等于原格子，直接回原位
        if (targetCell == null || targetCell == bindGrid) {
            transform.position = bindGrid.transform.position;
            return;
        }

        // 检查目标格子是否可以放置（CrossConnector 独占，只能空格子）
        if (targetCell.holdObjects.Count > 0) {
            transform.position = bindGrid.transform.position;
            return;
        }

        // 保存属性
        int savedWorkIntensity = workIntensity;

        // 从原格子移除（会销毁当前 gameObject）
        Remove();

        // 在目标格子重新放置
        targetCell.PutElement(CellType.CrossConnector);

        // 恢复属性到新实例
        foreach (var obj in targetCell.holdObjects) {
            if (obj != null && obj.TryGetComponent(out CrossConnector newCc)) {
                newCc.workIntensity = savedWorkIntensity;
                break;
            }
        }
    }

    /// <summary>
    /// 重写 RefreshTileState：同时更新隐形电线瓦片和 element 瓦片的通电状态
    /// </summary>
    protected override void RefreshTileState() {
        base.RefreshTileState();

        var em = ElectricManager.Instance;
        if (em == null || bindGrid == null || em.wireTilemap == null) return;

        // 更新隐形电线瓦片的通电状态
        Vector3Int cellPos = em.GetTilePos(bindGrid.x, bindGrid.y);
        TileBase target = intensity > 0 ? em.wireTilePowered : em.wireTileUnpowered;
        if (em.wireTilemap.GetTile(cellPos) != target) {
            em.wireTilemap.SetTile(cellPos, target);
            // 切换瓦片后保持透明，避免遮挡 elementTilemap 上的显示瓦片
            em.wireTilemap.SetColor(cellPos, Color.clear);
        }
    }
}
