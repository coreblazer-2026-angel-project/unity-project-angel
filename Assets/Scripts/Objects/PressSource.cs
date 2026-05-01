using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressSource : ElectricElementBase {
    private bool isPressed = false;

    protected override void Start() {
        base.Start();
    }

    public void Press() {
        if (isPressed) return;
        isPressed = true;
        intensity = workIntensity;
        Activate();
    }

    public void Release() {
        if (!isPressed) return;
        isPressed = false;
        intensity = 0;
        Deactive();
    }

    public override void Activate() {
        base.Activate();
        foreach (var neighbor in neighborElements) {
            neighbor.intensity = workIntensity;
            neighbor.Activate();
        }
    }

    public override void Deactive() {
        base.Deactive();
        foreach (var neighbor in neighborElements) {
            neighbor.intensity = 0;
            neighbor.Deactive();
        }
    }
}
