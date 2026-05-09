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
    public string idleAnim = "idle";
    public string walkAnim = "walk";
    public float animationMix = 0.15f;
    public float moveInputThreshold = 0.01f;
    public float idleReturnDelay = 0.15f;
    public bool randomizeIdleStartTime = true;

    [Header("脚下影子")]
    public Transform shadow;
    public Vector3 shadowBaseScale = new Vector3(0.98f, 0.27f, 0.2404757f);
    [Tooltip("朝右走：光在前面，影子在身后，短一些。")]
    public Vector2 shadowFacingRightOffset = new Vector2(-0.08f, -0.85f);
    public Vector2 shadowFacingRightScaleMultiplier = new Vector2(0.9f, 1f);
    [Tooltip("朝左走：光在身后，影子在前面，拉得更长。")]
    public Vector2 shadowFacingLeftOffset = new Vector2(-0.22f, -0.85f);
    public Vector2 shadowFacingLeftScaleMultiplier = new Vector2(1.35f, 1f);
    public float shadowFacingSmooth = 14f;

    [Header("平滑移动")]
    public bool useSmoothMove = true;
    public float smoothFactor = 10f;

    Vector2 _inputDir;
    Vector2 _targetPos;
    string _currentAnim;
    float _lastMoveTime;
    bool _facingRight = true;
    Vector3 _shadowTargetLocalPosition;
    Vector3 _shadowTargetLocalScale;

    void Start()
    {
        if (skeletonAnimation == null)
            skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();

        _targetPos = transform.position;
        if (shadow != null)
            shadow.localScale = shadowBaseScale;

        ApplyFacing(true, true);
        PlaySpineAnimation(idleAnim, true, false);
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
            ApplyFacing(true, false);
        else if (_inputDir.x < 0)
            ApplyFacing(false, false);

        UpdateShadow();

        // 动画：有方向输入播 walk，没输入播 idle
        bool hasMoveInput = _inputDir.sqrMagnitude > moveInputThreshold * moveInputThreshold;
        if (hasMoveInput)
            _lastMoveTime = Time.time;

        bool shouldPlayWalk = hasMoveInput;
        bool shouldReturnIdle = !hasMoveInput && Time.time - _lastMoveTime >= idleReturnDelay;

        if (shouldPlayWalk)
            PlaySpineAnimation(walkAnim, true, false);
        else if (shouldReturnIdle)
            PlaySpineAnimation(idleAnim, true, randomizeIdleStartTime);
    }

    void PlaySpineAnimation(string animationName, bool loop, bool randomizeStartTime)
    {
        if (skeletonAnimation == null || string.IsNullOrEmpty(animationName))
            return;

        if (_currentAnim == animationName)
            return;

        if (skeletonAnimation.skeleton.Data.FindAnimation(animationName) == null) {
            Debug.LogWarning($"[SideScrollPlayer] Spine animation not found: {animationName}", this);
            return;
        }

        skeletonAnimation.state.Data.DefaultMix = animationMix;
        var entry = skeletonAnimation.state.SetAnimation(0, animationName, loop);
        if (randomizeStartTime && entry.Animation != null && entry.Animation.Duration > 0f)
            entry.TrackTime = Random.Range(0f, entry.Animation.Duration);

        _currentAnim = animationName;
    }

    void ApplyFacing(bool facingRight, bool instant)
    {
        _facingRight = facingRight;

        if (skeletonAnimation != null)
            skeletonAnimation.skeleton.ScaleX = facingRight ? 1 : -1;

        _shadowTargetLocalPosition = facingRight
            ? new Vector3(shadowFacingRightOffset.x, shadowFacingRightOffset.y, 0f)
            : new Vector3(shadowFacingLeftOffset.x, shadowFacingLeftOffset.y, 0f);

        Vector2 scaleMultiplier = facingRight
            ? shadowFacingRightScaleMultiplier
            : shadowFacingLeftScaleMultiplier;

        _shadowTargetLocalScale = new Vector3(
            shadowBaseScale.x * scaleMultiplier.x,
            shadowBaseScale.y * scaleMultiplier.y,
            shadowBaseScale.z
        );

        if (instant && shadow != null) {
            float z = shadow.localPosition.z;
            shadow.localPosition = new Vector3(_shadowTargetLocalPosition.x, _shadowTargetLocalPosition.y, z);
            shadow.localScale = _shadowTargetLocalScale;
        }
    }

    void UpdateShadow()
    {
        if (shadow == null)
            return;

        float z = shadow.localPosition.z;
        Vector3 targetPos = new Vector3(_shadowTargetLocalPosition.x, _shadowTargetLocalPosition.y, z);
        float t = 1f - Mathf.Exp(-shadowFacingSmooth * Time.deltaTime);

        shadow.localPosition = Vector3.Lerp(shadow.localPosition, targetPos, t);
        shadow.localScale = Vector3.Lerp(shadow.localScale, _shadowTargetLocalScale, t);
    }
}
