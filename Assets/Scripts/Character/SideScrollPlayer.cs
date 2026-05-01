using UnityEngine;
using Spine.Unity;

public class SideScrollPlayer : MonoBehaviour
{
    [Header("移动速度")]
    public float moveSpeed = 5f;

    [Header("左右移动范围")]
    public float minX = -7.2f;
    public float maxX = 7.2f;

    [Header("上下移动范围（红框边界）")]
    public float minY = -2f;
    public float maxY = 2f;

    [Header("Spine")]
    public SkeletonAnimation skeletonAnimation;

    [Header("平滑移动")]
    public bool useSmoothMove = true;
    public float smoothFactor = 10f;

    const string AnimWalk = "walk";

    Vector2 _inputDir;
    Vector2 _targetPos;

    void Start()
    {
        _targetPos = transform.position;
    }

    void Update()
    {
        // 读取输入
        _inputDir.x = Input.GetAxisRaw("Horizontal");
        _inputDir.y = Input.GetAxisRaw("Vertical");

        // 计算目标位置
        _targetPos.x += _inputDir.x * moveSpeed * Time.deltaTime;
        _targetPos.y += _inputDir.y * moveSpeed * Time.deltaTime;

        // 钳制范围
        _targetPos.x = Mathf.Clamp(_targetPos.x, minX, maxX);
        _targetPos.y = Mathf.Clamp(_targetPos.y, minY, maxY);

        // 应用位置
        if (useSmoothMove)
        {
            Vector3 pos = transform.position;
            pos.x = Mathf.Lerp(pos.x, _targetPos.x, 1f - Mathf.Exp(-smoothFactor * Time.deltaTime));
            pos.y = Mathf.Lerp(pos.y, _targetPos.y, 1f - Mathf.Exp(-smoothFactor * Time.deltaTime));
            transform.position = pos;
        }
        else
        {
            transform.position = new Vector3(_targetPos.x, _targetPos.y, 0);
        }

        // 翻转朝向（左右）
        if (_inputDir.x > 0)
            skeletonAnimation.skeleton.ScaleX = 1;
        else if (_inputDir.x < 0)
            skeletonAnimation.skeleton.ScaleX = -1;

        // 动画：有方向输入播 walk，没输入回到 idle
        bool moving = _inputDir != Vector2.zero;
        if (skeletonAnimation.AnimationName != (moving ? AnimWalk : null))
        {
            if (moving)
                skeletonAnimation.state.SetAnimation(0, AnimWalk, true);
            else
                skeletonAnimation.state.SetEmptyAnimation(0, 0.2f);
        }
    }
}
