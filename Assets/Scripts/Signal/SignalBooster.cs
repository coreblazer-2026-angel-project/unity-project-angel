using UnityEngine;

/// <summary>
/// 信号增幅器（一次性能量注入器）。
/// 当上方有被激活的电线时，将自身 boostValue 永久加到所有电源的 workIntensity 上，然后自我销毁。
/// </summary>
public class SignalBooster : ElectricElementBase {
    [Header("增幅数值")]
    [Tooltip("触发后加到电源 workIntensity 上的数值")]
    public int boostValue = 2;
}
