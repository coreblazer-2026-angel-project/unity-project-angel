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
        Color32 color32 = (Color32)color;
        foreach (var block in AllBlocks) {
            if (block == null) {
                continue;
            }

            if ((Color32)block.phaseColor == color32) {
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
