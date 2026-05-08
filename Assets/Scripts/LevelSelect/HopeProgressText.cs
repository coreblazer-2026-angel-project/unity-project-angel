using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// WalkingScene 右下角希望收集进度文字。
/// 支持原生 Text 或 TextMeshProUGUI。
/// </summary>
public class HopeProgressText : MonoBehaviour
{
    [Header("显示")]
    public string format = "{0}";
    public bool showCompletedLevelCount = true;
    public Image iconImage;
    public TMP_Text iconText;
    public string iconSymbol = "✦";
    public Color iconColor = new Color32(255, 218, 76, 255);
    public bool pulseIcon = true;
    public float pulseScale = 0.08f;
    public float pulseSpeed = 2.2f;

    [Header("测试清档")]
    public bool resetUnlockProgressToo = true;

    Text _text;
    TMP_Text _tmpText;
    Vector3 _iconBaseScale = Vector3.one;
    int _lastValue = -1;

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
        if (_lastValue != GetDisplayValue())
            Refresh();

        if (pulseIcon && iconText != null) {
            float scale = 1f + Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseScale;
            iconText.rectTransform.localScale = _iconBaseScale * scale;
        }
    }

    void Refresh()
    {
        CacheTextComponents();
        _lastValue = GetDisplayValue();
        string value = string.Format(format, _lastValue);

        if (_tmpText != null)
            _tmpText.text = value;

        if (_text != null)
            _text.text = value;
    }

    [ContextMenu("Reset Hope For Test")]
    public void ResetHopeForTest()
    {
        if (resetUnlockProgressToo) {
            LevelProgress.ResetToFirstLevelIncomplete();
        } else {
            LevelProgress.ResetHope();
        }

        Refresh();
        Debug.Log($"[HopeProgressText] Hope progress reset. Hope={LevelProgress.Hope}, MaxUnlockedLevel={LevelProgress.MaxUnlockedLevel}", this);
    }

    [ContextMenu("Reset To First Level Incomplete")]
    public void ResetToFirstLevelIncomplete()
    {
        LevelProgress.ResetToFirstLevelIncomplete();
        Refresh();
        Debug.Log($"[HopeProgressText] Reset to first level incomplete. Hope={LevelProgress.Hope}, MaxUnlockedLevel={LevelProgress.MaxUnlockedLevel}", this);
    }

    [ContextMenu("Set Hope To 1 For Test")]
    public void SetHopeToOneForTest()
    {
        LevelProgress.SetCompletedLevelCount(1);
        Refresh();
        Debug.Log($"[HopeProgressText] Set Hope to 1. Hope={LevelProgress.Hope}, MaxUnlockedLevel={LevelProgress.MaxUnlockedLevel}", this);
    }

    void CacheTextComponents()
    {
        if (_text == null)
            _text = GetComponent<Text>();

        if (_tmpText == null)
            _tmpText = GetComponent<TMP_Text>();
    }

    int GetDisplayValue()
    {
        return showCompletedLevelCount
            ? LevelProgress.CompletedLevelCount
            : LevelProgress.Hope;
    }
}
