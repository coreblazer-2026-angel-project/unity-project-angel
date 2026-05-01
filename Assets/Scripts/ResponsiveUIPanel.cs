using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class ResponsiveUIPanel : MonoBehaviour
{
    [SerializeField] private Vector2 referenceSize = new Vector2(890f, 550f);
    [SerializeField, Range(0.1f, 1f)] private float maxScreenWidth = 0.72f;
    [SerializeField, Range(0.1f, 1f)] private float maxScreenHeight = 0.78f;
    [SerializeField] private Vector2 minSize = new Vector2(560f, 360f);

    private RectTransform rectTransform;
    private Vector2 lastParentSize;

    private void OnEnable()
    {
        rectTransform = GetComponent<RectTransform>();
        ApplySize();
    }

    private void Update()
    {
        ApplySize();
    }

    private void OnRectTransformDimensionsChange()
    {
        ApplySize();
    }

    private void ApplySize()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        RectTransform parent = rectTransform.parent as RectTransform;
        if (parent == null || parent.rect.width <= 0f || parent.rect.height <= 0f)
        {
            return;
        }

        Vector2 parentSize = parent.rect.size;
        if (parentSize == lastParentSize && Application.isPlaying)
        {
            return;
        }

        lastParentSize = parentSize;

        float scale = Mathf.Min(
            parentSize.x * maxScreenWidth / referenceSize.x,
            parentSize.y * maxScreenHeight / referenceSize.y
        );

        Vector2 targetSize = referenceSize * scale;
        targetSize.x = Mathf.Max(targetSize.x, minSize.x);
        targetSize.y = Mathf.Max(targetSize.y, minSize.y);

        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localScale = Vector3.one;
        rectTransform.sizeDelta = targetSize;
    }
}
