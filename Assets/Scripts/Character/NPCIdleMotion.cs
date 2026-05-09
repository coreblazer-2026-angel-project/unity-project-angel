using UnityEngine;

/// <summary>
/// 控制 NPC 待机微动效果
/// - 上下浮动
/// - 呼吸缩放
/// - 轻微旋转
/// - Shadow 跟随缩放
/// - Label 轻微上下浮动
/// </summary>
public class NPCIdleMotion : MonoBehaviour
{
    [Header("浮动设置")]
    public float floatAmount = 0.06f;       // 上下浮动幅度
    public float floatSpeed = 1.35f;        // 浮动速度

    [Header("呼吸缩放")]
    public float scaleAmount = 0.035f;      // 缩放幅度
    public float scaleSpeed = 1.1f;         // 缩放速度

    [Header("旋转")]
    public float rotateAmount = 1.8f;       // Z轴旋转幅度
    public float rotateSpeed = 0.9f;        // 旋转速度

    [Header("Shadow 脚下影子")]
    public Transform shadow;                // Shadow 节点
    public float shadowScaleMin = 0.9f;
    public float shadowScaleMax = 1.1f;

    [Header("时间偏移（让多个 NPC 不同步）")]
    public float timeOffset = 0f;

    // 内部记录初始值
    private Vector3 startPos;
    private Vector3 startScale;
    private float startRotZ;
    private Vector3 shadowStartScale;

    void Start()
    {
        startPos = transform.localPosition;
        startScale = transform.localScale;
        startRotZ = transform.localEulerAngles.z;

        if (shadow != null)
            shadowStartScale = shadow.localScale;


        // 随机时间偏移，让多个 NPC 不同步
        if (timeOffset == 0f)
            timeOffset = Random.Range(0f, 10f);
    }

    void Update()
    {
        float t = Time.time + timeOffset;

        // 上下浮动
        float yOffset = Mathf.Sin(t * floatSpeed) * floatAmount;
        transform.localPosition = startPos + new Vector3(0f, yOffset, 0f);

        // 呼吸缩放
        float scaleOffset = 1f + Mathf.Sin(t * scaleSpeed) * scaleAmount;
        transform.localScale = startScale * scaleOffset;

        //  轻微旋转
        float zRot = startRotZ + Mathf.Sin(t * rotateSpeed) * rotateAmount;
        transform.localRotation = Quaternion.Euler(0f, 0f, zRot);

        // Shadow 缩放跟随
        if (shadow != null)
        {
            float shadowScale = Mathf.Lerp(shadowScaleMin, shadowScaleMax, (Mathf.Sin(t * scaleSpeed) + 1f) / 2f);
            shadow.localScale = shadowStartScale * shadowScale;
        }


    }

    [ContextMenu("Apply Stronger Idle Motion")]
    void ApplyStrongerIdleMotion()
    {
        floatAmount = 0.06f;
        floatSpeed = 1.35f;
        scaleAmount = 0.035f;
        scaleSpeed = 1.1f;
        rotateAmount = 1.8f;
        rotateSpeed = 0.9f;
        shadowScaleMin = 0.9f;
        shadowScaleMax = 1.1f;
    }
}
