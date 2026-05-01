using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridV2 : MonoBehaviour {
    public int x;
    public int y;

    /// <summary>兼容旧代码，返回第一个放置的物体</summary>
    public GameObject holdObject;

    /// <summary>当前格子内所有放置的物体（支持叠加）</summary>
    public List<GameObject> holdObjects = new();

    void Start() {

    }

    void Update() {

    }

    public void PutElement(CellType cellType) {
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

        // 可叠加类型：Wire / SignalAmplifier 可以共存
        bool isOverlayType = cellType == CellType.Wire || cellType == CellType.SignalAmplifier;

        // 非叠加类型独占格子
        if (!isOverlayType && holdObjects.Count > 0) {
            Debug.LogWarning($"GridV2.PutElement: 坐标 ({x},{y}) 已有其他元件，[{cellType}] 无法叠加放置");
            return;
        }

        // 叠加类型只能和 Wire / SignalAmplifier 共存
        if (isOverlayType) {
            foreach (var obj in holdObjects) {
                if (obj == null) continue;
                if (obj.TryGetComponent(out ElectricElementBase elem)) {
                    if (elem.cellType != CellType.Wire && elem.cellType != CellType.SignalAmplifier) {
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
        holdObjects.Add(spawnGameObject);
        if (holdObject == null) holdObject = spawnGameObject;

        if (spawnGameObject.TryGetComponent(out ElectricElementBase electricElement)) {
            electricElement.cellType = cellType;
            electricElement.BindToGrid(this);
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
