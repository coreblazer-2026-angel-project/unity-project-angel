using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSource : ElectricElementBase {
    void Start() {
        intensity = workIntensity;
        Activate();
    }

    public override void Activate() {
        base.Activate();
        foreach (var neighbor in neighborElements) {
            neighbor.intensity = workIntensity;
            neighbor.Activate();
        }
    }
}
