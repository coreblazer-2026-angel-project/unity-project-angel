using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ElectricElementBase : MonoBehaviour {
    public int intensity;
    public int workIntensity;
    public List<ElectricElementBase> neighborElements = new();
    public GridCell bindGrid;
    [SerializeField] protected List<Sprite> sprites;
    [SerializeField] protected Sprite showSprite;
    [SerializeField] protected SpriteRenderer spriteRenderer;

    public int ID;

    void Start() {
        ElectricManager.Instance.AddElement(this);

    }

    // Update is called once per frame
    void Update() {

    }


    public void BindToGrid(GridCell grid) {
        bindGrid = grid;
        IGridEntity[] gridEntities = bindGrid.GetAllNeighbors();
        foreach (IGridEntity gridEntity in gridEntities) {
            if (gridEntity.HoldObject.TryGetComponent(out ElectricElementBase electricElement)) {
                electricElement.neighborElements.Add(this);
                this.neighborElements.Add(electricElement);
            }
        }
    }

    public virtual void Remove() {
        ElectricManager.Instance.RemoveElement(this);
    }

    public virtual void Activate() {
        Debug.Log($"{GetType().Name} Activate Intensity = {this.intensity}");
    }

    public virtual void Deactive() {
        Debug.Log($"{GetType().Name} Deactivate Intensity = {this.intensity}");
    }
}
