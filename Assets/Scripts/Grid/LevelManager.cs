using UnityEngine;

public class LevelManager : MonoBehaviour {
    [Header("预制体配置")]
    public GameObject[] cellPrefabs; // 按照 CellType 的 int 值对应

    public void LoadLevel(LevelData levelData) {
        // 让 GridManager 初始化地图尺寸；地板可运行时生成，或仅在编辑模式下烘焙到场景后由 spawnRuntimeFloor=false 跳过重复生成
        GridManager.Instance.InitializeGrid(
            levelData.width,
            levelData.height,
            levelData.floorPrefab,
            skipFloorEntirely: !levelData.spawnRuntimeFloor);

        // 根据数据生成实体并注册进 GridManager
        foreach (var item in levelData.items) {
            SpawnAndRegisterEntity(item);
        }
    }

    private void SpawnAndRegisterEntity(LevelItem item) {
        int prefabIndex = (int)item.type;
        if (prefabIndex < 0 || prefabIndex >= cellPrefabs.Length || cellPrefabs[prefabIndex] == null) return;

        // 生成物体
        GameObject go = Instantiate(cellPrefabs[prefabIndex]);
        go.name = $"{item.type}_{item.position.x}_{item.position.y}";

        // 获取实体组件
        IGridEntity entity = go.GetComponent<IGridEntity>();
        if (entity != null) {
            // 将自己初始化特定的数据
            var gridCell = entity as GridCell;
            if (gridCell != null) {
                gridCell.InitByLevelItem(item);
            }

            // 让 GridManager 接管它的定位
            if (GridManager.Instance.PlaceEntity(item.position, entity)) {
                GridManager.Instance.SetCellType(item.position, item.type);
            }
        }
    }
}