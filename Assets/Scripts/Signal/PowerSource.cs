using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSource : ElectricElementBase {
    [Tooltip("当前已连接的第一根电线（只读，运行时自动维护）")]
    public Wire firstWire;

    protected override void Start() {
        // 电源使用 SpriteRenderer 渲染，不走 Tilemap
        // 放置隐形电线供 RuleTile 连接
        PlaceInvisibleWireTile();
        ElectricManager.Instance?.BeginSimulate();
        ValidateFirstWire();
        RefreshPowerSourceSprite();
        RefreshAdjacentPowerSources();
    }

    public override void Activate() {
        ValidateFirstWire();
        Debug.Log($"{GetType().Name} Activate Intensity = {this.intensity} Grid = {bindGrid.x},{bindGrid.y}");
        RefreshPowerSourceSprite();
    }

    public override void Deactive() {
        ValidateFirstWire();
        Debug.Log($"{GetType().Name} Deactivate Intensity = {this.intensity} Grid = {bindGrid.x},{bindGrid.y}");
        RefreshPowerSourceSprite();
    }

    /// <summary>
    /// 电源只能连接一根电线。
    /// 如果已经有 firstWire，拒绝新的 Wire；firstWire 销毁后可重新连接。
    /// </summary>
    public override bool CanConnectTo(ElectricElementBase other) {
        if (other is Wire) {
            ValidateFirstWire();
            if (firstWire != null && firstWire != other)
                return false;
        }
        return true;
    }

    /// <summary>连接成功时的回调，记录第一根电线</summary>
    protected override void OnNeighborConnected(ElectricElementBase neighbor) {
        if (neighbor is Wire wire && firstWire == null) {
            firstWire = wire;
        }
    }

    /// <summary>检查 firstWire 是否仍然有效，若已销毁则清空</summary>
    void ValidateFirstWire() {
        if (firstWire != null && firstWire.gameObject == null)
            firstWire = null;
    }

    // ---------- 根据四面电线刷新 Sprite ----------

    /// <summary>
    /// 根据四周是否有电线刷新电源的 SpriteRenderer。
    /// sprites 列表固定为 5 个：
    /// 0=未连接, 1=上, =下, 3=左, 4=右
    /// 若多个方向同时有电线，按 上→下→左→右 优先级取第一个。
    /// </summary>
    public void RefreshPowerSourceSprite() {
        if (spriteRenderer == null || sprites == null || sprites.Count == 0) return;

        bool up = HasWireNeighbor(0, -1);
        bool down = HasWireNeighbor(0, 1);
        bool left = HasWireNeighbor(-1, 0);
        bool right = HasWireNeighbor(1, 0);

        int index = CalcSpriteIndex(up, down, left, right);
        spriteRenderer.sprite = sprites[index];
        spriteRenderer.enabled = true;
    }

    /// <summary>刷新相邻格子上的所有电源</summary>
    public void RefreshAdjacentPowerSources() {
        if (bindGrid == null) return;
        var gmv2 = GridManagerV2.Instance;
        if (gmv2 == null) return;

        GridV2[] neighbors = new[] {
            gmv2.GetGrid(bindGrid.x, bindGrid.y - 1),
            gmv2.GetGrid(bindGrid.x, bindGrid.y + 1),
            gmv2.GetGrid(bindGrid.x - 1, bindGrid.y),
            gmv2.GetGrid(bindGrid.x + 1, bindGrid.y),
        };

        foreach (var n in neighbors) {
            if (n == null) continue;
            foreach (var obj in n.holdObjects) {
                if (obj != null && obj.TryGetComponent(out PowerSource ps)) {
                    ps.RefreshPowerSourceSprite();
                }
            }
        }
    }

    bool HasWireNeighbor(int dx, int dy) {
        if (bindGrid == null) return false;
        var gmv2 = GridManagerV2.Instance;
        if (gmv2 == null) return false;

        GridV2 neighbor = gmv2.GetGrid(bindGrid.x + dx, bindGrid.y + dy);
        if (neighbor == null) return false;

        foreach (var obj in neighbor.holdObjects) {
            if (obj != null && obj.GetComponent<Wire>() != null) return true;
        }
        return false;
    }

    int CalcSpriteIndex(bool up, bool down, bool left, bool right) {
        if (sprites.Count >= 5) {
            // 0=未连接, 1=上, 2=下, 3=左, 4=右
            // 多方向时按 上→下→左→右 优先级
            if (up) return 1;
            if (down) return 2;
            if (left) return 3;
            if (right) return 4;
            return 0;
        }

        // 不足 5 个时回退到最简逻辑
        bool any = up || down || left || right;
        if (sprites.Count >= 2) {
            return any ? 1 : 0; // [0=无, 1=有]
        }
        return 0;
    }
}
