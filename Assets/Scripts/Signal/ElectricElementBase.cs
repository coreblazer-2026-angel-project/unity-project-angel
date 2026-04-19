using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ElectricElementBase : MonoBehaviour {
    public int intensity;
    public int workIntensity;
    public List<ElectricElementBase> neighborElements = new();
    public GridV2 bindGrid;
    [SerializeField] protected List<Sprite> sprites;
    [SerializeField] protected Sprite showSprite;
    [SerializeField] protected SpriteRenderer spriteRenderer;

    public int ID;

    void Awake() {
        if (ElectricManager.Instance != null)
            ElectricManager.Instance.AddElement(this);
    }

    void OnDestroy() {
        if (ElectricManager.Instance != null)
            ElectricManager.Instance.ElectricElements.Remove(ID);
    }


    public void BindToGrid(GridV2 grid) {
        bindGrid = grid;
        GridV2[] neighborGrids = bindGrid.GetAllNeighbors();
        foreach (GridV2 neighborGrid in neighborGrids) {
            if (!neighborGrid || !neighborGrid.holdObject) continue;
            if (neighborGrid.holdObject.TryGetComponent(out ElectricElementBase electricElement)) {
                electricElement.neighborElements.Add(this);
                this.neighborElements.Add(electricElement);
            }
        }
    }

    [ContextMenu("Remove")]
    public virtual void Remove() {
        GridV2[] neighborGrids = bindGrid.GetAllNeighbors();
        foreach (GridV2 neighborGrid in neighborGrids) {
            if (!neighborGrid || !neighborGrid.holdObject) continue;
            if (neighborGrid.holdObject.TryGetComponent(out ElectricElementBase electricElement)) {
                electricElement.neighborElements.Remove(this);
                this.neighborElements.Remove(electricElement);
            }
        }
        bindGrid.holdObject = null;
        ElectricManager.Instance.RemoveElement(this);
    }

    public virtual void Activate() {
        Debug.Log($"{GetType().Name} Activate Intensity = {this.intensity}");
    }

    public virtual void Deactive() {
        Debug.Log($"{GetType().Name} Deactivate Intensity = {this.intensity}");
    }
}
