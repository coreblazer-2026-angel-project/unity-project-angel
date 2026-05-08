using UnityEngine;

/// <summary>
/// 信号合并器：两侧输入信号相加后从指定方向输出。
/// 支持四种方向变体（上/下/左/右），由 cellType 决定。
/// 只与 Wire 建立连接。
/// 使用 SpriteRenderer 渲染，根据方向自动旋转，根据输入端激活状态切换 sprite。
/// sprites 列表顺序：0=无输入无输出, 1=仅输入1, 2=仅输入2, 3=双输入无输出, 4=双输入有输出
/// 同时保留 wireTilemap 上的隐形 tile，确保与电线的 RuleTile 正常交互。
/// </summary>
public class SignalMerger : ElectricElementBase {

    public override bool CanConnectTo(ElectricElementBase other) {
        return other is Wire;
    }

    protected override void Start() {
        PlaceInvisibleWireTile();
        // 使用 SpriteRenderer 渲染，启用并设置方向
        if (spriteRenderer != null) {
            spriteRenderer.enabled = true;
            ApplyDirectionRotation();
        }
        ElectricManager.Instance?.BeginSimulate();
        // 显式刷新一次，确保初始 sprite 正确（即使 BeginSimulate 因无电源未执行）
        RefreshTileState();
    }

    /// <summary>获取输入方向（两个输入端，dx/dy 为相对偏移）</summary>
    public (int dx1, int dy1, int dx2, int dy2) GetInputDirections() {
        switch (cellType) {
            case CellType.SignalMerger:      // 上：左右输入
            case CellType.SignalMergerDown:  // 下：左右输入
                return (-1, 0, 1, 0);
            case CellType.SignalMergerLeft:  // 左：上下输入
            case CellType.SignalMergerRight: // 右：上下输入
                return (0, -1, 0, 1);
            default:
                return (-1, 0, 1, 0);
        }
    }

    /// <summary>获取输出方向（dx/dy 为相对偏移）</summary>
    public (int dx, int dy) GetOutputDirection() {
        switch (cellType) {
            case CellType.SignalMerger:      return (0, -1);  // 上
            case CellType.SignalMergerDown:  return (0, 1);   // 下
            case CellType.SignalMergerLeft:  return (-1, 0);  // 左
            case CellType.SignalMergerRight: return (1, 0);   // 右
            default: return (0, -1);
        }
    }

    /// <summary>根据 cellType 设置 SpriteRenderer 的旋转（假设基础 sprite 朝上）</summary>
    void ApplyDirectionRotation() {
        if (spriteRenderer == null) return;
        switch (cellType) {
            case CellType.SignalMerger:
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case CellType.SignalMergerDown:
                transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
            case CellType.SignalMergerLeft:
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case CellType.SignalMergerRight:
                transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
        }
    }

    /// <summary>
    /// 刷新显示：根据输入端是否有激活电线切换 sprite（状态0~4）
    /// 同时刷新隐形电线 tile 的激活状态
    /// </summary>
    protected override void RefreshTileState() {
        int state = CalculateState();

        // 切换 sprite
        if (spriteRenderer != null && sprites != null && sprites.Count > 0) {
            Sprite targetSprite = null;
            // 优先使用对应状态的 sprite
            if (state >= 0 && state < sprites.Count && sprites[state] != null) {
                targetSprite = sprites[state];
            }
            // 回退到 sprites[0]
            if (targetSprite == null && sprites[0] != null) {
                targetSprite = sprites[0];
            }
            // 再回退到第一个非 null 的 sprite
            if (targetSprite == null) {
                foreach (var s in sprites) {
                    if (s != null) { targetSprite = s; break; }
                }
            }
            if (targetSprite != null) {
                spriteRenderer.sprite = targetSprite;
            }
        }

        RefreshInvisibleWireTile();
    }

    /// <summary>计算当前状态（0~4）。
    /// 输入端：1 = 有激活电线，0 = 无电线或未激活
    /// 输出端：1 = 有激活电线 或 无电线连接（合并器自身输出），0 = 有电线但未激活
    /// </summary>
    int CalculateState() {
        var (in1x, in1y, in2x, in2y) = GetInputDirections();
        var (outx, outy) = GetOutputDirection();

        bool in1 = HasActiveWireAtOffset(in1x, in1y);
        bool in2 = HasActiveWireAtOffset(in2x, in2y);
        bool outActive = HasActiveWireAtOffset(outx, outy);
        bool outHasWire = HasWireAtOffset(outx, outy);

        if (!in1 && !in2) return 0;            // 无输入
        if (in1  && !in2) return 1;            // 仅输入1
        if (!in1 && in2)  return 2;            // 仅输入2

        // 双输入有激活：输出端有激活电线 或 无电线连接 → 视为有输出（状态 4）
        if (outActive || !outHasWire) return 4;
        return 3;                               // 双输入有激活，但输出端电线未激活
    }

    /// <summary>指定偏移格子是否有 intensity > 0 的 Wire</summary>
    bool HasActiveWireAtOffset(int dx, int dy) {
        var gmv2 = GridManagerV2.Instance;
        if (gmv2 == null) return false;
        GridV2 cell = gmv2.GetGrid(bindGrid.x + dx, bindGrid.y + dy);
        if (cell == null) return false;
        foreach (var obj in cell.holdObjects) {
            if (obj != null && obj.TryGetComponent(out Wire wire) && wire.intensity > 0)
                return true;
        }
        return false;
    }

    /// <summary>指定偏移格子是否有 Wire（不区分激活与否）</summary>
    bool HasWireAtOffset(int dx, int dy) {
        var gmv2 = GridManagerV2.Instance;
        if (gmv2 == null) return false;
        GridV2 cell = gmv2.GetGrid(bindGrid.x + dx, bindGrid.y + dy);
        if (cell == null) return false;
        foreach (var obj in cell.holdObjects) {
            if (obj != null && obj.GetComponent<Wire>() != null)
                return true;
        }
        return false;
    }
}
