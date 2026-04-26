using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSource : ElectricElementBase {
    protected override void Start() {
        base.Start();
    }

    public override void Activate() {
        base.Activate();
    }

    public override bool CanConnectTo(ElectricElementBase other) {
        if (other is Wire) {
            foreach (var neighbor in neighborElements) {
                if (neighbor is Wire)
                    return false;
            }
        }
        return true;
    }
}
