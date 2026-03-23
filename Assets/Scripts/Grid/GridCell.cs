using UnityEngine;

// 具体的元器件类（不再管网格是怎么生成的，只管自己）
public class GridCell : MonoBehaviour, IGridEntity {
    public Vector2Int GridPosition { get; set; }

    public GameObject HoldObject { get; set; }

    public void PutElement(CellType cellType) {
        ElectricManager.Instance.prefabDict.TryGetValue(cellType, out GameObject prefab);
        GameObject spawnGameObject = Instantiate(prefab, this.gameObject.transform.position, this.gameObject.transform.rotation);
        this.HoldObject = spawnGameObject;
        if (spawnGameObject.TryGetComponent<ElectricElementBase>(out var electricElement)) {
            electricElement.BindToGrid(this);
        }
    }

    public void InitByLevelItem(LevelItem levelItem) {

    }

    [ContextMenu("Debug PutElement")]
    public void DebugPutElement() {
        PutElement(CellType.Wire);
    }


    public IGridEntity GetNeighbor(Vector2Int currentPos, Vector2Int direction) {
        Vector2Int neighborPos = currentPos + direction;
        return GridManager.Instance.GetEntity(neighborPos);
    }

    // 查一圈房（
    public IGridEntity[] GetAllNeighbors(Vector2Int pos) {
        return new IGridEntity[]
        {
            GridManager.Instance.GetEntity(pos + Vector2Int.up),
            GridManager.Instance.GetEntity(pos + Vector2Int.down),
            GridManager.Instance.GetEntity(pos + Vector2Int.left),
            GridManager.Instance.GetEntity(pos + Vector2Int.right)
        };
    }

    public IGridEntity[] GetAllNeighbors() {
        return GetAllNeighbors(GridManager.Instance.WorldToGrid(this.transform.position));
    }
}