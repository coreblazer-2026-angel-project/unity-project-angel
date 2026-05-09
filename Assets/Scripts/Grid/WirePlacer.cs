using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 鼠标左键放置 / 删除电线，支持按住拖动逐格铺设。
/// 拖动时每经过一个格子立即放置电线，松手后运行电路模拟。
/// </summary>
public class WirePlacer : MonoBehaviour {
    [Header("相机（留空则自动取 Camera.main）")]
    public Camera cam;

    Camera Cam => cam != null ? cam : Camera.main;

    bool isDragging;
    Vector2Int dragStartGridPos;
    Vector2Int? lastGridPos;
    HashSet<Vector2Int> placedCells = new();

    // 右键长按擦除
    bool isRightMouseDown;
    float rightMouseDownTime;
    bool isErasing;
    Vector2Int? lastEraseGridPos;
    const float LONG_PRESS_THRESHOLD = 0.125f;

    void Update() {
        // 左键：放置 / 拖动
        if (Input.GetMouseButtonDown(0)) {
            isDragging = true;
            placedCells.Clear();
            HandleDragStart();
        }

        if (isDragging && Input.GetMouseButton(0)) {
            HandleDrag();
        }

        if (Input.GetMouseButtonUp(0)) {
            if (isDragging) {
                if (placedCells.Count > 0) {
                    ElectricManager.Instance?.BeginSimulate();
                    ElectricManager.Instance?.PlayWirePlaceSound();
                }
                isDragging = false;
                lastGridPos = null;
                placedCells.Clear();
            }
        }

        // 右键：长按擦除
        if (Input.GetMouseButtonDown(1)) {
            if (isDragging) {
                CancelDrag();
            }
            isRightMouseDown = true;
            rightMouseDownTime = Time.time;
        }

        if (isRightMouseDown && Input.GetMouseButton(1)) {
            if (!isErasing && Time.time - rightMouseDownTime >= LONG_PRESS_THRESHOLD) {
                isErasing = true;
            }

            if (isErasing) {
                EraseWiresUnderCursor();
            }
        }

        if (Input.GetMouseButtonUp(1)) {
            isRightMouseDown = false;
            isErasing = false;
            lastEraseGridPos = null;
        }
    }

    void HandleDragStart() {
        if (Cam == null) return;

        Vector3 world = Cam.ScreenToWorldPoint(Input.mousePosition);
        world.z = 0f;

        var gmv2 = GridManagerV2.Instance;
        if (gmv2 == null) return;

        Vector2Int gridPos = WorldToGrid(world, gmv2.ScaledGridSize);
        dragStartGridPos = gridPos;
        lastGridPos = gridPos;

        GridV2 cell = gmv2.GetGrid(gridPos.x, gridPos.y);
        if (cell == null) {
            isDragging = false;
            return;
        }

        // 起点已有电线 → 删除并结束
        if (HasWire(cell)) {
            DeleteWire(cell);
            ElectricManager.Instance?.BeginSimulate();
            isDragging = false;
            lastGridPos = null;
            return;
        }

        // 起点直接放置电线
        if (CanPlaceWire(cell)) {
            PlaceWireAt(cell);
            placedCells.Add(gridPos);
        } else {
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

        Vector2Int gridPos = WorldToGrid(world, gmv2.ScaledGridSize);
        if (lastGridPos.HasValue && gridPos == lastGridPos.Value) return;

        // 从上一次位置到当前位置，用 Bresenham 填补中间格子
        Vector2Int from = lastGridPos ?? gridPos;
        foreach (Vector2Int pos in GetLinePoints(from, gridPos)) {
            if (placedCells.Contains(pos)) continue;

            GridV2 cell = gmv2.GetGrid(pos.x, pos.y);
            if (cell == null || !CanPlaceWire(cell)) continue;

            PlaceWireAt(cell);
            placedCells.Add(pos);
        }

        lastGridPos = gridPos;
    }

    void PlaceWireAt(GridV2 cell) {
        cell.PutElement(CellType.Wire);
        Wire wire = GetWire(cell);
        if (wire != null) {
            var sr = wire.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
        }
    }

    void CancelDrag() {
        // 右键取消：撤销本次拖动放置的所有电线
        var gmv2 = GridManagerV2.Instance;
        if (gmv2 != null) {
            foreach (var pos in placedCells) {
                GridV2 cell = gmv2.GetGrid(pos.x, pos.y);
                if (cell != null) DeleteWire(cell);
            }
            ElectricManager.Instance?.BeginSimulate();
        }
        isDragging = false;
        lastGridPos = null;
        placedCells.Clear();
    }

    void DeleteWire(GridV2 cell) {
        Wire wire = GetWire(cell);
        if (wire != null) wire.Remove();
    }

    // ---------- 右键长按擦除 ----------

    void EraseWiresUnderCursor() {
        if (Cam == null) return;

        Vector3 world = Cam.ScreenToWorldPoint(Input.mousePosition);
        world.z = 0f;

        var gmv2 = GridManagerV2.Instance;
        if (gmv2 == null) return;

        Vector2Int gridPos = WorldToGrid(world, gmv2.ScaledGridSize);

        if (lastEraseGridPos.HasValue && gridPos == lastEraseGridPos.Value) return;
        lastEraseGridPos = gridPos;

        GridV2 cell = gmv2.GetGrid(gridPos.x, gridPos.y);
        if (cell == null) return;

        if (HasWire(cell)) {
            DeleteWire(cell);
        }
    }

    // ---------- 辅助方法 ----------

    static bool HasWire(GridV2 cell) => GetWire(cell) != null;

    static Wire GetWire(GridV2 cell) {
        if (cell == null) return null;
        foreach (var obj in cell.holdObjects) {
            if (obj != null && obj.TryGetComponent(out Wire wire)) return wire;
        }
        return null;
    }

    static bool CanPlaceWire(GridV2 cell) {
        if (cell == null) return false;
        if (cell.noPlace) return false;
        if (cell.holdObjects.Count == 0) return true;
        foreach (var obj in cell.holdObjects) {
            if (obj == null) continue;
            if (obj.GetComponent<Wire>() != null) return false;
            if (obj.GetComponent<SignalAmplifier>() == null
                && obj.GetComponent<SignalBooster>() == null)
                return false;
        }
        return true;
    }

    static Vector2Int WorldToGrid(Vector3 world, float gridSize) {
        var gmv2 = GridManagerV2.Instance;
        if (gmv2 != null) {
            Vector3 origin = gmv2.GridOrigin;
            float gs = gmv2.ScaledGridSize;
            int gx = Mathf.FloorToInt((world.x - origin.x) / gs);
            int gy = Mathf.FloorToInt((origin.y + gs - world.y) / gs);
            return new Vector2Int(gx, gy);
        }
        int gx2 = Mathf.RoundToInt(world.x / gridSize);
        int gy2 = Mathf.RoundToInt(-world.y / gridSize);
        return new Vector2Int(gx2, gy2);
    }

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
