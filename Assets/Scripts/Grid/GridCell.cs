using UnityEngine;

// 具体的元器件类（不再管网格是怎么生成的，只管自己）
public class GridCell : MonoBehaviour, IGridEntity {
    // 实现接口要求
    public Vector2Int GridPosition { get; set; }
    public GameObject HoldObject { get; set; }

    public void PutElement(CellType cellType) {
        ElectricManager.Instance.prefabDict.TryGetValue(cellType, out GameObject prefab);
        GameObject spawnGameObject = Instantiate(prefab, this.gameObject.transform.position, this.gameObject.transform.rotation);
        this.HoldObject = spawnGameObject;
        if (spawnGameObject.TryGetComponent<ElectricElementBase>(out var electricElement)) {
            electricElement.bindGrid = this;
        }
    }

    public void InitByLevelItem(LevelItem levelItem) {

    }

    [ContextMenu("Debug PutElement")]
    public void DebugPutElement() {
        PutElement(CellType.Wire);
    }
}