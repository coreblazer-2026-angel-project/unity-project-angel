using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ElectricManager : ManagerBase<ElectricManager> {
    public PowerSource powerSource;
    public int curId = 0;
    [Serializable]
    public struct ElectricPrefabEntry {
        public CellType type;
        public GameObject prefab;
    }
    public List<ElectricPrefabEntry> prefabEntries;
    public Dictionary<CellType, GameObject> prefabDict = new();
    public Dictionary<int, ElectricElementBase> ElectricElements = new();

    [Header("Tilemap 配置")]
    [Tooltip("专门用于电线的 Tilemap 层")]
    public Tilemap wireTilemap;
    [Tooltip("通电状态的 RuleTile")]
    public TileBase wireTilePowered;
    [Tooltip("不通电状态的 RuleTile")]
    public TileBase wireTileUnpowered;

    [Header("预览 Tilemap 层")]
    [Tooltip("专门用于预览电线的 Tilemap 层（半透明）")]
    public Tilemap previewTilemap;

    [Header("元件 Tilemap 层")]
    [Tooltip("专门用于非电线元件的 Tilemap 层")]
    public Tilemap elementTilemap;

    [Header("元件 Tile 配置")]
    [Tooltip("非电线元件的 CellType → Tile 映射（未激活/激活）")]
    public List<ElementTileEntry> elementTileEntries;
    public Dictionary<CellType, ElementTileEntry> elementTileDict = new();

    [Serializable]
    public struct ElementTileEntry {
        public CellType type;
        public TileBase tile;
        public TileBase poweredTile;
        [Tooltip("上方有输出连接时使用的 Tile（如 SignalMerger 上方有电线）")]
        public TileBase outputTile;
    }

    [Header("音效")]
    [Tooltip("电线放置时播放（玩家拖拽确认后）")]
    public AudioClip wirePlaceClip;
    [Tooltip("电线销毁时播放（玩家擦除时）")]
    public AudioClip wireRemoveClip;
    [Tooltip("信号补充球自我销毁时播放")]
    public AudioClip boosterTriggerClip;
    [Tooltip("音效播放器；不指定时自动从同 GameObject 拿/创建一个 AudioSource")]
    public AudioSource audioSource;

    Grid _tilemapGrid;

    protected override void Awake() {
        base.Awake();
        foreach (ElectricPrefabEntry electricPrefabEntry in prefabEntries) {
            if (electricPrefabEntry.prefab == null) {
                Debug.LogWarning($"ElectricManager: prefabEntries 中 CellType [{electricPrefabEntry.type}] 的预制体为空，已跳过。");
                continue;
            }
            prefabDict[electricPrefabEntry.type] = electricPrefabEntry.prefab;
        }
        foreach (ElementTileEntry entry in elementTileEntries) {
            elementTileDict[entry.type] = entry;
        }

        // 为 SignalMerger 方向变体自动复制 prefab 和 tile（只需配置 SignalMerger 一种）
        CopySignalMergerVariants();

        SyncTilemapGrid();
    }

    void SyncTilemapGrid() {
        if (wireTilemap == null) return;
        _tilemapGrid = wireTilemap.layoutGrid;
        if (_tilemapGrid == null) return;

        float gs = GridManagerV2.Instance != null ? GridManagerV2.Instance.gridSize : 0.32f;
        _tilemapGrid.cellSize = new Vector3(gs, gs, 1f);
        _tilemapGrid.transform.position = new Vector3(-gs / 2f, -gs / 2f, 0f);

        if (elementTilemap != null && elementTilemap.layoutGrid != null) {
            elementTilemap.layoutGrid.cellSize = new Vector3(gs, gs, 1f);
            elementTilemap.layoutGrid.transform.position = new Vector3(-gs / 2f, -gs / 2f, 0f);
        }

        if (previewTilemap != null && previewTilemap.layoutGrid != null) {
            previewTilemap.layoutGrid.cellSize = new Vector3(gs, gs, 1f);
            previewTilemap.layoutGrid.transform.position = new Vector3(-gs / 2f, -gs / 2f, 0f);
        }
    }

    public Vector3Int GetTilePos(int x, int y) => new Vector3Int(x, -y, 0);

    public void SetWireTile(int x, int y, TileBase tile) {
        if (wireTilemap == null) return;
        Vector3Int cellPos = GetTilePos(x, y);
        wireTilemap.SetTile(cellPos, tile);
        // 重置 tile 颜色为不透明白色（PlaceInvisibleWireTile 之后会再单独 SetColor(clear) 让自己透明）
        wireTilemap.SetColor(cellPos, Color.white);
        RefreshNeighborTiles(cellPos);
    }

    public void RefreshWireTile(int x, int y, bool powered) {
        if (wireTilemap == null) return;
        Vector3Int cellPos = GetTilePos(x, y);
        TileBase target = powered ? wireTilePowered : wireTileUnpowered;
        if (wireTilemap.GetTile(cellPos) != target) {
            wireTilemap.SetTile(cellPos, target);
            RefreshNeighborTiles(cellPos);
        }
    }

    void RefreshNeighborTiles(Vector3Int cellPos) {
        wireTilemap.RefreshTile(cellPos);
        wireTilemap.RefreshTile(cellPos + Vector3Int.up);
        wireTilemap.RefreshTile(cellPos + Vector3Int.down);
        wireTilemap.RefreshTile(cellPos + Vector3Int.left);
        wireTilemap.RefreshTile(cellPos + Vector3Int.right);
    }

    public void ClearTile(int x, int y) {
        if (wireTilemap == null) return;
        Vector3Int cellPos = GetTilePos(x, y);
        wireTilemap.SetTile(cellPos, null);
        RefreshNeighborTiles(cellPos);
    }

    public bool HasElementTile(CellType type) {
        return elementTileDict.ContainsKey(type);
    }

    public void SetElementTile(int x, int y, CellType type, bool powered, bool hasOutput = false) {
        if (elementTilemap == null) return;
        if (!elementTileDict.TryGetValue(type, out var entry)) return;

        TileBase tile = null;
        if (hasOutput && entry.outputTile != null) {
            tile = entry.outputTile;
        } else if (powered && entry.poweredTile != null) {
            tile = entry.poweredTile;
        } else {
            tile = entry.tile;
        }
        if (tile == null) return;

        Vector3Int cellPos = GetTilePos(x, y);
        elementTilemap.SetTile(cellPos, tile);
        RefreshElementNeighborTiles(cellPos);
    }

    void RefreshElementNeighborTiles(Vector3Int cellPos) {
        if (elementTilemap == null) return;
        elementTilemap.RefreshTile(cellPos);
        elementTilemap.RefreshTile(cellPos + Vector3Int.up);
        elementTilemap.RefreshTile(cellPos + Vector3Int.down);
        elementTilemap.RefreshTile(cellPos + Vector3Int.left);
        elementTilemap.RefreshTile(cellPos + Vector3Int.right);
    }

    public void ClearElementTile(int x, int y) {
        if (elementTilemap == null) return;
        Vector3Int cellPos = GetTilePos(x, y);
        elementTilemap.SetTile(cellPos, null);
    }

    // ---------- 预览电线 Tilemap 操作 ----------

    public void SetPreviewWireTile(int x, int y) {
        if (previewTilemap == null) return;
        Vector3Int cellPos = GetTilePos(x, y);
        previewTilemap.SetTile(cellPos, wireTileUnpowered);
        RefreshPreviewNeighborTiles(cellPos);
    }

    public void ClearPreviewTile(int x, int y) {
        if (previewTilemap == null) return;
        Vector3Int cellPos = GetTilePos(x, y);
        previewTilemap.SetTile(cellPos, null);
        RefreshPreviewNeighborTiles(cellPos);
    }

    public void ClearAllPreviewTiles() {
        if (previewTilemap == null) return;
        previewTilemap.ClearAllTiles();
    }

    void RefreshPreviewNeighborTiles(Vector3Int cellPos) {
        previewTilemap.RefreshTile(cellPos);
        previewTilemap.RefreshTile(cellPos + Vector3Int.up);
        previewTilemap.RefreshTile(cellPos + Vector3Int.down);
        previewTilemap.RefreshTile(cellPos + Vector3Int.left);
        previewTilemap.RefreshTile(cellPos + Vector3Int.right);
    }

    // ----------

    void Start() {

    }

    void Update() {

    }

    [ContextMenu("Begin Simulate")]
    public void BeginSimulate() {
        // 收集所有 PowerSource 作为起点
        List<PowerSource> powerSources = new();
        foreach (var element in ElectricElements.Values) {
            if (element is PowerSource ps) {
                powerSources.Add(ps);
            }
        }

        if (powerSources.Count == 0) {
            Debug.Log("PowerSource Not Selected!");
            return;
        }

        // 先重置所有元件
        foreach (var element in ElectricElements.Values) {
            element.intensity = 0;
            element.sourcePower = null;
            element.Deactive();
        }

        Queue<(ElectricElementBase current, ElectricElementBase from)> queue = new();
        HashSet<ElectricElementBase> visited = new();
        // 按电源分组：每个电源独立的 allValid 标志和待激活列表，避免一个电源拉闸影响其他电源的电路
        Dictionary<PowerSource, bool> psAllValid = new();
        Dictionary<PowerSource, List<ElectricElementBase>> psToActivate = new();

        // 追踪 CrossConnector 各轴向是否已处理，防止循环
        Dictionary<CrossConnector, bool> ccHorizontalProcessed = new();
        Dictionary<CrossConnector, bool> ccVerticalProcessed = new();

        // 从所有电源开始 BFS
        foreach (var ps in powerSources) {
            psAllValid[ps] = true;
            psToActivate[ps] = new List<ElectricElementBase>();
            if (!visited.Contains(ps)) {
                queue.Enqueue((ps, null));
                visited.Add(ps);
                ps.intensity = ps.workIntensity;
                ps.sourcePower = ps;     // 电源本身的 sourcePower 是自己
            }
        }

        // 第一遍遍历：收集所有可能激活的元件，检查是否有 intensity <= 0
        while (queue.Count > 0) {
            var (cur, from) = queue.Dequeue();

            // 按当前元件的 sourcePower 分组判定 allValid
            PowerSource curPs = cur.sourcePower;

            // 检查当前元件是否满足激活条件
            if (cur.intensity <= 0) {
                if (curPs != null && psAllValid.ContainsKey(curPs))
                    psAllValid[curPs] = false;
            }

            // 只有 intensity >= workIntensity 才加入待激活列表
            if (cur.intensity >= cur.workIntensity) {
                if (curPs != null && psToActivate.ContainsKey(curPs))
                    psToActivate[curPs].Add(cur);
            }

            // CrossConnector 特殊处理：按来源轴向隔离信号
            if (cur is CrossConnector cc) {
                bool isFromHorizontal = from != null && from.bindGrid != null && cc.bindGrid != null
                    && from.bindGrid.y == cc.bindGrid.y;
                bool isFromVertical = from != null && from.bindGrid != null && cc.bindGrid != null
                    && from.bindGrid.x == cc.bindGrid.x;

                if (isFromHorizontal) {
                    if (ccHorizontalProcessed.ContainsKey(cc) && ccHorizontalProcessed[cc]) continue;
                    ccHorizontalProcessed[cc] = true;
                } else if (isFromVertical) {
                    if (ccVerticalProcessed.ContainsKey(cc) && ccVerticalProcessed[cc]) continue;
                    ccVerticalProcessed[cc] = true;
                } else {
                    continue;
                }

                foreach (var next in cc.neighborElements) {
                    // SignalBooster 不参与 BFS
                    if (next is SignalBooster) continue;

                    bool nextIsHorizontal = next.bindGrid != null && cc.bindGrid != null
                        && next.bindGrid.y == cc.bindGrid.y;
                    bool nextIsVertical = next.bindGrid != null && cc.bindGrid != null
                        && next.bindGrid.x == cc.bindGrid.x;

                    // 只向同轴方向的邻居传播
                    if (!((isFromHorizontal && nextIsHorizontal) || (isFromVertical && nextIsVertical))) continue;

                    // CrossConnector 邻居用方向标记，普通元件用 visited
                    if (next is CrossConnector ccNext) {
                        bool toH = isFromHorizontal;
                        bool toV = isFromVertical;
                        if (toH && ccHorizontalProcessed.TryGetValue(ccNext, out bool hp) && hp) continue;
                        if (toV && ccVerticalProcessed.TryGetValue(ccNext, out bool vp) && vp) continue;
                    } else {
                        if (visited.Contains(next)) continue;
                        visited.Add(next);
                    }

                    int outgoingIntensity = Mathf.Max(0, cur.intensity - 1);
                    if (next is CrossConnector) {
                        next.intensity = Mathf.Max(next.intensity, outgoingIntensity);
                    } else {
                        next.intensity = outgoingIntensity;
                    }
                    next.sourcePower = cc.sourcePower;
                    Debug.Log($"{next.GetType().Name} Intensity = {next.intensity} CalcIntensity = {outgoingIntensity} CurIntensity = {cur.intensity} Grid = {next.bindGrid.x},{next.bindGrid.y}");
                    queue.Enqueue((next, cur));
                }
                continue;
            }

            // 非电线元件且不传播信号的元件（SignalAmplifier 也会继续传播）
            if (cur is not PowerSource && cur is not Wire && cur is not SignalAmplifier)
                continue;

            foreach (var next in cur.neighborElements) {
                // SignalBooster 不参与 BFS：它只是触发器，不应被视为电路节点（避免被设 intensity / 影响 allValid）
                if (next is SignalBooster) continue;

                // CrossConnector 邻居用方向标记，普通元件用 visited
                if (next is CrossConnector ccNext) {
                    bool fromH = cur.bindGrid != null && ccNext.bindGrid != null && cur.bindGrid.y == ccNext.bindGrid.y;
                    bool fromV = cur.bindGrid != null && ccNext.bindGrid != null && cur.bindGrid.x == ccNext.bindGrid.x;
                    if (!fromH && !fromV) continue;
                    if (fromH && ccHorizontalProcessed.TryGetValue(ccNext, out bool hp) && hp) continue;
                    if (fromV && ccVerticalProcessed.TryGetValue(ccNext, out bool vp) && vp) continue;
                } else {
                    if (visited.Contains(next)) continue;
                    visited.Add(next);
                }

                // 电源出来的第一格不衰减；从其他元件出来的衰减 1
                int outgoingIntensity = (cur is PowerSource)
                    ? cur.intensity
                    : Mathf.Max(0, cur.intensity - 1);
                if (next is CrossConnector) {
                    next.intensity = Mathf.Max(next.intensity, outgoingIntensity);
                } else {
                    next.intensity = outgoingIntensity;
                }
                // 传播信号源：自己是 PowerSource 就用自己，否则继承 cur 的 sourcePower
                next.sourcePower = (cur as PowerSource) ?? cur.sourcePower;
                Debug.Log($"{next.GetType().Name} Intensity = {next.intensity} CalcIntensity = {outgoingIntensity} CurIntensity = {cur.intensity} Grid = {next.bindGrid.x},{next.bindGrid.y}");
                queue.Enqueue((next, cur));
            }
        }

        // 按电源分组处理：每个电源独立判定 allValid，互不影响
        foreach (var ps in powerSources) {
            bool valid = psAllValid.TryGetValue(ps, out bool v) && v;
            var list = psToActivate.TryGetValue(ps, out var l) ? l : null;

            if (valid) {
                // 该电源电路完整有效，激活所有满足条件的元件
                if (list != null) {
                    foreach (var elem in list) {
                        elem.Activate();
                    }
                }
            } else {
                // 该电源电路拉闸：仅把该电源的 sourcePower 元件 intensity 归零并 Deactive
                foreach (var element in ElectricElements.Values) {
                    if (element.sourcePower == ps) {
                        element.intensity = 0;
                        element.Deactive();
                    }
                }
            }
        }

        // 处理 SignalMerger：两侧输入相加，从指定方向输出
        var gmv2 = GridManagerV2.Instance;
        foreach (var element in ElectricElements.Values) {
            if (element is not SignalMerger merger || merger.bindGrid == null) continue;

            var (in1x, in1y, in2x, in2y) = merger.GetInputDirections();
            var (outx, outy) = merger.GetOutputDirection();

            int in1 = GetMaxIntensityAt(gmv2, merger.bindGrid.x + in1x, merger.bindGrid.y + in1y);
            int in2 = GetMaxIntensityAt(gmv2, merger.bindGrid.x + in2x, merger.bindGrid.y + in2y);
            int sum = in1 + in2;

            if (sum <= 0) continue;

            GridV2 outCell = gmv2?.GetGrid(merger.bindGrid.x + outx, merger.bindGrid.y + outy);
            if (outCell == null) continue;

            foreach (var obj in outCell.holdObjects) {
                if (obj == null) continue;
                if (obj.TryGetComponent(out ElectricElementBase outElem)) {
                    if (outElem.intensity < sum) {
                        outElem.intensity = sum;
                        outElem.sourcePower = merger.sourcePower;   // 继承 SignalMerger 的 sourcePower
                        PropagateFrom(outElem, sum);
                    }
                }
            }
        }

        // 所有 Wire intensity 已确定，刷新所有 SignalMerger 的显示状态
        foreach (var element in ElectricElements.Values) {
            if (element is SignalMerger merger) {
                merger.Activate();
            }
        }

        // 处理 SignalBooster：仅当同格子叠加有 intensity >= 1 的电线时触发，
        // 把 boostValue 永久加到该电线所属电源的 workIntensity 上，然后销毁自身
        bool anyBoosterTriggered = false;
        foreach (var element in ElectricElements.Values.ToList()) {
            if (element is not SignalBooster booster || booster.bindGrid == null) continue;

            Wire activeWire = null;

            // 仅检查同格子叠加的电线，忽略上下左右邻居格子
            foreach (var obj in booster.bindGrid.holdObjects) {
                if (obj == null || obj == booster.gameObject) continue;
                if (obj.TryGetComponent(out Wire sameWire) && sameWire.intensity >= 1) {
                    activeWire = sameWire;
                    break;
                }
            }

            if (activeWire != null) {
                // 找到电线所属的电源，只增强它
                PowerSource ps = activeWire.sourcePower;
                if (ps != null) {
                    ps.workIntensity += booster.boostValue;
                    Debug.Log($"SignalBooster 触发：电源 [{ps.name}] workIntensity +{booster.boostValue} → {ps.workIntensity}，booster 销毁");
                } else {
                    Debug.LogWarning($"SignalBooster 触发：电线没有 sourcePower 记录，跳过电源增强");
                }

                // 记录销毁前的位置和格子，触发后在原位置重新放置一根电线（流程与玩家鼠标点击一致）
                GridV2 boosterCell = booster.bindGrid;
                PlayBoosterTriggerSound();
                booster.Remove();

                if (boosterCell != null) {
                    boosterCell.PutElement(CellType.Wire);

                    // 与 WirePlacer.ConfirmWires 一致：找到 Wire 实例，隐藏 SpriteRenderer，由 wireTilemap 负责渲染 tile
                    Wire newWire = null;
                    foreach (var obj in boosterCell.holdObjects) {
                        if (obj != null && obj.TryGetComponent(out Wire w)) { newWire = w; break; }
                    }
                    if (newWire != null) {
                        var sr = newWire.GetComponent<SpriteRenderer>();
                        if (sr != null) sr.enabled = false;
                    }
                }

                anyBoosterTriggered = true;
            }
        }

        // 如果有 booster 被触发，电源变强了，重新计算电路
        if (anyBoosterTriggered) {
            BeginSimulate();
            return;
        }

        // 处理 SignalAmplifier：当自身被信号激活（intensity >= workIntensity）时，
        // 给该信号源头电源加 boostValue（一次性）
        bool anyAmplifierTriggered = false;
        foreach (var element in ElectricElements.Values) {
            if (element is not SignalAmplifier amp || amp.bindGrid == null) continue;
            if (amp.hasBuffedPower) continue;   // 同一 amp 只触发一次

            // 激活条件：自身 intensity 达到 workIntensity（与"被信号点亮"同义）
            if (amp.intensity < Mathf.Max(1, amp.workIntensity)) continue;

            PowerSource ps = amp.sourcePower;
            if (ps != null) {
                ps.workIntensity += amp.boostValue;
                Debug.Log($"SignalAmplifier 触发：电源 [{ps.name}] workIntensity +{amp.boostValue} → {ps.workIntensity}");
                amp.hasBuffedPower = true;
                anyAmplifierTriggered = true;
            } else {
                Debug.LogWarning($"SignalAmplifier 触发：自身没有 sourcePower 记录，跳过电源增强");
            }
        }

        // 如果有 amplifier 被触发，电源变强了，重新计算电路
        if (anyAmplifierTriggered) {
            BeginSimulate();
            return;
        }

        // 检查所有灯：硬性条件 —— 相邻 Wire 的最大 intensity >= 灯自身的 workIntensity（激活阈值）才点亮
        foreach (var element in ElectricElements.Values) {
            if (element is Light light) {
                int maxNeighborIntensity = 0;
                foreach (var neighbor in light.neighborElements) {
                    if (neighbor is Wire && neighbor.intensity > maxNeighborIntensity) {
                        maxNeighborIntensity = neighbor.intensity;
                    }
                }
                light.intensity = maxNeighborIntensity;
                if (maxNeighborIntensity >= light.workIntensity) {
                    light.Activate();
                } else {
                    light.Deactive();
                }
            }
        }
    }

    /// <summary>为 SignalMerger 三种方向变体自动复制 SignalMerger 的 prefab。
    /// SignalMerger 使用 SpriteRenderer 渲染，不走 elementTilemap，因此不复制 tile 配置。</summary>
    void CopySignalMergerVariants() {
        CellType[] variants = { CellType.SignalMergerDown, CellType.SignalMergerLeft, CellType.SignalMergerRight };

        if (prefabDict.TryGetValue(CellType.SignalMerger, out GameObject mergerPrefab)) {
            foreach (var variant in variants) {
                if (!prefabDict.ContainsKey(variant))
                    prefabDict[variant] = mergerPrefab;
            }
        }
    }

    void RefreshAllWireTiles() {
        if (wireTilemap == null) return;
        var gmv2 = GridManagerV2.Instance;
        if (gmv2 == null) return;

        for (int y = 0; y < gmv2.row; y++) {
            for (int x = 0; x < gmv2.column; x++) {
                GridV2 cell = gmv2.GetGrid(x, y);
                if (cell == null || cell.holdObject == null) continue;

                var wire = cell.holdObject.GetComponent<Wire>();
                if (wire == null) continue;

                RefreshWireTile(x, y, wire.intensity > 0);
            }
        }
    }

    // ---------- SignalMerger 辅助方法 ----------

    /// <summary>获取指定格子中所有元件的最大 intensity</summary>
    static int GetMaxIntensityAt(GridManagerV2 gmv2, int x, int y) {
        if (gmv2 == null) return 0;
        GridV2 cell = gmv2.GetGrid(x, y);
        if (cell == null) return 0;

        int max = 0;
        foreach (var obj in cell.holdObjects) {
            if (obj != null && obj.TryGetComponent(out ElectricElementBase e)) {
                max = Mathf.Max(max, e.intensity);
            }
        }
        return max;
    }

    /// <summary>从指定元件开始，向四周传播信号（SignalMerger 除外）</summary>
    static void PropagateFrom(ElectricElementBase start, int initialIntensity) {
        Queue<(ElectricElementBase elem, int intensity)> queue = new();
        HashSet<ElectricElementBase> visited = new();

        queue.Enqueue((start, initialIntensity));

        while (queue.Count > 0) {
            var (cur, intensity) = queue.Dequeue();
            if (visited.Contains(cur)) continue;
            visited.Add(cur);

            cur.intensity = intensity;
            if (intensity >= cur.workIntensity) {
                cur.Activate();
            }

            if (cur is not Wire && cur is not SignalAmplifier) continue;

            foreach (var next in cur.neighborElements) {
                if (next is SignalMerger) continue;
                if (next is SignalBooster) continue;   // booster 不参与 BFS / 额外传播
                if (visited.Contains(next)) continue;

                int outgoing = Mathf.Max(0, intensity - 1);

                if (next.intensity < outgoing) {
                    next.intensity = outgoing;
                    next.sourcePower = cur.sourcePower;  // 继承 cur 的 sourcePower
                    queue.Enqueue((next, outgoing));
                }
            }
        }
    }

    // ----------

    public void AddElement(ElectricElementBase electricElement) {
        electricElement.ID = curId;
        ElectricElements.Add(electricElement.ID, electricElement);
        ++curId;
    }

    public void RemoveElement(ElectricElementBase electricElementBase) {
        ElectricElements.Remove(electricElementBase.ID);
        Destroy(electricElementBase.gameObject);
    }

    /// <summary>清理所有元件状态和 tilemap，用于切换关卡前的重置。
    /// 显式销毁所有元件 GameObject，并清空字典/电源/curId/三个 tilemap。</summary>
    public void ClearAll() {
        // 先收集快照，避免遍历过程中字典被 OnDestroy 修改
        var toDestroy = ElectricElements.Values.ToList();

        // 先清空字典，避免 OnDestroy 中的 Remove 找不到 key
        ElectricElements.Clear();
        curId = 0;
        powerSource = null;

        // 销毁所有元件 GameObject（OnDestroy 中的 BeginSimulate 因 powerSource 为 null 会立即 return）
        foreach (var element in toDestroy) {
            if (element != null && element.gameObject != null) {
                Destroy(element.gameObject);
            }
        }

        if (wireTilemap != null) wireTilemap.ClearAllTiles();
        if (elementTilemap != null) elementTilemap.ClearAllTiles();
        if (previewTilemap != null) previewTilemap.ClearAllTiles();
    }

    // ---------- 音效 ----------

    void EnsureAudioSource() {
        if (audioSource != null) return;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    /// <summary>播放电线放置音效（玩家拖拽确认放置后调用）</summary>
    public void PlayWirePlaceSound() {
        if (wirePlaceClip == null) return;
        EnsureAudioSource();
        audioSource.PlayOneShot(wirePlaceClip);
    }

    /// <summary>播放电线销毁音效（玩家擦除时调用）</summary>
    public void PlayWireRemoveSound() {
        if (wireRemoveClip == null) return;
        EnsureAudioSource();
        audioSource.PlayOneShot(wireRemoveClip);
    }

    /// <summary>播放信号补充球自我销毁音效</summary>
    public void PlayBoosterTriggerSound() {
        if (boosterTriggerClip == null) return;
        EnsureAudioSource();
        audioSource.PlayOneShot(boosterTriggerClip);
    }

    class UnionFind {
        private int[] parent;

        public UnionFind(int n) {
            parent = new int[n];
            for (int i = 0; i < n; i++)
                parent[i] = i;
        }

        public int Find(int x) {
            if (parent[x] != x)
                parent[x] = Find(parent[x]);
            return parent[x];
        }

        public bool Union(int a, int b) {
            int rootA = Find(a);
            int rootB = Find(b);

            if (rootA == rootB) {
                return false;
            }

            parent[rootA] = rootB;
            return true;
        }
    }

    public bool CheckRing() {
        UnionFind uf = new UnionFind(curId);

        foreach (var kv in ElectricElements) {
            var element = kv.Value;

            foreach (var neighbor in element.neighborElements) {
                if (element.ID < neighbor.ID) {
                    if (!uf.Union(element.ID, neighbor.ID)) {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}
