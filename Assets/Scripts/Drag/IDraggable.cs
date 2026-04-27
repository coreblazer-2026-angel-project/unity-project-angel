using UnityEngine;

/// <summary>
/// 可拖拽物件接口。实现此接口的组件可以接收拖拽生命周期回调。
/// </summary>
public interface IDraggable {
    /// <summary>当前是否可以被拖拽</summary>
    bool CanDrag { get; }

    /// <summary>拖拽开始时调用</summary>
    void OnDragStart(Vector3 worldPos);

    /// <summary>拖拽过程中每帧调用</summary>
    void OnDragging(Vector3 worldPos);

    /// <summary>拖拽结束时调用</summary>
    void OnDragEnd(Vector3 worldPos);
}
