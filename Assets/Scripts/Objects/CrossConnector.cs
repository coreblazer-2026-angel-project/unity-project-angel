using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CrossConnector : ElectricElementBase {
    protected override void Start() {
        // 先在 wireTilemap 放置隐形电线瓦片
        // 让相邻 Wire 的 RuleTile 能识别并连接到此位置
        PlaceInvisibleWireTile();
        // 调用 base.Start() 在 elementTilemap 放置显示瓦片，隐藏 SpriteRenderer
        base.Start();
    }

    void PlaceInvisibleWireTile() {
        if (bindGrid == null) return;
        var em = ElectricManager.Instance;
        if (em == null || em.wireTilemap == null) return;

        em.SetWireTile(bindGrid.x, bindGrid.y, em.wireTileUnpowered);

        // 设置瓦片颜色为透明，使电线瓦片不可见
        Vector3Int cellPos = em.GetTilePos(bindGrid.x, bindGrid.y);
        em.wireTilemap.SetColor(cellPos, Color.clear);
    }

    public override void Remove() {
        // 先清除 wireTilemap 上的隐形电线瓦片
        if (bindGrid != null) {
            ElectricManager.Instance?.ClearTile(bindGrid.x, bindGrid.y);
        }
        // base.Remove() 会清除 elementTilemap 上的元件瓦片
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
