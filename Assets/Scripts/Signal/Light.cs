using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Light : ElectricElementBase {
    protected override void Start() {
        base.Start();
    }

    public override void Activate() {
        base.Activate();
        spriteRenderer.color = Color.yellow;
    }

    public override void Deactive() {
        base.Deactive();
        spriteRenderer.color = Color.blue;
    }
}
