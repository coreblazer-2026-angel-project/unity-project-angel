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
        ElectricManager.Instance.prefabDict.TryGetValue(cellType, out GameObject prefab);
        GameObject spawnGameObject = Instantiate(prefab, transform);
        holdObject = spawnGameObject;
        if (spawnGameObject.TryGetComponent<ElectricElementBase>(out var electricElement)) {
            electricElement.BindToGrid(this);
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
