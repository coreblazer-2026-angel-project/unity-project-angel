using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire : ElectricElementBase
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Activate() {
        base.Activate();
        spriteRenderer.color = Color.red;
    }

    public override void Deactive() {
        base.Deactive();
        spriteRenderer.color = Color.gray;
    }
}
