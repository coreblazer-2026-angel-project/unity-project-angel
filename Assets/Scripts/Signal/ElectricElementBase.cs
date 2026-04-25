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
        Debug.Log($"{GetType().Name} Activate Intensity = {this.intensity} Grid = {bindGrid.x},{bindGrid.y}");
        RefreshTileState();
    }

    public virtual void Deactive() {
        Debug.Log($"{GetType().Name} Deactivate Intensity = {this.intensity} Grid = {bindGrid.x},{bindGrid.y}");
        RefreshTileState();
    }

    /// <summary>
    /// 刷新该元件在 Tilemap 上的显示状态
    /// 子类可重写以实现自定义显示逻辑
    /// </summary>
    protected virtual void RefreshTileState() {
        // 基类默认实现：刷新 Wire 类型的 Tile 状态
        if (this is Wire wire && bindGrid != null) {
            var em = ElectricManager.Instance;
            if (em != null) {
                em.RefreshWireTile(bindGrid.x, bindGrid.y, intensity > 0);
            }
        }
    }
}
