using UnityEngine;

/// <summary>
/// 信号放大器：当上方格子有激活电线时，把 boostValue 永久加到该电线所属电源的 workIntensity 上。
/// 只触发一次（用 hasBuffedPower 标记），不自销毁。
/// </summary>
public class SignalAmplifier : ElectricElementBase {
    [Header("增强数值")]
    [Tooltip("触发时加到电源 workIntensity 的电强值，同时也作为传出信号的额外增益")]
    public int boostValue = 2;

    [Tooltip("是否已触发过电源增强（运行时标记，避免每次 BeginSimulate 重复触发）")]
    [System.NonSerialized] public bool hasBuffedPower = false;
}
