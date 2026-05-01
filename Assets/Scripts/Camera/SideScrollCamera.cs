using UnityEngine;

public class SideScrollCamera : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform target;

    [Header("死区半径（世界单位）")]
    public float deadZoneHalf = 2f;

    [Header("背景边界（世界坐标）")]
    public float bgMinX;
    public float bgMaxX;
    public float fixedY;

    [Header("平滑速度")]
    public float smoothSpeed = 8f;

    float _camTargetX;

    void Start()
    {
        _camTargetX = transform.position.x;
    }

    float GetHalfWidth()
    {
        return Camera.main.orthographicSize * Camera.main.aspect;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float playerX = target.position.x;

        // 死区检测：角色超出死区才移动相机目标
        float diff = playerX - _camTargetX;
        if (diff < -deadZoneHalf)
            _camTargetX = playerX + deadZoneHalf;
        else if (diff > deadZoneHalf)
            _camTargetX = playerX - deadZoneHalf;

        // 钳制到背景范围
        float hw = GetHalfWidth();
        _camTargetX = Mathf.Clamp(_camTargetX, bgMinX + hw, bgMaxX - hw);

        // 平滑移动
        Vector3 pos = transform.position;
        pos.x = Mathf.Lerp(pos.x, _camTargetX, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));
        pos.y = fixedY;
        pos.z = -10f;
        transform.position = pos;
    }
}
