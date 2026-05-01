using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraAdapter : MonoBehaviour
{
    private Camera mainCam;

    void Start()
    {
        mainCam = GetComponent<Camera>();
        AdaptCamera();
    }

    void AdaptCamera()
    {
        float targetAspect = 16f / 9f;// 设计图比例
        float currentAspect = (float)Screen.width / Screen.height;

        if (currentAspect > targetAspect) // 带鱼屏
        {
            float scaleWidth = targetAspect / currentAspect;
            Rect rect = mainCam.rect;
            rect.width = scaleWidth;
            rect.height = 1f;
            rect.x = (1f - scaleWidth) / 2f;
            rect.y = 0f;
            mainCam.rect = rect;
        }
        else // 窄屏
        {
            float scaleHeight = currentAspect / targetAspect;
            Rect rect = mainCam.rect;
            rect.width = 1f;
            rect.height = scaleHeight;
            rect.x = 0f;
            rect.y = (1f - scaleHeight) / 2f;
            mainCam.rect = rect;
        }
    }
}