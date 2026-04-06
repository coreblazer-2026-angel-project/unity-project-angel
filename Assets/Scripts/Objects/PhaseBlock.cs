using System.Collections.Generic;
using UnityEngine;

public class PhaseBlock : ElectricElementBase {
    private static readonly HashSet<PhaseBlock> AllBlocks = new();

    [Header("Phase")]
    [SerializeField] private Color phaseColor = Color.white;

    [Header("Visual")]
    [SerializeField] private float phasedAlpha = 0.25f;

    [Header("Collision")]
    [SerializeField] private Collider2D[] collidersToToggle;

    private bool isPhased;
    private Color solidColor = Color.white;

    public Color PhaseColor => phaseColor;
    public bool IsPhased => isPhased;
    public bool CanPlaceWire => false;

    void OnEnable() {
        AllBlocks.Add(this);
        CacheSolidColor();
        isPhased = false;
        ApplyVisual();
        ApplyCollider();
    }

    void OnDisable() {
        AllBlocks.Remove(this);
    }

    public static void SetPhasedByColor(Color color, bool phased) {
        foreach (var block in AllBlocks) {
            if (block == null) {
                continue;
            }

            // 比较 RGB 分量，忽略 Alpha 消除浮点转换误差
            if (Mathf.Approximately(block.phaseColor.r, color.r) &&
                Mathf.Approximately(block.phaseColor.g, color.g) &&
                Mathf.Approximately(block.phaseColor.b, color.b)) {
                block.SetPhased(phased);
            }
        }
    }

    public void SetPhased(bool phased) {
        if (isPhased == phased) {
            return;
        }

        isPhased = phased;
        ApplyVisual();
        ApplyCollider();
    }

    public override void Activate() {
        base.Activate();
    }

    public override void Deactive() {
        base.Deactive();
    }

    private void CacheSolidColor() {
        if (spriteRenderer != null) {
            solidColor = spriteRenderer.color;
        }
    }

    private void ApplyVisual() {
        if (spriteRenderer == null) {
            return;
        }

        Color nextColor = solidColor;
        nextColor.a = isPhased ? Mathf.Clamp01(phasedAlpha) : 1f;
        spriteRenderer.color = nextColor;
    }

    private void ApplyCollider() {
        if (collidersToToggle == null || collidersToToggle.Length == 0) {
            Collider2D selfCollider = GetComponent<Collider2D>();
            if (selfCollider != null) {
                selfCollider.enabled = !isPhased;
            }
            return;
        }

        for (int i = 0; i < collidersToToggle.Length; i++) {
            Collider2D col = collidersToToggle[i];
            if (col != null) {
                col.enabled = !isPhased;
            }
        }
    }
}
