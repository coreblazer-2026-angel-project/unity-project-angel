using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Light : ElectricElementBase {
    protected override void Start() {
        base.Start();
    }

    public override void Activate() {
        base.Activate();
        spriteRenderer.color = Color.yellow;
    }

    public override void Deactive() {
        base.Deactive();
        spriteRenderer.color = Color.blue;
    }

    /// <summary>
    /// 灯的 tile 切换硬性条件：intensity >= workIntensity 才用 poweredTile，
    /// 否则用 tile（即使 intensity > 0 但未达阈值，仍显示未激活）
    /// </summary>
    protected override void RefreshTileState() {
        var em = ElectricManager.Instance;
        if (em == null || bindGrid == null) return;

        bool powered = intensity >= workIntensity;
        if (em.HasElementTile(cellType)) {
            em.SetElementTile(bindGrid.x, bindGrid.y, cellType, powered);
        }
        RefreshInvisibleWireTile();
    }
}
