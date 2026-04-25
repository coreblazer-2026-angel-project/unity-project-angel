using UnityEngine;

/// <summary>
/// 鼠标左键放置 / 删除电线。
/// 电路模拟由 ElectricElementBase 在 Start/OnDestroy 中自动触发。
/// </summary>
public class WirePlacer : MonoBehaviour {
    [Header("相机（留空则自动取 Camera.main）")]
    public Camera cam;

    Camera Cam => cam != null ? cam : Camera.main;

    void Update() {
        if (Input.GetMouseButtonDown(0))
            HandleClick();
    }

    void HandleClick() {
        if (Cam == null) return;

        Vector3 world = Cam.ScreenToWorldPoint(Input.mousePosition);
        world.z = 0f;

        var gmv2 = GridManagerV2.Instance;
        if (gmv2 == null) return;

        Vector2Int gridPos = WorldToGrid(world, gmv2.gridSize);
        GridV2 cell = gmv2.GetGrid(gridPos.x, gridPos.y);
        if (cell == null) return;

        if (cell.holdObject != null && cell.holdObject.GetComponent<Wire>() != null)
            DeleteWire(cell);
        else if (cell.holdObject == null)
            TryPlaceWire(cell);
    }

    void TryPlaceWire(GridV2 cell) {
        cell.PutElement(CellType.Wire);
        // 模拟由 Wire.Start() 自动触发
    }

    void DeleteWire(GridV2 cell) {
        if (cell.holdObject != null) {
            var elem = cell.holdObject.GetComponent<ElectricElementBase>();
            if (elem != null) elem.Remove();
        }
        // 模拟由 ElectricElementBase.OnDestroy() 自动触发
    }

    static Vector2Int WorldToGrid(Vector3 world, float gridSize) {
        int gx = Mathf.RoundToInt(world.x / gridSize);
        int gy = Mathf.RoundToInt(-world.y / gridSize);
        return new Vector2Int(gx, gy);
    }
}
