using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour {

    public void LoadLevel(LevelData levelData) {
        var gmv2 = GridManagerV2.Instance;
        if (gmv2 == null) {
            Debug.LogError("LevelManager: GridManagerV2 不存在，无法加载关卡");
            return;
        }

        // 若配置了 CSV 且未勾选 useInlineItems，从 CSV 解析 items
        levelData.ParseCSV();

        if (levelData.items == null || levelData.items.Count == 0) {
            Debug.LogWarning("LevelManager: 关卡没有元件数据");
            return;
        }

        gmv2.OnLevelLoaded();

        // elementId -> 运行时元件映射，用于建立显式连接
        Dictionary<int, ElectricElementBase> idToElement = new();

        foreach (var item in levelData.items) {
            if (item.type == CellType.Empty || item.type == CellType.Wall)
                continue;

            GridV2 cell = gmv2.GetGrid(item.position.x, item.position.y);
            if (cell == null) {
                Debug.LogWarning($"LevelManager: 坐标 {item.position} 超出网格范围，跳过");
                continue;
            }

            cell.PutElement(item.type);

            // 遍历所有物体找到刚放置的元件（holdObject 只保留第一个，可能不是本次放置的）
            foreach (var obj in cell.holdObjects) {
                if (obj == null) continue;
                if (obj.TryGetComponent(out ElectricElementBase elem)) {
                    elem.workIntensity = item.signalStrength;
                    if (item.elementId >= 0) {
                        elem.levelElementId = item.elementId;
                        idToElement[item.elementId] = elem;
                    }
                    if (elem is PowerSource ps) {
                        // signalStrength 为 0 时给默认强度，避免整路不激活
                        int strength = item.signalStrength > 0 ? item.signalStrength : 3;
                        elem.intensity = strength;
                        elem.workIntensity = strength;
                        // 将运行时创建的电源注册给 ElectricManager，BeginSimulate 才能找到它
                        if (ElectricManager.Instance != null)
                            ElectricManager.Instance.powerSource = ps;
                    }
                }
            }
        }

        // 建立显式连接（CSV 中 connections 字段定义）
        foreach (var item in levelData.items) {
            if (item.connections == null || item.connections.Count == 0) continue;
            if (!idToElement.TryGetValue(item.elementId, out var fromElem)) continue;

            foreach (int toId in item.connections) {
                if (!idToElement.TryGetValue(toId, out var toElem)) {
                    Debug.LogWarning($"LevelManager: elementId {item.elementId} 的连接目标 {toId} 不存在");
                    continue;
                }
                if (!fromElem.neighborElements.Contains(toElem)) {
                    fromElem.neighborElements.Add(toElem);
                }
                if (!toElem.neighborElements.Contains(fromElem)) {
                    toElem.neighborElements.Add(fromElem);
                }
            }
        }

        // 为所有非电线元件在 wireTilemap 上放置隐形电线瓦片，让 RuleTile 能正确连接
        var em = ElectricManager.Instance;
        if (em != null && em.wireTilemap != null) {
            foreach (var element in em.ElectricElements.Values) {
                if (element is Wire) continue;
                if (element.bindGrid == null) continue;

                Vector3Int cellPos = em.GetTilePos(element.bindGrid.x, element.bindGrid.y);
                if (em.wireTilemap.GetTile(cellPos) == null) {
                    TileBase tile = element.intensity > 0 ? em.wireTilePowered : em.wireTileUnpowered;
                    em.SetWireTile(element.bindGrid.x, element.bindGrid.y, tile);
                    em.wireTilemap.SetColor(cellPos, Color.clear);
                }
            }
        }

        // 关卡加载完成后，同步一次电路状态
        if (em != null)
            em.BeginSimulate();
    }
}
