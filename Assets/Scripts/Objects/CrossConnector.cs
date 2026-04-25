using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossConnector : ElectricElementBase {
    protected override void Start() {
        base.Start();
    }

    public override void Activate() {
        base.Activate();
        foreach (var neighbor in neighborElements) {
            if (neighbor.intensity < intensity) {
                neighbor.intensity = intensity;
                neighbor.Activate();
            }
        }
    }

    public override void Deactive() {
        base.Deactive();
        spriteRenderer.color = Color.gray;
    }
}
