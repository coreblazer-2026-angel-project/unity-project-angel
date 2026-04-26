using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridV2 : MonoBehaviour {
    public int x;
    public int y;
    public GameObject holdObject;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void PutElement(CellType cellType) {
        if (holdObject) return;

        if (ElectricManager.Instance == null) {
            Debug.LogError($"GridV2.PutElement: ElectricManager 实例不存在，无法放置 {cellType}");
            return;
        }

        if (!ElectricManager.Instance.prefabDict.TryGetValue(cellType, out GameObject prefab) || prefab == null) {
            Debug.LogError($"GridV2.PutElement: CellType [{cellType}] 没有对应的预制体。请在 ElectricManager 的 prefabEntries 中配置。");
            return;
        }

        // 诊断日志：确认将要实例化的 prefab 上的组件类型
        var prefabComponent = prefab.GetComponent<ElectricElementBase>();
        if (prefabComponent != null) {
            Debug.Log($"GridV2.PutElement: 正在放置 [{cellType}]，prefab 上的组件类型是 [{prefabComponent.GetType().Name}]");
        }

        GameObject spawnGameObject = Instantiate(prefab, transform);
        holdObject = spawnGameObject;
        if (spawnGameObject.TryGetComponent<ElectricElementBase>(out var electricElement)) {
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
