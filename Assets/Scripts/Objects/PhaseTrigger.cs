using UnityEngine;

public class PhaseTrigger : ElectricElementBase, ISignalReceiver {
    [Header("Phase")]
    [SerializeField] private Color phaseColor = Color.white;
    [SerializeField] private int requiredStrength = 1;

    [Header("Visual")]
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color activeColor = Color.green;

    [SerializeField] private int signalStrength;

    private bool isActive;

    public void ReceiveSignal(int strength) {
        signalStrength = Mathf.Max(0, strength);
        EvaluateState();
    }

    public override void Activate() {
        ReceiveSignal(intensity);
        base.Activate();
    }

    public override void Deactive() {
        signalStrength = 0;
        isActive = false;
        ApplyVisual(false);
        PhaseBlock.SetPhasedByColor(phaseColor, false);
        base.Deactive();
    }

    private void EvaluateState() {
        bool nextActive = signalStrength >= Mathf.Max(0, requiredStrength);
        if (nextActive == isActive) {
            return;
        }

        isActive = nextActive;
        ApplyVisual(isActive);
        PhaseBlock.SetPhasedByColor(phaseColor, isActive);
    }

    private void ApplyVisual(bool active) {
        if (spriteRenderer == null) {
            return;
        }

        spriteRenderer.color = active ? activeColor : inactiveColor;
    }
}
