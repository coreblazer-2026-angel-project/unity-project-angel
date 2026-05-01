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

    // 拖拽阈值
    const float DRAG_THRESHOLD = 0.125f;

    bool _isDragging;
    GameObject _currentDragObject;
    Vector3 _dragOffset;

    // 待确认拖拽状态
    GameObject _pendingTarget;
    Vector3 _pendingWorldPos;
    float _pendingTime;

    void Update() {
        // 左键按下：检测目标，进入待确认状态
        if (Input.GetMouseButtonDown(0)) {
            if (Cam != null) {
                Vector3 worldPos = Cam.ScreenToWorldPoint(Input.mousePosition);
                worldPos.z = 0f;
                GameObject target = PickDraggableObject(worldPos);
                if (target != null && target.TryGetComponent(out IDraggable d) && d.CanDrag) {
                    _pendingTarget = target;
                    _pendingWorldPos = worldPos;
                    _pendingTime = Time.time;
                }
            }
        }

        // 按住期间：达到阈值后正式开始拖拽
        if (_pendingTarget != null && Input.GetMouseButton(0)) {
            if (Time.time - _pendingTime >= DRAG_THRESHOLD) {
                StartDragging(_pendingTarget, _pendingWorldPos);
                _pendingTarget = null;
            }
        }

        if (_isDragging && Input.GetMouseButton(0))
            UpdateDrag();

        // 松开：取消待确认 或 结束拖拽
        if (Input.GetMouseButtonUp(0)) {
            if (_pendingTarget != null) {
                _pendingTarget = null; // 未达阈值，取消
            } else if (_isDragging) {
                EndDrag();
            }
        }
    }

    void StartDragging(GameObject target, Vector3 worldPos) {
        _isDragging = true;
        _currentDragObject = target;
        _dragOffset = target.transform.position - worldPos;
        target.GetComponent<IDraggable>()?.OnDragStart(worldPos);
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
    /// 优先使用 Collider2D 射线检测，未命中时回退到距离检测，最后回退到网格坐标检测。
    /// </summary>
    GameObject PickDraggableObject(Vector3 worldPos) {
        // 1. 优先射线检测（需要目标挂载 Collider2D）
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null) {
            GameObject hitObj = hit.collider.gameObject;
            if (draggableObjects.Contains(hitObj))
                return hitObj;
        }

        // 2. 回退：按距离检测（不需要 Collider2D，阈值约一个格子大小）
        float minSqrDistance = float.MaxValue;
        GameObject closest = null;
        float thresholdSqr = gridSize * gridSize; // 放宽到整格范围

        foreach (var obj in draggableObjects) {
            if (obj == null) continue;
            float sqrDist = ((Vector2)obj.transform.position - (Vector2)worldPos).sqrMagnitude;
            if (sqrDist < thresholdSqr && sqrDist < minSqrDistance) {
                minSqrDistance = sqrDist;
                closest = obj;
            }
        }
        if (closest != null) return closest;

        // 3. 最终回退：通过网格坐标查找（适合 Tilemap 渲染且无 Collider2D 的元件）
        var gmv2 = GridManagerV2.Instance;
        if (gmv2 != null) {
            int gx = Mathf.RoundToInt(worldPos.x / gmv2.gridSize);
            int gy = Mathf.RoundToInt(-worldPos.y / gmv2.gridSize);
            GridV2 cell = gmv2.GetGrid(gx, gy);
            if (cell != null) {
                foreach (var obj in cell.holdObjects) {
                    if (obj != null && draggableObjects.Contains(obj))
                        return obj;
                }
            }
        }

        return null;
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
