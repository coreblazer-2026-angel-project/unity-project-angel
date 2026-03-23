using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Light : ElectricElementBase {
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

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
