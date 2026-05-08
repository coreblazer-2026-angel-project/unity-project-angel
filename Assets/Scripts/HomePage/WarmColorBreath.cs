using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WarmColorBreath : MonoBehaviour
{
    public Volume volume;              // 挂在你的 Global Volume
    public float minIntensity = 0.08f; // 最小 Alpha/强度
    public float maxIntensity = 0.12f; // 最大 Alpha/强度
    public float speed = 0.5f;         // 呼吸速度

    private ColorAdjustments colorAdjust;

    void Start()
    {
        if (volume.profile.TryGet<ColorAdjustments>(out var ca))
        {
            colorAdjust = ca;
        }
        else
        {
            Debug.LogError("Color Adjustments not found in Volume Profile!");
        }
    }

    void Update()
    {
        if (colorAdjust != null)
        {
            // PingPong 在 min/max 之间循环
            float t = Mathf.PingPong(Time.time * speed, 1f);
            colorAdjust.colorFilter.value = new Color(1f, 0.914f, 0.761f, Mathf.Lerp(minIntensity, maxIntensity, t));
        }
    }
}