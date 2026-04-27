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
        PlaceInvisibleWireTile();
        bool tilePlaced = PlaceElementTile();
        if (tilePlaced && spriteRenderer != null) {
            spriteRenderer.enabled = false;
        }
        _electricManager?.BeginSimulate();
    }

    /// <summary>
    /// 在 wireTilemap 上放置隐形电线瓦片，让 Wire 的 RuleTile 能连接到该位置。
    /// 如果该位置已有瓦片（如 Wire 已放置），则跳过避免覆盖。
    /// </summary>
    protected void PlaceInvisibleWireTile() {
        if (this is Wire) return;
        var em = _electricManager;
        if (em == null || em.wireTilemap == null || bindGrid == null) return;

        Vector3Int cellPos = em.GetTilePos(bindGrid.x, bindGrid.y);
        if (em.wireTilemap.GetTile(cellPos) != null) return;

        em.SetWireTile(bindGrid.x, bindGrid.y, em.wireTileUnpowered);
        em.wireTilemap.SetColor(cellPos, Color.clear);
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

        // 连接同格子内的其他元件
        foreach (var obj in grid.holdObjects) {
            if (obj == null || obj == this.gameObject) continue;
            if (obj.TryGetComponent(out ElectricElementBase elem)) {
                if (!this.CanConnectTo(elem) || !elem.CanConnectTo(this)) continue;
                if (!this.neighborElements.Contains(elem)) {
                    this.neighborElements.Add(elem);
                    elem.neighborElements.Add(this);
                }
            }
        }

        // 连接邻居格子内的元件
        GridV2[] neighborGrids = bindGrid.GetAllNeighbors();
        foreach (GridV2 neighborGrid in neighborGrids) {
            if (!neighborGrid) continue;
            foreach (var obj in neighborGrid.holdObjects) {
                if (obj == null) continue;
                if (obj.TryGetComponent(out ElectricElementBase electricElement)) {
                    if (!this.CanConnectTo(electricElement) || !electricElement.CanConnectTo(this))
                        continue;
                    if (!this.neighborElements.Contains(electricElement)) {
                        this.neighborElements.Add(electricElement);
                        electricElement.neighborElements.Add(this);
                        this.OnNeighborConnected(electricElement);
                        electricElement.OnNeighborConnected(this);
                    }
                }
            }
        }
    }

    /// <summary>连接建立后的回调，子类可重写</summary>
    protected virtual void OnNeighborConnected(ElectricElementBase neighbor) { }

    [ContextMenu("Remove")]
    public virtual void Remove() {
        if (!(this is Wire) && bindGrid != null) {
            ElectricManager.Instance?.ClearElementTile(bindGrid.x, bindGrid.y);
        }

        // 从同格子内的其他元件中移除引用
        foreach (var obj in bindGrid.holdObjects) {
            if (obj == null || obj == gameObject) continue;
            if (obj.TryGetComponent(out ElectricElementBase elem)) {
                elem.neighborElements.Remove(this);
                this.neighborElements.Remove(elem);
            }
        }

        // 从邻居格子内的元件中移除引用
        GridV2[] neighborGrids = bindGrid.GetAllNeighbors();
        foreach (GridV2 neighborGrid in neighborGrids) {
            if (!neighborGrid) continue;
            foreach (var obj in neighborGrid.holdObjects) {
                if (obj == null) continue;
                if (obj.TryGetComponent(out ElectricElementBase electricElement)) {
                    electricElement.neighborElements.Remove(this);
                    this.neighborElements.Remove(electricElement);
                }
            }
        }

        // 清除隐形电线瓦片（但该格子还有 Wire 时保留，让 Wire 继续正常显示）
        if (!(this is Wire) && bindGrid != null) {
            bool hasWire = false;
            foreach (var obj in bindGrid.holdObjects) {
                if (obj != null && obj != gameObject && obj.GetComponent<Wire>() != null) {
                    hasWire = true;
                    break;
                }
            }
            if (!hasWire) {
                ElectricManager.Instance?.ClearTile(bindGrid.x, bindGrid.y);
            }
        }

        bindGrid.holdObjects.Remove(gameObject);
        if (bindGrid.holdObject == gameObject)
            bindGrid.holdObject = bindGrid.holdObjects.Count > 0 ? bindGrid.holdObjects[0] : null;

        // 刷新相邻的 PowerSource（邻居电线情况可能变了）
        RefreshNeighborPowerSources();

        ElectricManager.Instance.RemoveElement(this);
    }

    void RefreshNeighborPowerSources() {
        if (bindGrid == null) return;
        var gmv2 = GridManagerV2.Instance;
        if (gmv2 == null) return;

        GridV2[] neighbors = new[] {
            gmv2.GetGrid(bindGrid.x, bindGrid.y - 1),
            gmv2.GetGrid(bindGrid.x, bindGrid.y + 1),
            gmv2.GetGrid(bindGrid.x - 1, bindGrid.y),
            gmv2.GetGrid(bindGrid.x + 1, bindGrid.y),
        };

        foreach (var n in neighbors) {
            if (n == null) continue;
            foreach (var obj in n.holdObjects) {
                if (obj != null && obj.TryGetComponent(out PowerSource ps)) {
                    ps.RefreshPowerSourceSprite();
                }
            }
        }
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
