using UnityEngine;

/// <summary>
/// 信号增强器（放大器）。信号经过此元件时，传出强度会在衰减后额外增加 boostValue。
/// 只有当自身接收到的信号强度 >= workIntensity 时，增强效果才会生效。
/// </summary>
public class SignalAmplifier : ElectricElementBase {
    [Header("增强数值")]
    [Tooltip("传播给邻居时额外增加的电强值")]
    public int boostValue = 2;
}
