using TMPro;
using UnityEngine;

public interface ISignalReceiver {
    void ReceiveSignal(int strength);
}

public class SignalAmplifier : ElectricElementBase, ISignalReceiver {
    [Tooltip("增幅值，输出强度 = 输入强度 + amplifyValue")]
    public int amplifyValue = 2;

    [Header("UI")]
    [SerializeField] private TMP_Text amplifyValueText;
    [SerializeField] private TMP_Text currentIntensityText;

    [Header("Visual")]
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color activeColor = Color.cyan;

    private int inputStrength;

    void Awake() {
        UpdateDisplay();
        UpdateVisual(false);
    }

    void OnValidate() {
        if (amplifyValue < 0) {
            amplifyValue = 0;
        }

        UpdateDisplay();
    }

    public override void Activate() {
        // BFS 在进入当前节点时已经写入 intensity，这里只做本节点信号变换，不做额外传播。
        ReceiveSignal(intensity);
        base.Activate();
    }

    public override void Deactive() {
        inputStrength = 0;
        intensity = 0;
        UpdateDisplay();
        UpdateVisual(false);
        base.Deactive();
    }

    public void ReceiveSignal(int strength) {
        inputStrength = Mathf.Max(0, strength);

        if (inputStrength > 0) {
            intensity = inputStrength + amplifyValue;
            UpdateVisual(true);
        }
        else {
            intensity = 0;
            UpdateVisual(false);
        }

        UpdateDisplay();
    }

    private void UpdateDisplay() {
        if (amplifyValueText != null) {
            amplifyValueText.text = $"+{amplifyValue}";
        }

        if (currentIntensityText != null) {
            currentIntensityText.text = $"E:{intensity}";
        }
    }

    private void UpdateVisual(bool isActive) {
        if (spriteRenderer == null) {
            return;
        }

        spriteRenderer.color = isActive ? activeColor : inactiveColor;
    }
}
