using UnityEngine;

public class DynamicBackground : MonoBehaviour
{
    public RectTransform backgroundRect;
    public float designWidth = 3840f;
    public float designHeight = 2160f;

    void Start()
    {
        AdaptBackground();
    }

    void OnRectTransformDimensionsChange()
    {
        AdaptBackground();
    }

    void AdaptBackground()
    {
        if (backgroundRect == null || designHeight <= 0f)
        {
            return;
        }

        RectTransform parentRect = backgroundRect.parent as RectTransform;
        float targetWidth = parentRect != null && parentRect.rect.width > 0f ? parentRect.rect.width : Screen.width;
        float targetHeight = parentRect != null && parentRect.rect.height > 0f ? parentRect.rect.height : Screen.height;
        float designRatio = designWidth / designHeight;
        float targetRatio = targetWidth / targetHeight;

        float width;
        float height;

        if (targetRatio > designRatio)
        {
            width = targetWidth;
            height = width / designRatio;
        }
        else
        {
            height = targetHeight;
            width = height * designRatio;
        }

        backgroundRect.anchorMin = new Vector2(0.5f, 0.5f);
        backgroundRect.anchorMax = new Vector2(0.5f, 0.5f);
        backgroundRect.pivot = new Vector2(0.5f, 0.5f);
        backgroundRect.anchoredPosition = Vector2.zero;
        backgroundRect.localScale = Vector3.one;
        backgroundRect.sizeDelta = new Vector2(width, height);
    }
}
