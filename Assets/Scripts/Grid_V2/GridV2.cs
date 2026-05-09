using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridV2 : MonoBehaviour {
    public int x;
    public int y;

    /// <summary>不可放置标记：true 时此格子禁止放置任何元件</summary>
    public bool noPlace;

    /// <summary>兼容旧代码，返回第一个放置的物体</summary>
    public GameObject holdObject;

    /// <summary>当前格子内所有放置的物体（支持叠加）</summary>
    public List<GameObject> holdObjects = new();

    [Header("边框")]
    [Tooltip("勾选后在 Start 时生成 32x32（gridSize）的方框边线")]
    public bool showBorder = true;
    [Tooltip("边线宽度（世界单位）")]
    public float borderWidth = 0.01f;
    [Tooltip("边线颜色")]
    public Color borderColor = Color.white;

    void Start() {
        if (showBorder) CreateBorder();
    }

    void Update() {

    }

    /// <summary>在自身位置画一个 gridSize × gridSize 的方框（中心对齐）</summary>
    void CreateBorder() {
        float size = GridManagerV2.Instance != null ? GridManagerV2.Instance.ScaledGridSize : 0.32f;
        float half = size / 2f;

        var go = new GameObject("Border");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.zero;

        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = false;
        lr.positionCount = 5;
        float scaledWidth = borderWidth * (size / 0.32f);
        lr.startWidth = scaledWidth;
        lr.endWidth = scaledWidth;
        lr.startColor = borderColor;
        lr.endColor = borderColor;
        lr.material = new Material(Shader.Find("Sprites/Default"));

        lr.SetPosition(0, new Vector3(-half, -half, 0));
        lr.SetPosition(1, new Vector3( half, -half, 0));
        lr.SetPosition(2, new Vector3( half,  half, 0));
        lr.SetPosition(3, new Vector3(-half,  half, 0));
        lr.SetPosition(4, new Vector3(-half, -half, 0));   // 闭合
    }

    public void PutElement(CellType cellType) {
        if (noPlace) {
            Debug.Log($"GridV2.PutElement: 坐标 ({x},{y}) 是不可放置区，跳过 [{cellType}]");
            return;
        }

        if (ElectricManager.Instance == null) {
            Debug.LogError($"GridV2.PutElement: ElectricManager 实例不存在，无法放置 {cellType}");
            return;
        }

        if (!ElectricManager.Instance.prefabDict.TryGetValue(cellType, out GameObject prefab) || prefab == null) {
            Debug.LogError($"GridV2.PutElement: CellType [{cellType}] 没有对应的预制体。请在 ElectricManager 的 prefabEntries 中配置。");
            return;
        }

        // 同类型不能重复放置
        foreach (var obj in holdObjects) {
            if (obj == null) continue;
            if (obj.TryGetComponent(out ElectricElementBase elem) && elem.cellType == cellType) {
                Debug.Log($"GridV2.PutElement: 坐标 ({x},{y}) 已有 [{cellType}]，跳过重复放置");
                return;
            }
        }

        // 可叠加类型：Wire / SignalAmplifier / SignalBooster 可以共存
        bool isOverlayType = cellType == CellType.Wire
            || cellType == CellType.SignalAmplifier
            || cellType == CellType.SignalBooster;

        // 非叠加类型独占格子
        if (!isOverlayType && holdObjects.Count > 0) {
            Debug.LogWarning($"GridV2.PutElement: 坐标 ({x},{y}) 已有其他元件，[{cellType}] 无法叠加放置");
            return;
        }

        // 叠加类型只能和 Wire / SignalAmplifier / SignalBooster 共存
        if (isOverlayType) {
            foreach (var obj in holdObjects) {
                if (obj == null) continue;
                if (obj.TryGetComponent(out ElectricElementBase elem)) {
                    if (elem.cellType != CellType.Wire
                        && elem.cellType != CellType.SignalAmplifier
                        && elem.cellType != CellType.SignalBooster) {
                        Debug.LogWarning($"GridV2.PutElement: 坐标 ({x},{y}) 已有非叠加元件 [{elem.cellType}]，无法放置 [{cellType}]");
                        return;
                    }
                }
            }
        }

        var prefabComponent = prefab.GetComponent<ElectricElementBase>();
        if (prefabComponent != null) {
            Debug.Log($"GridV2.PutElement: 正在放置 [{cellType}]，prefab 上的组件类型是 [{prefabComponent.GetType().Name}]");
        }

        GameObject spawnGameObject = Instantiate(prefab, transform);

        // 缩放物件以匹配动态网格大小
        var gm = GridManagerV2.Instance;
        if (gm != null && gm.gridSize > 0) {
            float s = gm.ScaledGridSize / gm.gridSize;
            spawnGameObject.transform.localScale = Vector3.Scale(
                spawnGameObject.transform.localScale, new Vector3(s, s, 1f));
        }

        holdObjects.Add(spawnGameObject);
        if (holdObject == null) holdObject = spawnGameObject;

        if (spawnGameObject.TryGetComponent(out ElectricElementBase electricElement)) {
            electricElement.cellType = cellType;
            electricElement.BindToGrid(this);

            // Wire 创建时立即在 wireTilemap 上设置默认 tile（不依赖 Wire.Start，因为 Start 要等下一帧）
            if (electricElement is Wire) {
                var em = ElectricManager.Instance;
                if (em != null && em.wireTilemap != null) {
                    em.SetWireTile(this.x, this.y, em.wireTileUnpowered);
                }
            }

            Debug.Log($"GridV2.PutElement: 实例化完成，实际组件类型 = [{electricElement.GetType().Name}]，cellType = {electricElement.cellType}");
        } else {
            Debug.LogError($"GridV2.PutElement: 实例化出来的对象没有 ElectricElementBase 组件！cellType = {cellType}");
        }
    }

    [ContextMenu("Debug PutElement Wire")]
    public void DebugPutElementWire() {
        PutElement(CellType.Wire);
    }

    [ContextMenu("Debug PutElement Light")]
    public void DebugPutElementLight() {
        PutElement(CellType.HopeLamp);
    }

    public GridV2[] GetAllNeighbors() {
        return new[] {
            GridManagerV2.Instance.GetGrid(x,y-1),
            GridManagerV2.Instance.GetGrid(x,y+1),
            GridManagerV2.Instance.GetGrid(x-1,y),
            GridManagerV2.Instance.GetGrid(x+1,y),
        };
    }
}
