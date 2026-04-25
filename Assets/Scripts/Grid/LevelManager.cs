using UnityEngine;

public class LevelManager : MonoBehaviour {

    public void LoadLevel(LevelData levelData) {
        var gmv2 = GridManagerV2.Instance;
        if (gmv2 == null) {
            Debug.LogError("LevelManager: GridManagerV2 不存在，无法加载关卡");
            return;
        }

        gmv2.OnLevelLoaded();

        foreach (var item in levelData.items) {
            if (item.type == CellType.Empty || item.type == CellType.Wall)
                continue;

            GridV2 cell = gmv2.GetGrid(item.position.x, item.position.y);
            if (cell == null) {
                Debug.LogWarning($"LevelManager: 坐标 {item.position} 超出网格范围，跳过");
                continue;
            }

            if (cell.holdObject != null) {
                Debug.LogWarning($"LevelManager: 坐标 {item.position} 已有元件，跳过");
                continue;
            }

            cell.PutElement(item.type);

            if (cell.holdObject != null) {
                var elem = cell.holdObject.GetComponent<ElectricElementBase>();
                if (elem != null) {
                    elem.workIntensity = item.signalStrength;
                    if (elem is PowerSource ps) {
                        elem.intensity = item.signalStrength;
                        // 将运行时创建的电源注册给 ElectricManager，BeginSimulate 才能找到它
                        if (ElectricManager.Instance != null && ElectricManager.Instance.powerSource == null)
                            ElectricManager.Instance.powerSource = ps;
                    }
                }
            }
        }

        // 关卡加载完成后，同步一次电路状态
        if (ElectricManager.Instance != null)
            ElectricManager.Instance.BeginSimulate();
    }
}
