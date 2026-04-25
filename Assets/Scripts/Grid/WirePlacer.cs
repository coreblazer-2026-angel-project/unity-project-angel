using UnityEngine;

/// <summary>
/// 鼠标左键放置 / 删除电线。
/// 只负责交互逻辑，Tilemap 显示由 ElectricManager 和 Wire 自身管理。
/// </summary>
public class WirePlacer : MonoBehaviour {
    [Header("相机（留空则自动取 Camera.main）")]
    public Camera cam;

    int _wireCount;

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
        _wireCount++;
        var em = ElectricManager.Instance;
        if (em != null) em.BeginSimulate();
    }

    void DeleteWire(GridV2 cell) {
        if (cell.holdObject != null) {
            var elem = cell.holdObject.GetComponent<ElectricElementBase>();
            if (elem != null) elem.Remove();
        }

        _wireCount = Mathf.Max(0, _wireCount - 1);
        var em = ElectricManager.Instance;
        if (em != null) em.BeginSimulate();
    }

    static Vector2Int WorldToGrid(Vector3 world, float gridSize) {
        int gx = Mathf.RoundToInt(world.x / gridSize);
        int gy = Mathf.RoundToInt(-world.y / gridSize);
        return new Vector2Int(gx, gy);
    }
}
