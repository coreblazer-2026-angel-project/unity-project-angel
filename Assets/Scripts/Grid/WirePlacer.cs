using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 鼠标左键放置 / 删除电线，支持按住拖动批量预览放置。
/// 拖动时显示透明度 50% 的预览电线，松开后转为真正的电线（SpriteRenderer 隐藏，由 Tilemap 渲染）。
/// </summary>
public class WirePlacer : MonoBehaviour {
    [Header("相机（留空则自动取 Camera.main）")]
    public Camera cam;

    Camera Cam => cam != null ? cam : Camera.main;

    bool isDragging;
    Vector2Int? lastGridPos;
    Dictionary<Vector2Int, GameObject> previewObjects = new();
    Sprite wireSprite;

    void Start() {
        var em = ElectricManager.Instance;
        if (em?.previewTilemap != null) {
            // 预览层整体半透明
            em.previewTilemap.color = new Color(1f, 1f, 1f, 0.5f);
        } else {
            // 无预览 Tilemap，退化为 SpriteRenderer 方案（无自动连接效果）
            if (em?.prefabDict.TryGetValue(CellType.Wire, out GameObject wirePrefab) == true) {
                if (wirePrefab.TryGetComponent<SpriteRenderer>(out var sr)) {
                    wireSprite = sr.sprite;
                }
            }
        }
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            isDragging = true;
            HandleDragStart();
        }

        if (isDragging && Input.GetMouseButton(0)) {
            HandleDrag();
        }

        if (Input.GetMouseButtonUp(0)) {
            if (isDragging) {
                ConfirmWires();
                isDragging = false;
                lastGridPos = null;
            }
        }
    }

    void HandleDragStart() {
        if (Cam == null) return;

        Vector3 world = Cam.ScreenToWorldPoint(Input.mousePosition);
        world.z = 0f;

        var gmv2 = GridManagerV2.Instance;
        if (gmv2 == null) return;

        Vector2Int gridPos = WorldToGrid(world, gmv2.gridSize);
        lastGridPos = gridPos;

        GridV2 cell = gmv2.GetGrid(gridPos.x, gridPos.y);
        if (cell == null) {
            isDragging = false;
            return;
        }

        // 如果起点已有电线，删除它并结束拖动
        if (cell.holdObject != null && cell.holdObject.GetComponent<Wire>() != null) {
            DeleteWire(cell);
            isDragging = false;
            lastGridPos = null;
            return;
        }

        // 起点为空，创建起点预览
        if (cell.holdObject == null) {
            CreatePreview(cell, gridPos);
        } else {
            // 起点被非电线物体占据，取消拖动
            isDragging = false;
            lastGridPos = null;
        }
    }

    void HandleDrag() {
        if (Cam == null) return;

        Vector3 world = Cam.ScreenToWorldPoint(Input.mousePosition);
        world.z = 0f;

        var gmv2 = GridManagerV2.Instance;
        if (gmv2 == null) return;

        Vector2Int gridPos = WorldToGrid(world, gmv2.gridSize);
        if (lastGridPos.HasValue && gridPos == lastGridPos.Value) return;

        // 从上一个格子到当前格子画线，填充中间格子（防止鼠标移动过快漏掉格子）
        Vector2Int from = lastGridPos ?? gridPos;
        foreach (Vector2Int pos in GetLinePoints(from, gridPos)) {
            if (previewObjects.ContainsKey(pos)) continue;

            GridV2 cell = gmv2.GetGrid(pos.x, pos.y);
            if (cell == null || cell.holdObject != null) continue;

            CreatePreview(cell, pos);
        }

        lastGridPos = gridPos;
    }

    void CreatePreview(GridV2 cell, Vector2Int gridPos) {
        var em = ElectricManager.Instance;

        // 优先使用 previewTilemap（RuleTile 自动变化连接方向）
        if (em?.previewTilemap != null) {
            previewObjects[gridPos] = null;
            em.SetPreviewWireTile(gridPos.x, gridPos.y);
            return;
        }

        // 退化为 SpriteRenderer 方案
        GameObject go = null;
        if (wireSprite != null) {
            go = new GameObject("PreviewWire");
            go.transform.SetParent(cell.transform);
            go.transform.localPosition = Vector3.zero;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = wireSprite;
            sr.sortingOrder = 10;
            Color c = sr.color;
            c.a = 0.5f;
            sr.color = c;
        }

        previewObjects[gridPos] = go;
    }

    void ConfirmWires() {
        var em = ElectricManager.Instance;
        em?.ClearAllPreviewTiles();

        var gmv2 = GridManagerV2.Instance;
        if (gmv2 == null) {
            ClearPreviews();
            return;
        }

        foreach (var kv in previewObjects) {
            Vector2Int pos = kv.Key;
            GameObject previewGo = kv.Value;
            if (previewGo != null) Destroy(previewGo);

            GridV2 cell = gmv2.GetGrid(pos.x, pos.y);
            if (cell != null && cell.holdObject == null) {
                cell.PutElement(CellType.Wire);
                // 隐藏 Wire 的 SpriteRenderer，由 Tilemap 负责渲染
                if (cell.holdObject != null) {
                    var sr = cell.holdObject.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.enabled = false;
                }
            }
        }

        previewObjects.Clear();
    }

    void ClearPreviews() {
        ElectricManager.Instance?.ClearAllPreviewTiles();
        foreach (var go in previewObjects.Values) {
            if (go != null) Destroy(go);
        }
        previewObjects.Clear();
    }

    void DeleteWire(GridV2 cell) {
        if (cell.holdObject != null) {
            var elem = cell.holdObject.GetComponent<ElectricElementBase>();
            if (elem != null) elem.Remove();
        }
    }

    static Vector2Int WorldToGrid(Vector3 world, float gridSize) {
        int gx = Mathf.RoundToInt(world.x / gridSize);
        int gy = Mathf.RoundToInt(-world.y / gridSize);
        return new Vector2Int(gx, gy);
    }

    // Bresenham 线段算法：返回从 from 到 to 经过的所有整数坐标点
    static IEnumerable<Vector2Int> GetLinePoints(Vector2Int from, Vector2Int to) {
        int x0 = from.x, y0 = from.y;
        int x1 = to.x, y1 = to.y;
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true) {
            yield return new Vector2Int(x0, y0);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx) {
                err += dx;
                y0 += sy;
            }
        }
    }
}
