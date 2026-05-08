using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// WalkingScene 右下角希望收集进度文字。
/// 支持原生 Text 或 TextMeshProUGUI。
/// </summary>
public class HopeProgressText : MonoBehaviour
{
    public string format = "{0}";
    public Image iconImage;
    public TMP_Text iconText;
    public string iconSymbol = "✦";
    public Color iconColor = new Color32(255, 218, 76, 255);
    public bool pulseIcon = true;
    public float pulseScale = 0.08f;
    public float pulseSpeed = 2.2f;

    Text _text;
    TMP_Text _tmpText;
    Vector3 _iconBaseScale = Vector3.one;
    int _lastHope = -1;

    void Awake()
    {
        _text = GetComponent<Text>();
        _tmpText = GetComponent<TMP_Text>();

        if (iconImage != null)
            iconImage.color = iconColor;

        if (iconText != null) {
            iconText.text = iconSymbol;
            iconText.color = iconColor;
            _iconBaseScale = iconText.rectTransform.localScale;
        }

        Refresh();
    }

    void Update()
    {
        if (_lastHope != LevelProgress.Hope)
            Refresh();

        if (pulseIcon && iconText != null) {
            float scale = 1f + Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseScale;
            iconText.rectTransform.localScale = _iconBaseScale * scale;
        }
    }

    void Refresh()
    {
        _lastHope = LevelProgress.Hope;
        string value = string.Format(format, _lastHope);

        if (_tmpText != null)
            _tmpText.text = value;

        if (_text != null)
            _text.text = value;
    }
}
