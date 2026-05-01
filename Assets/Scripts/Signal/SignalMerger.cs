using UnityEngine;

/// <summary>
/// 信号合并器：左右两侧输入信号，将两边电强相加后从上方输出。
/// 只与 Wire 建立连接。
/// 上方有电线时，切换到 outputTile 渲染。
/// </summary>
public class SignalMerger : ElectricElementBase {

    public override bool CanConnectTo(ElectricElementBase other) {
        return other is Wire;
    }

    /// <summary>
    /// 刷新 Tile 状态：上方有 Wire 时使用 outputTile，否则使用普通 tile/poweredTile
    /// </summary>
    protected override void RefreshTileState() {
        var em = ElectricManager.Instance;
        if (em == null || bindGrid == null) return;

        // 检查上方格子是否有 Wire
        bool hasUpWire = false;
        var gmv2 = GridManagerV2.Instance;
        if (gmv2 != null) {
            GridV2 up = gmv2.GetGrid(bindGrid.x, bindGrid.y - 1);
            if (up != null) {
                foreach (var obj in up.holdObjects) {
                    if (obj != null && obj.GetComponent<Wire>() != null) {
                        hasUpWire = true;
                        break;
                    }
                }
            }
        }

        if (em.HasElementTile(cellType)) {
            em.SetElementTile(bindGrid.x, bindGrid.y, cellType, intensity > 0, hasUpWire);
        }
        RefreshInvisibleWireTile();
    }
}
