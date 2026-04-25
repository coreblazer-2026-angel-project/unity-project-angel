using UnityEngine;

public class Wire : ElectricElementBase {
    void Start() {
        // 在 Tilemap 上放置未通电的电线 Tile
        SetInitialTile();
    }

    void SetInitialTile() {
        if (bindGrid == null) return;
        var em = ElectricManager.Instance;
        if (em == null) return;
        em.SetWireTile(bindGrid.x, bindGrid.y, em.wireTileUnpowered);
    }

    public override void Remove() {
        // 清除 Tilemap 上的 Tile
        if (bindGrid != null) {
            ElectricManager.Instance?.ClearTile(bindGrid.x, bindGrid.y);
        }
        base.Remove();
    }
}
