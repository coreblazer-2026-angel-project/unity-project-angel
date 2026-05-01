using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class ResponsiveUIScale : MonoBehaviour
{
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920f, 1080f);
    [SerializeField] private float referenceScale = 1f;
    [SerializeField] private float minScale = 0.75f;
    [SerializeField] private float maxScale = 1.45f;
    [SerializeField, Range(0f, 1f)] private float widthHeightMatch = 0.5f;

    private RectTransform rectTransform;
    private Vector2 lastParentSize;

    private void OnEnable()
    {
        rectTransform = GetComponent<RectTransform>();
        ApplyScale();
    }

    private void Update()
    {
        ApplyScale();
    }

    private void OnRectTransformDimensionsChange()
    {
        ApplyScale();
    }

    private void ApplyScale()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        RectTransform parent = rectTransform.parent as RectTransform;
        Vector2 currentSize = parent != null && parent.rect.width > 0f && parent.rect.height > 0f
            ? parent.rect.size
            : new Vector2(Screen.width, Screen.height);

        if (currentSize == lastParentSize && Application.isPlaying)
        {
            return;
        }

        lastParentSize = currentSize;

        float widthScale = currentSize.x / referenceResolution.x;
        float heightScale = currentSize.y / referenceResolution.y;
        float scale = Mathf.Lerp(widthScale, heightScale, widthHeightMatch) * referenceScale;
        scale = Mathf.Clamp(scale, minScale, maxScale);

        rectTransform.localScale = new Vector3(scale, scale, 1f);
    }
}
