using UnityEngine;

public class Wire : ElectricElementBase {
    protected override void Start() {
        // 在 Tilemap 上放置未通电的电线 Tile
        SetInitialTile();
        base.Start();
    }

    void SetInitialTile() {
        if (bindGrid == null) return;
        var em = ElectricManager.Instance;
        if (em == null) return;
        em.SetWireTile(bindGrid.x, bindGrid.y, em.wireTileUnpowered);
    }

    public override void Remove() {
        // 播放销毁音效（玩家擦除时触发；ClearAll 走 Destroy 不调 Remove，不会触发）
        ElectricManager.Instance?.PlayWireRemoveSound();

        // 清除 Tilemap 上的 Tile
        if (bindGrid != null) {
            ElectricManager.Instance?.ClearTile(bindGrid.x, bindGrid.y);
        }
        base.Remove();
    }
}
