using UnityEngine;

public class SignalMerger : ElectricElementBase, ISignalReceiver {
    public enum InputDirection {
        InputA = 0,
        InputB = 1,
        InputC = 2
    }

    [Tooltip("1 出口目标；合流后仅向该对象传播")]
    [SerializeField] private ElectricElementBase outputTarget;

    [Tooltip("输入变化时是否自动向输出传播")]
    [SerializeField] private bool autoPropagate = true;

    [Tooltip("负输入按 0 处理")]
    [SerializeField] private bool clampNegativeInput = true;

    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color activeColor = Color.yellow;

    [SerializeField] private int inputA;
    [SerializeField] private int inputB;
    [SerializeField] private int inputC;

    public int OutputStrength => intensity;

    public void ReceiveSignal(int strength) {
        ReceiveSignal(InputDirection.InputA, strength);
    }

    public void ReceiveSignal(InputDirection direction, int strength) {
        int nextStrength = NormalizeInput(strength);
        bool changed = SetInput(direction, nextStrength);

        if (!changed) {
            return;
        }

        RecalculateOutput();

        if (autoPropagate) {
            Propagate();
        }
    }

    public int GetInput(InputDirection direction) {
        return direction switch {
            InputDirection.InputA => inputA,
            InputDirection.InputB => inputB,
            _ => inputC
        };
    }

    public void ClearAllInputs(bool propagateNow = true) {
        bool changed = inputA != 0 || inputB != 0 || inputC != 0;
        inputA = 0;
        inputB = 0;
        inputC = 0;

        if (!changed) {
            return;
        }

        RecalculateOutput();

        if (propagateNow) {
            Propagate();
        }
    }

    public override void Activate() {
        base.Activate();
        UpdateVisual(true);
    }

    public override void Deactive() {
        base.Deactive();
        UpdateVisual(false);
    }

    public void Propagate() {
        if (outputTarget == null) {
            return;
        }

        outputTarget.intensity = intensity;

        if (outputTarget is ISignalReceiver receiver) {
            receiver.ReceiveSignal(intensity);
            return;
        }

        if (intensity > 0) {
            outputTarget.Activate();
        }
        else {
            outputTarget.Deactive();
        }
    }

    private void RecalculateOutput() {
        intensity = inputA + inputB + inputC;

        if (intensity > 0) {
            Activate();
        }
        else {
            Deactive();
        }
    }

    private int NormalizeInput(int strength) {
        if (!clampNegativeInput) {
            return strength;
        }

        return Mathf.Max(0, strength);
    }

    private bool SetInput(InputDirection direction, int value) {
        switch (direction) {
            case InputDirection.InputA:
                if (inputA == value) {
                    return false;
                }

                inputA = value;
                return true;
            case InputDirection.InputB:
                if (inputB == value) {
                    return false;
                }

                inputB = value;
                return true;
            default:
                if (inputC == value) {
                    return false;
                }

                inputC = value;
                return true;
        }
    }

    private void UpdateVisual(bool active) {
        if (spriteRenderer == null) {
            return;
        }

        spriteRenderer.color = active ? activeColor : inactiveColor;
    }
}
