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

    void AdaptBackground()
    {
        float screenRatio = (float)Screen.width / Screen.height;
        float designRatio = designWidth / designHeight;

        // 超宽屏横向延展
        if (screenRatio > designRatio)
        {
            float extraWidth = Screen.width - Screen.height * designRatio;
            backgroundRect.sizeDelta = new Vector2(backgroundRect.sizeDelta.x + extraWidth, backgroundRect.sizeDelta.y);
        }
        else
        {
            // 正常或窄屏，保持原始宽度
            backgroundRect.sizeDelta = new Vector2(designWidth, backgroundRect.sizeDelta.y);
        }
    }
}