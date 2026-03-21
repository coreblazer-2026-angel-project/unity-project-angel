using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ElectricElementBase : MonoBehaviour
{
    public int intensity;
    public int workIntensity;
    public ElectricElementBase[] neighborElements;
    //public GridCell bindGrid;
    [SerializeField] protected Sprite[] sprites;
    [SerializeField] protected Sprite showSprite;
    [SerializeField] protected SpriteRenderer spriteRenderer;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    //public void BindToGrid(TileGrid grid) {

    //}

    public virtual void Activate() {
        Debug.Log($"{GetType().Name} Activate Intensity = {this.intensity}");
    }

    public virtual void Deactive() {
        Debug.Log($"{GetType().Name} Deactivate Intensity = {this.intensity}");
    }
}
