using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour {
    [SerializeField] private TMP_Text remainingStrengthText;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private bool anchorTopRightOnAwake = true;

    void Awake() {
        if (anchorTopRightOnAwake) {
            TryAnchorTopRight();
        }
    }

    void OnEnable() {
        ElectricManager.OnSignalPropagated += RefreshRemainingStrength;
        RefreshRemainingStrength();
    }

    void OnDisable() {
        ElectricManager.OnSignalPropagated -= RefreshRemainingStrength;
    }

    [ContextMenu("Refresh HUD")]
    public void RefreshRemainingStrength() {
        if (remainingStrengthText == null) {
            return;
        }

        ElectricManager manager = ElectricManager.Instance;
        if (manager == null) {
            remainingStrengthText.text = "剩余电强: 0";
            remainingStrengthText.color = warningColor;
            return;
        }

        int maxOutput = manager.GetCurrentPathMaxOutput();
        int wireCount = manager.GetPlacedWireCount();
        int remainingStrength = maxOutput - wireCount;

        remainingStrengthText.text = $"剩余电强: {remainingStrength}";
        remainingStrengthText.color = remainingStrength <= 0 ? warningColor : normalColor;
    }

    private void TryAnchorTopRight() {
        if (remainingStrengthText == null) {
            return;
        }

        RectTransform rect = remainingStrengthText.rectTransform;
        if (rect == null) {
            return;
        }

        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
    }
}
