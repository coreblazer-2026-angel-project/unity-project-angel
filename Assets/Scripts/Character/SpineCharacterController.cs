using Spine.Unity;
using UnityEngine;

/// <summary>
/// WASD 控制角色移动，自动切换 Spine idle/walk 动画。
/// 使用方式：挂到带有 SkeletonAnimation 组件的 GameObject 上。
/// </summary>
public class SpineCharacterController : MonoBehaviour
{
    [Header("移动")]
    public float moveSpeed = 3f;

    [Header("Spine 动画名称")]
    [SpineAnimation] public string idleAnimation = "idle";
    [SpineAnimation] public string walkAnimation = "walk";

    [Header("动画混合")]
    [Tooltip("idle ↔ walk 过渡时长（秒）")]
    public float crossfadeDuration = 0.15f;

    SkeletonAnimation _skeleton;
    string _currentAnim;

    void Awake()
    {
        _skeleton = GetComponent<SkeletonAnimation>();
    }

    void Start()
    {
        SetAnimation(idleAnimation, true);
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A/D
        float v = Input.GetAxisRaw("Vertical");   // W/S
        Vector2 input = new Vector2(h, v);

        if (input.sqrMagnitude > 0.01f)
        {
            Vector2 dir = input.normalized;
            transform.Translate(dir * moveSpeed * Time.deltaTime, Space.World);

            SetAnimation(walkAnimation, true);

            if (h != 0f)
                _skeleton.Skeleton.ScaleX = h < 0 ? -1f : 1f;
        }
        else
        {
            SetAnimation(idleAnimation, true);
        }
    }

    void SetAnimation(string animName, bool loop)
    {
        if (_currentAnim == animName) return;
        _currentAnim = animName;
        _skeleton.AnimationState.SetAnimation(0, animName, loop).MixDuration = crossfadeDuration;
    }
}
