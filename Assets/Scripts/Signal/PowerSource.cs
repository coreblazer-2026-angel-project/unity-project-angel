using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSource : ElectricElementBase {
    void Start() {
    }

    public override void Activate() {
        base.Activate();
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
