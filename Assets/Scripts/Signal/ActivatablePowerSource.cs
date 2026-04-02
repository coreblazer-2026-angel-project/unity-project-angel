using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatablePowerSource : ElectricElementBase {
    private bool isActivated = false;

    public void ToggleActivation() {
        isActivated = !isActivated;
        if (isActivated) {
            intensity = workIntensity;
            Activate();
        } else {
            intensity = 0;
            Deactive();
        }
    }

    public override void Activate() {
        base.Activate();
        if (isActivated) {
            foreach (var neighbor in neighborElements) {
                neighbor.intensity = workIntensity;
                neighbor.Activate();
            }
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
