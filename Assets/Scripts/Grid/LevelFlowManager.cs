using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 关卡流程管理器：维护多个章节的关卡列表，监听胜利条件（所有 Light 被点亮），
/// 自动清理当前关卡并加载下一关。可以独立加载某一章。
/// </summary>
public class LevelFlowManager : MonoBehaviour {

    [System.Serializable]
    public class Chapter {
        [Tooltip("章节名称（用于 LoadChapter(string) 查找）")]
        public string name;
        [Tooltip("该章节的关卡列表，按顺序播放")]
        public List<LevelData> levels = new();
    }

    /// <summary>章节完成事件（带 chapterIndex 和 chapterName 参数）</summary>
    [System.Serializable]
    public class ChapterCompletedEvent : UnityEvent<int, string> {}

    [Header("章节列表")]
    public List<Chapter> chapters = new();

    [Header("当前进度")]
    [Tooltip("当前章节索引")]
    public int currentChapterIndex = 0;
    [Tooltip("当前章节内的关卡索引")]
    public int currentLevelIndex = 0;

    [Header("行为")]
    [Tooltip("胜利后延迟多少秒再加载下一关（让玩家看到通关效果）")]
    public float advanceDelay = 1.5f;

    [Tooltip("勾选后，Start 时自动加载 currentChapterIndex / currentLevelIndex 指向的关卡")]
    public bool autoStart = true;

    [Tooltip("通关一章的最后一关后是否自动进入下一章；不勾则停在该关卡")]
    public bool autoAdvanceToNextChapter = false;

    [Tooltip("胜利条件检测间隔（秒）")]
    public float checkInterval = 0.3f;

    [Header("章节完成回调")]
    [Tooltip("当章节最后一关通关后触发（参数：chapterIndex, chapterName）。Inspector 里可绑 UI 显示等。")]
    public ChapterCompletedEvent onChapterCompleted = new();

    /// <summary>章节完成事件（C# 代码端订阅用），参数：(chapterIndex, chapterName)</summary>
    public event System.Action<int, string> OnChapterCompleted;

    LevelManager _lm;
    bool _advancing;
    float _checkTimer;

    void Awake() {
        _lm = FindObjectOfType<LevelManager>();
    }

    void Start() {
        if (autoStart && chapters.Count > 0)
            LoadCurrent();
    }

    void Update() {
        if (_advancing || chapters.Count == 0) return;

        _checkTimer += Time.deltaTime;
        if (_checkTimer < checkInterval) return;
        _checkTimer = 0f;

        if (CheckWinCondition()) {
            _advancing = true;
            Invoke(nameof(Advance), advanceDelay);
        }
    }

    /// <summary>胜利条件：场上至少有一个 Light，且所有 Light 都被点亮（intensity >= workIntensity）</summary>
    bool CheckWinCondition() {
        var em = ElectricManager.Instance;
        if (em == null) return false;

        bool hasLight = false;
        foreach (var element in em.ElectricElements.Values) {
            if (element is Light light) {
                hasLight = true;
                if (light.intensity < light.workIntensity) return false;
            }
        }
        return hasLight;
    }

    /// <summary>推进到下一关（章节内）</summary>
    public void Advance() {
        _advancing = false;
        currentLevelIndex++;

        if (currentChapterIndex < 0 || currentChapterIndex >= chapters.Count) return;
        var chapter = chapters[currentChapterIndex];

        if (currentLevelIndex >= chapter.levels.Count) {
            // 当前章节完成 —— 触发完成事件
            Debug.Log($"LevelFlowManager: 章节 [{chapter.name}] 已完成！");
            onChapterCompleted?.Invoke(currentChapterIndex, chapter.name);
            OnChapterCompleted?.Invoke(currentChapterIndex, chapter.name);

            if (autoAdvanceToNextChapter && currentChapterIndex + 1 < chapters.Count) {
                currentChapterIndex++;
                currentLevelIndex = 0;
                LoadCurrent();
            }
            return;
        }

        LoadCurrent();
    }

    /// <summary>加载指定章节的指定关卡（外部入口）</summary>
    public void LoadChapter(int chapterIndex, int startLevel = 0) {
        if (chapterIndex < 0 || chapterIndex >= chapters.Count) {
            Debug.LogError($"LevelFlowManager: chapterIndex {chapterIndex} 越界（chapters.Count = {chapters.Count}）");
            return;
        }
        currentChapterIndex = chapterIndex;
        currentLevelIndex = Mathf.Clamp(startLevel, 0, Mathf.Max(0, chapters[chapterIndex].levels.Count - 1));
        LoadCurrent();
    }

    /// <summary>按名称加载章节（外部入口）</summary>
    public void LoadChapter(string chapterName, int startLevel = 0) {
        for (int i = 0; i < chapters.Count; i++) {
            if (chapters[i].name == chapterName) {
                LoadChapter(i, startLevel);
                return;
            }
        }
        Debug.LogError($"LevelFlowManager: 未找到章节 [{chapterName}]");
    }

    /// <summary>加载当前章节当前索引指向的关卡</summary>
    public void LoadCurrent() {
        if (_lm == null) _lm = FindObjectOfType<LevelManager>();
        if (_lm == null) {
            Debug.LogError("LevelFlowManager: 未找到 LevelManager");
            return;
        }
        if (currentChapterIndex < 0 || currentChapterIndex >= chapters.Count) {
            Debug.LogWarning($"LevelFlowManager: currentChapterIndex {currentChapterIndex} 越界");
            return;
        }
        var chapter = chapters[currentChapterIndex];
        if (currentLevelIndex < 0 || currentLevelIndex >= chapter.levels.Count) {
            Debug.LogWarning($"LevelFlowManager: 章节 [{chapter.name}] currentLevelIndex {currentLevelIndex} 越界");
            return;
        }
        _lm.LoadLevel(chapter.levels[currentLevelIndex]);

        // 当 currentLevelIndex 在 [1, 8] 范围（即第 2 ~ 第 9 关）时，调整主摄像头
        AdjustCameraForCurrentLevel();
    }

    /// <summary>关卡 2~9（索引 1~8）时把主相机移动到 (2, 1.75)，正交大小调到 5.5</summary>
    void AdjustCameraForCurrentLevel() {
        Camera cam = Camera.main;
        if (cam == null) return;

        if (currentLevelIndex >= 1 && currentLevelIndex <= 8) {
            Vector3 pos = cam.transform.position;
            pos.x = 2f;
            pos.y = -1.75f;
            cam.transform.position = pos;
            cam.orthographicSize = 5.5f;
        }
    }

    [ContextMenu("Manually Advance")]
    public void ManuallyAdvance() => Advance();

    /// <summary>
    /// 重启当前关卡（外部入口）：清空所有元件状态后重新加载当前 chapterIndex/levelIndex 指向的关卡。
    /// 可以从 UI 按钮 OnClick 直接绑定，或者代码 levelFlowManager.RestartCurrentLevel() 调用。
    /// </summary>
    [ContextMenu("Restart Current Level")]
    public void RestartCurrentLevel() {
        ElectricManager.Instance?.ClearAll();
        // 同时取消正在进行的胜利推进，防止重启后仍触发推进
        CancelInvoke(nameof(Advance));
        _advancing = false;
        _checkTimer = 0f;
        LoadCurrent();
    }

    /// <summary>RestartCurrentLevel 的简短别名，便于旧代码兼容</summary>
    public void Restart() => RestartCurrentLevel();

    [ContextMenu("Load Chapter 1")]
    public void LoadChapter1() => LoadChapter(0);

    [ContextMenu("Load Chapter 2")]
    public void LoadChapter2() => LoadChapter(1);
}
