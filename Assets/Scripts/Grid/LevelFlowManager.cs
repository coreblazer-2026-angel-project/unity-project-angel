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
    SaveData _saveData;
    int _currentLevelNumber = -1;

    void Awake() {
        _lm = FindObjectOfType<LevelManager>();

        // 启动时从 JSON 存档恢复进度和章节通关 flag
        _saveData = SaveSystem.Load();
        if (_saveData != null) {
            currentChapterIndex = _saveData.currentChapterIndex;
            currentLevelIndex = _saveData.currentLevelIndex;
        }
    }

    void Start() {
        if (autoStart && chapters.Count > 0) {
            // 关卡选择场景传入的 pending 关卡优先于 JSON 当前进度。
            ApplyPendingLevelSelection();
            LoadCurrent();
        }
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
        LevelProgress.CompleteLevel(GetCurrentLevelNumber());
        currentLevelIndex++;

        if (currentChapterIndex < 0 || currentChapterIndex >= chapters.Count) return;
        var chapter = chapters[currentChapterIndex];

        if (currentLevelIndex >= chapter.levels.Count) {
            // 当前章节完成 —— 设置通关 flag、触发完成事件并保存
            Debug.Log($"LevelFlowManager: 章节 [{chapter.name}] 已完成！");
            SetChapterCompleted(currentChapterIndex, true);
            onChapterCompleted?.Invoke(currentChapterIndex, chapter.name);
            OnChapterCompleted?.Invoke(currentChapterIndex, chapter.name);

            if (autoAdvanceToNextChapter && currentChapterIndex + 1 < chapters.Count) {
                currentChapterIndex++;
                currentLevelIndex = 0;
                SaveProgress();
                LoadCurrent();
            }
            return;
        }

        SaveProgress();
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
        _currentLevelNumber = currentLevelIndex + 1;
        LevelProgress.SetPendingLevelSelection(currentChapterIndex, currentLevelIndex, _currentLevelNumber);
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

    // ================ 存档读取/写入接口 ================

    /// <summary>读取接口：获取当前内存中的 SaveData 副本（首次会自动 Load）</summary>
    public SaveData GetSaveData() {
        if (_saveData == null) _saveData = SaveSystem.Load();
        return _saveData;
    }

    /// <summary>读取接口：指定章节索引是否已通关</summary>
    public bool IsChapterCompleted(int chapterIndex) {
        if (_saveData == null) _saveData = SaveSystem.Load();
        if (_saveData.chapterCompleted == null) return false;
        if (chapterIndex < 0 || chapterIndex >= _saveData.chapterCompleted.Count) return false;
        return _saveData.chapterCompleted[chapterIndex];
    }

    /// <summary>读取接口：当前章节索引（来自存档/内存）</summary>
    public int GetCurrentChapterIndex() => currentChapterIndex;

    /// <summary>读取接口：当前章节内的关卡索引（来自存档/内存）</summary>
    public int GetCurrentLevelIndex() => currentLevelIndex;

    /// <summary>设置章节通关 flag 并立即写入存档</summary>
    public void SetChapterCompleted(int chapterIndex, bool completed = true) {
        if (chapterIndex < 0) return;
        if (_saveData == null) _saveData = SaveSystem.Load();
        if (_saveData.chapterCompleted == null) _saveData.chapterCompleted = new List<bool>();
        while (_saveData.chapterCompleted.Count <= chapterIndex)
            _saveData.chapterCompleted.Add(false);
        _saveData.chapterCompleted[chapterIndex] = completed;
        SaveProgress();
    }

    /// <summary>把当前 currentChapterIndex / currentLevelIndex 同步到存档并写盘</summary>
    public void SaveProgress() {
        SaveData latest = SaveSystem.Load();

        if (_saveData != null && _saveData.chapterCompleted != null)
            latest.chapterCompleted = _saveData.chapterCompleted;

        latest.currentChapterIndex = currentChapterIndex;
        latest.currentLevelIndex = currentLevelIndex;

        _saveData = latest;
        SaveSystem.Save(_saveData);
    }

    /// <summary>清除存档（重新开始游戏）</summary>
    [ContextMenu("Delete Save")]
    public void DeleteSave() {
        SaveSystem.Delete();
        _saveData = new SaveData();
        currentChapterIndex = 0;
        currentLevelIndex = 0;
    }

    void ApplyPendingLevelSelection() {
        if (!LevelProgress.TryConsumePendingLevelSelection(out int pendingChapter, out int pendingLevel, out int pendingLevelNumber))
            return;

        if (pendingChapter < 0 || pendingChapter >= chapters.Count)
            return;

        currentChapterIndex = pendingChapter;
        currentLevelIndex = Mathf.Clamp(pendingLevel, 0, Mathf.Max(0, chapters[pendingChapter].levels.Count - 1));
        _currentLevelNumber = Mathf.Max(1, pendingLevelNumber);
    }

    int GetCurrentLevelNumber() {
        if (_currentLevelNumber > 0)
            return _currentLevelNumber;

        return currentLevelIndex + 1;
    }
}
