using TMPro;
using UnityEngine;

/// <summary>
/// 在屏幕右上角显示当前关卡进度（章节 - 关卡），格式可自定义。
/// 挂在 Canvas 下的 TextMeshProUGUI 组件上即可，自动从 LevelFlowManager 取当前进度。
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class LevelProgressUI : MonoBehaviour {

    [Tooltip("关卡流程管理器；不指定时自动 FindObjectOfType")]
    public LevelFlowManager flow;

    [Tooltip("文本格式：{0} = 章节序号（从1计），{1} = 关卡序号（从1计）。默认 '1-3' 风格")]
    public string format = "{0}-{1}";

    TMP_Text _text;
    int _lastChapter = -1;
    int _lastLevel = -1;

    void Awake() {
        _text = GetComponent<TMP_Text>();
        if (flow == null) flow = FindObjectOfType<LevelFlowManager>();
    }

    void Update() {
        if (flow == null || _text == null) return;

        int chapter = flow.GetCurrentChapterIndex();
        int level   = flow.GetCurrentLevelIndex();

        // 仅在变化时刷新，避免每帧 SetText 触发 Canvas 重建
        if (chapter == _lastChapter && level == _lastLevel) return;
        _lastChapter = chapter;
        _lastLevel = level;

        _text.text = string.Format(format, chapter + 1, level + 1);
    }

    /// <summary>外部强制刷新（如改了 format 后立即生效）</summary>
    public void Refresh() {
        _lastChapter = -1;
        _lastLevel = -1;
    }
}

