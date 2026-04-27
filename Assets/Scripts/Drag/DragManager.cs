using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 拖拽管理器。管理通过鼠标左键拖拽场景中的物件。
/// 可拖拽的物件在 Inspector 中配置，也可以通过代码动态增删。
/// </summary>
public class DragManager : MonoBehaviour {
    [Header("相机（留空则自动取 Camera.main）")]
    public Camera cam;

    [Header("可拖拽物件列表")]
    [Tooltip("在 Inspector 中配置可以被拖拽的 GameObject")]
    public List<GameObject> draggableObjects = new();

    [Header("网格吸附")]
    [Tooltip("松开后是否吸附到网格")]
    public bool snapToGridOnRelease = false;

    [Tooltip("吸附网格大小（与 GridManagerV2 保持一致）")]
    public float gridSize = 0.32f;

    Camera Cam => cam != null ? cam : Camera.main;

    bool _isDragging;
    GameObject _currentDragObject;
    Vector3 _dragOffset;

    void Update() {
        if (Input.GetMouseButtonDown(0))
            TryStartDrag();

        if (_isDragging && Input.GetMouseButton(0))
            UpdateDrag();

        if (Input.GetMouseButtonUp(0))
            EndDrag();
    }

    void TryStartDrag() {
        if (Cam == null) return;

        Vector3 worldPos = Cam.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0f;

        GameObject target = PickDraggableObject(worldPos);
        if (target == null) return;

        // 检查接口权限
        if (target.TryGetComponent<IDraggable>(out var draggable) && !draggable.CanDrag)
            return;

        _isDragging = true;
        _currentDragObject = target;
        _dragOffset = target.transform.position - worldPos;

        draggable?.OnDragStart(worldPos);
    }

    void UpdateDrag() {
        if (_currentDragObject == null) return;

        Vector3 worldPos = Cam.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0f;

        _currentDragObject.transform.position = worldPos + _dragOffset;

        if (_currentDragObject.TryGetComponent<IDraggable>(out var draggable))
            draggable.OnDragging(worldPos);
    }

    void EndDrag() {
        if (!_isDragging || _currentDragObject == null) return;

        Vector3 worldPos = Cam.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0f;

        // 网格吸附
        if (snapToGridOnRelease) {
            Vector3 pos = _currentDragObject.transform.position;
            pos.x = Mathf.Round(pos.x / gridSize) * gridSize;
            pos.y = Mathf.Round(pos.y / gridSize) * gridSize;
            _currentDragObject.transform.position = pos;
        }

        if (_currentDragObject.TryGetComponent<IDraggable>(out var draggable))
            draggable.OnDragEnd(worldPos);

        _isDragging = false;
        _currentDragObject = null;
    }

    /// <summary>
    /// 根据世界坐标从可拖拽列表中找出目标物件。
    /// 优先使用 Collider2D 射线检测，未命中时回退到距离检测。
    /// </summary>
    GameObject PickDraggableObject(Vector3 worldPos) {
        // 优先射线检测（需要目标挂载 Collider2D）
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null) {
            GameObject hitObj = hit.collider.gameObject;
            if (draggableObjects.Contains(hitObj))
                return hitObj;
        }

        // 回退：按距离检测（不需要 Collider2D）
        float minSqrDistance = float.MaxValue;
        GameObject closest = null;
        float thresholdSqr = (gridSize * 0.6f) * (gridSize * 0.6f);

        foreach (var obj in draggableObjects) {
            if (obj == null) continue;
            float sqrDist = ((Vector2)obj.transform.position - (Vector2)worldPos).sqrMagnitude;
            if (sqrDist < thresholdSqr && sqrDist < minSqrDistance) {
                minSqrDistance = sqrDist;
                closest = obj;
            }
        }

        return closest;
    }

    // ---------- 动态增删接口 ----------

    /// <summary>将物件加入可拖拽列表</summary>
    public void AddDraggable(GameObject obj) {
        if (obj != null && !draggableObjects.Contains(obj))
            draggableObjects.Add(obj);
    }

    /// <summary>将物件从可拖拽列表移除</summary>
    public void RemoveDraggable(GameObject obj) {
        draggableObjects.Remove(obj);
    }

    /// <summary>清空可拖拽列表</summary>
    public void ClearDraggables() {
        draggableObjects.Clear();
    }
}
