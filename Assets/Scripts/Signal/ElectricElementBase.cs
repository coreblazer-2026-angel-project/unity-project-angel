using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ElectricElementBase : MonoBehaviour {
    public int intensity;
    public int workIntensity;
    public List<ElectricElementBase> neighborElements = new();
    public GridV2 bindGrid;
    public CellType cellType;
    [SerializeField] protected List<Sprite> sprites;
    [SerializeField] protected Sprite showSprite;
    [SerializeField] protected SpriteRenderer spriteRenderer;

    public int ID;
    ElectricManager _electricManager;

    void Awake() {
        _electricManager = ElectricManager.Instance;
        if (_electricManager != null)
            _electricManager.AddElement(this);
    }

    protected virtual void Start() {
        bool tilePlaced = PlaceElementTile();
        if (tilePlaced && spriteRenderer != null) {
            spriteRenderer.enabled = false;
        }
        _electricManager?.BeginSimulate();
    }

    protected bool PlaceElementTile() {
        if (this is Wire) return false;
        var em = _electricManager;
        if (em == null || bindGrid == null) return false;
        if (!em.HasElementTile(cellType)) return false;
        em.SetElementTile(bindGrid.x, bindGrid.y, cellType, false);
        return true;
    }

    protected virtual void OnDestroy() {
        // 使用缓存引用，避免退出播放模式时触发 Singleton 的 isQuitting 警告
        if (_electricManager != null) {
            _electricManager.ElectricElements.Remove(ID);
            _electricManager.BeginSimulate();
        }
    }


    public virtual bool CanConnectTo(ElectricElementBase other) => true;

    public void BindToGrid(GridV2 grid) {
        bindGrid = grid;
        GridV2[] neighborGrids = bindGrid.GetAllNeighbors();
        foreach (GridV2 neighborGrid in neighborGrids) {
            if (!neighborGrid || !neighborGrid.holdObject) continue;
            if (neighborGrid.holdObject.TryGetComponent(out ElectricElementBase electricElement)) {
                if (!this.CanConnectTo(electricElement) || !electricElement.CanConnectTo(this))
                    continue;
                electricElement.neighborElements.Add(this);
                this.neighborElements.Add(electricElement);
            }
        }
    }

    [ContextMenu("Remove")]
    public virtual void Remove() {
        if (!(this is Wire) && bindGrid != null) {
            ElectricManager.Instance?.ClearTile(bindGrid.x, bindGrid.y);
        }

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
        var em = ElectricManager.Instance;
        if (em == null || bindGrid == null) return;

        if (this is Wire) {
            em.RefreshWireTile(bindGrid.x, bindGrid.y, intensity > 0);
        } else if (em.HasElementTile(cellType)) {
            em.SetElementTile(bindGrid.x, bindGrid.y, cellType, intensity > 0);
        }
    }
}
