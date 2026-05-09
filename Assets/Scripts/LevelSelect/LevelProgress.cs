using UnityEngine;

/// <summary>
/// 关卡选择进度接口。实际存档统一走 SaveSystem/save.json。
/// </summary>
public static class LevelProgress
{
    public static int MaxUnlockedLevel
    {
        get => Mathf.Max(1, SaveSystem.Load().maxUnlockedLevel);
        set
        {
            SaveData data = SaveSystem.Load();
            data.maxUnlockedLevel = Mathf.Max(1, value);
            SaveSystem.Save(data);
        }
    }

    public static int Hope
    {
        get => Mathf.Max(0, SaveSystem.Load().hope);
        set
        {
            SaveData data = SaveSystem.Load();
            data.hope = Mathf.Max(0, value);
            SaveSystem.Save(data);
        }
    }

    public static int CompletedLevelCount
    {
        get => Hope;
    }

    public static bool IsUnlocked(int levelNumber)
    {
        return levelNumber <= MaxUnlockedLevel;
    }

    public static bool IsCompleted(int levelNumber)
    {
        return levelNumber <= Hope;
    }

    public static void CompleteLevel(int levelNumber)
    {
        SaveData data = SaveSystem.Load();
        int completedLevel = Mathf.Max(1, levelNumber);

        data.hope = Mathf.Max(data.hope, completedLevel);
        data.maxUnlockedLevel = Mathf.Max(1, data.maxUnlockedLevel, completedLevel + 1);
        SaveSystem.Save(data);
    }

    public static void ResetProgress()
    {
        ResetToFirstLevelIncomplete();
    }

    public static void ResetToFirstLevelIncomplete()
    {
        SaveData data = SaveSystem.Load();
        data.maxUnlockedLevel = 1;
        data.hope = 0;
        data.hasPendingLevelSelection = false;
        data.pendingChapterIndex = 0;
        data.pendingLevelIndex = 0;
        data.pendingLevelNumber = 1;
        SaveSystem.Save(data);
    }

    public static void ResetHope()
    {
        SaveData data = SaveSystem.Load();
        data.hope = 0;
        SaveSystem.Save(data);
    }

    public static void SetCompletedLevelCount(int completedCount)
    {
        completedCount = Mathf.Max(0, completedCount);
        Hope = completedCount;
        MaxUnlockedLevel = Mathf.Max(1, completedCount + 1);
    }

    public static void SetPendingLevelSelection(int chapterIndex, int levelIndex, int levelNumber)
    {
        SaveData data = SaveSystem.Load();
        data.hasPendingLevelSelection = true;
        data.pendingChapterIndex = Mathf.Max(0, chapterIndex);
        data.pendingLevelIndex = Mathf.Max(0, levelIndex);
        data.pendingLevelNumber = Mathf.Max(1, levelNumber);
        data.pendingChapterName = "";
        data.pendingLevelName = "";
        SaveSystem.Save(data);
    }

    public static void SetPendingLevelByName(string chapterName, string levelName, int levelNumber = 1)
    {
        SaveData data = SaveSystem.Load();
        data.hasPendingLevelSelection = true;
        data.pendingChapterName = chapterName ?? "";
        data.pendingLevelName = levelName ?? "";
        data.pendingLevelNumber = Mathf.Max(1, levelNumber);
        data.pendingChapterIndex = 0;
        data.pendingLevelIndex = 0;
        SaveSystem.Save(data);
    }

    public static bool TryConsumePendingLevelSelection(out int chapterIndex, out int levelIndex, out int levelNumber)
    {
        SaveData data = SaveSystem.Load();
        chapterIndex = data.pendingChapterIndex;
        levelIndex = data.pendingLevelIndex;
        levelNumber = Mathf.Max(1, data.pendingLevelNumber);

        if (!data.hasPendingLevelSelection)
            return false;

        data.hasPendingLevelSelection = false;
        SaveSystem.Save(data);
        return true;
    }
}
