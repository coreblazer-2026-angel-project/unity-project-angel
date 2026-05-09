using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour {

    [Header("背景")]
    [Tooltip("场景中的背景 SpriteRenderer。LoadLevel 时把 LevelData.background 设到它上面。留空则不切换背景。")]
    public SpriteRenderer backgroundRenderer;

    public void LoadLevel(LevelData levelData) {
        var gmv2 = GridManagerV2.Instance;
        if (gmv2 == null) {
            Debug.LogError("LevelManager: GridManagerV2 不存在，无法加载关卡");
            return;
        }

        // 切换关卡前先清空所有旧元件和 tilemap 状态，避免上一关的电强残留
        ElectricManager.Instance?.ClearAll();

        // 切换关卡背景（LevelData.background 为 null 时保留上一关背景）
        if (backgroundRenderer != null && levelData.background != null) {
            backgroundRenderer.sprite = levelData.background;
        }

        // 若配置了 CSV 且未勾选 useInlineItems，从 CSV 解析 items
        levelData.ParseCSV();

        // 调试：打印关卡所有 PowerSource 的 signalStrength，方便确认数据是否正确
        Debug.Log($"LevelManager: 加载 [{levelData.name}], useInlineItems={levelData.useInlineItems}, csvData={(levelData.csvData != null ? levelData.csvData.name : "null")}, items={levelData.items?.Count}");
        if (levelData.items != null) {
            foreach (var dItem in levelData.items) {
                if (dItem.type == CellType.PowerSource) {
                    Debug.Log($"  → PowerSource id={dItem.elementId}, pos={dItem.position}, signalStrength={dItem.signalStrength}");
                }
            }
        }

        if (levelData.items == null || levelData.items.Count == 0) {
            Debug.LogWarning("LevelManager: 关卡没有元件数据");
            return;
        }

        // 根据关卡数据的 width/height 重建网格
        if (levelData.width > 0 && levelData.height > 0) {
            gmv2.BuildGridForLevel(levelData.width, levelData.height);
        }

        gmv2.OnLevelLoaded();

        // elementId -> 运行时元件映射，用于建立显式连接
        Dictionary<int, ElectricElementBase> idToElement = new();

        foreach (var item in levelData.items) {
            if (item.type == CellType.Empty)
                continue;

            // 墙：只在 elementTilemap 上放 RuleTile，不创建 ElectricElement，不参与电路
            // 同时标记 GridV2.noPlace，禁止任何元件（包括玩家拖动的电线）放在墙上
            if (item.type == CellType.Wall) {
                GridV2 wallCell = gmv2.GetGrid(item.position.x, item.position.y);
                if (wallCell != null) wallCell.noPlace = true;

                var emWall = ElectricManager.Instance;
                if (emWall != null && emWall.HasElementTile(CellType.Wall)) {
                    emWall.SetElementTile(item.position.x, item.position.y, CellType.Wall, false);
                }
                continue;
            }

            // 不可放置区：标记 GridV2.noPlace，禁止后续在此格放任何元件，可选放 tile 显示
            if (item.type == CellType.NoPlaceZone) {
                GridV2 npCell = gmv2.GetGrid(item.position.x, item.position.y);
                if (npCell != null) npCell.noPlace = true;

                var emNp = ElectricManager.Instance;
                if (emNp != null && emNp.HasElementTile(CellType.NoPlaceZone)) {
                    emNp.SetElementTile(item.position.x, item.position.y, CellType.NoPlaceZone, false);
                }
                continue;
            }

            GridV2 cell = gmv2.GetGrid(item.position.x, item.position.y);
            if (cell == null) {
                Debug.LogWarning($"LevelManager: 坐标 {item.position} 超出网格范围，跳过");
                continue;
            }

            cell.PutElement(item.type);

            // 遍历所有物体找到刚放置的元件（holdObject 只保留第一个，可能不是本次放置的）
            foreach (var obj in cell.holdObjects) {
                if (obj == null) continue;
                if (!obj.TryGetComponent(out ElectricElementBase elem)) continue;

                // 记录 elementId 映射
                if (item.elementId >= 0) {
                    elem.levelElementId = item.elementId;
                    idToElement[item.elementId] = elem;
                }

                // 默认把 signalStrength 设到 workIntensity（多数元件都用这个）
                elem.workIntensity = item.signalStrength;

                // 按类型应用 signalStrength 的具体语义
                switch (item.type) {
                    // 电源类：signalStrength = 电源强度（intensity 和 workIntensity 都设）
                    case CellType.PowerSource:
                    case CellType.ActivatablePower:
                    case CellType.PressSource: {
                        int strength = item.signalStrength > 0 ? item.signalStrength : 3;
                        elem.intensity = strength;
                        elem.workIntensity = strength;
                        if (elem is PowerSource ps && ElectricManager.Instance != null)
                            ElectricManager.Instance.powerSource = ps;
                        break;
                    }

                    // 放大器：signalStrength = 给电源加的电强（boostValue）
                    case CellType.SignalAmplifier: {
                        if (elem is SignalAmplifier amp && item.signalStrength > 0)
                            amp.boostValue = item.signalStrength;
                        elem.workIntensity = 1; // 放大器自身阈值给个小值，便于被点亮
                        break;
                    }

                    // 增幅球：signalStrength = 给电源加的电强（boostValue）
                    case CellType.SignalBooster: {
                        if (elem is SignalBooster booster && item.signalStrength > 0)
                            booster.boostValue = item.signalStrength;
                        elem.workIntensity = 1;
                        break;
                    }

                    // 灯：signalStrength = 激活阈值（workIntensity，默认 1）
                    case CellType.HopeLamp: {
                        elem.workIntensity = item.signalStrength > 0 ? item.signalStrength : 1;
                        break;
                    }

                    // 电线：忽略 signalStrength（电线没有自身强度概念）
                    case CellType.Wire: {
                        elem.workIntensity = 0;
                        break;
                    }

                    // 十字交叉：忽略 signalStrength（中继元件，无阈值）
                    case CellType.CrossConnector: {
                        elem.workIntensity = 0;
                        break;
                    }

                    // 合并器（4 个方向）：signalStrength 作为输出门槛，默认 0
                    case CellType.SignalMerger:
                    case CellType.SignalMergerDown:
                    case CellType.SignalMergerLeft:
                    case CellType.SignalMergerRight: {
                        // 保持默认 workIntensity = signalStrength
                        break;
                    }

                    // 其他类型保持默认 workIntensity = signalStrength
                    default: break;
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
