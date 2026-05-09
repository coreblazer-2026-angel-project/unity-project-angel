using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HeadUIFloat : MonoBehaviour
{
    [Header("整体呼吸缩放")]
    public Transform titleText;           // TitleText 节点
    public float scaleAmount = 0.03f;     // 呼吸幅度
    public float scaleSpeed = 2f;         // 呼吸速度

    [Header("Marker 上下浮动")]
    public RectTransform markerText;
    public float markerFloatAmount = 6f;  // 浮动像素
    public float markerFloatSpeed = 2.5f; // 浮动速度

    [Header("左右箭头随 Marker 浮动")]
    public RectTransform leftArrowText;
    public RectTransform rightArrowText;

    [Header("StatusText 淡入淡出")]
    public CanvasGroup statusTextCanvasGroup;
    public float fadeSpeed = 5f;
    public bool isVisible = true;

    [Header("时间偏移，多个 NPC 可以错开节奏")]
    public float timeOffset = 0f;

    // 内部记录初始状态
    private Vector3 titleStartScale;
    private Vector2 markerStartPos;
    private Vector2 leftArrowStartPos;
    private Vector2 rightArrowStartPos;

    void Start()
    {
        if (titleText != null) titleStartScale = titleText.localScale;
        if (markerText != null) markerStartPos = markerText.anchoredPosition;
        if (leftArrowText != null) leftArrowStartPos = leftArrowText.anchoredPosition;
        if (rightArrowText != null) rightArrowStartPos = rightArrowText.anchoredPosition;

        if (timeOffset == 0f)
            timeOffset = Random.Range(0f, 10f); // 每个 NPC 自动错开
    }

    void Update()
    {
        float t = Time.time + timeOffset;

        // TitleText 缩放呼吸
        if (titleText != null)
        {
            float scale = 1f + Mathf.Sin(t * scaleSpeed) * scaleAmount;
            titleText.localScale = titleStartScale * scale;
        }

        //  MarkerText 上下浮动
        if (markerText != null)
        {
            float yOffset = Mathf.Sin(t * markerFloatSpeed) * markerFloatAmount;
            markerText.anchoredPosition = markerStartPos + new Vector2(0, yOffset);

            // 左右箭头随 Marker 浮动
            if (leftArrowText != null)
                leftArrowText.anchoredPosition = leftArrowStartPos + new Vector2(0, yOffset);
            if (rightArrowText != null)
                rightArrowText.anchoredPosition = rightArrowStartPos + new Vector2(0, yOffset);
        }

        // StatusText 淡入淡出
        if (statusTextCanvasGroup != null)
        {
            float targetAlpha = isVisible ? 1f : 0f;
            statusTextCanvasGroup.alpha = Mathf.Lerp(statusTextCanvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
        }
    }

    /// <summary>
    /// 设置头顶提示显示或隐藏
    /// </summary>
    /// <param name="visible"></param>
    public void SetVisible(bool visible)
    {
        isVisible = visible;
    }
}