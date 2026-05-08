using UnityEngine;

/// <summary>
/// 关卡选择进度。先用 Unity 原生 PlayerPrefs 存最小进度：
/// 已解锁到第几关、已经收集多少希望。
/// </summary>
public static class LevelProgress
{
    const string MaxUnlockedKey = "LevelSelect.MaxUnlockedLevel";
    const string HopeKey = "LevelSelect.Hope";
    const string LegacyMaxUnlockedKey = "LevelProgress_MaxUnlockedLevel";
    const string LegacyHopeKey = "LevelProgress_Hope";

    public static int MaxUnlockedLevel
    {
        get => PlayerPrefs.GetInt(MaxUnlockedKey, 1);
        set
        {
            PlayerPrefs.SetInt(MaxUnlockedKey, Mathf.Max(1, value));
            PlayerPrefs.Save();
        }
    }

    public static int Hope
    {
        get => PlayerPrefs.GetInt(HopeKey, 0);
        set
        {
            PlayerPrefs.SetInt(HopeKey, Mathf.Max(0, value));
            PlayerPrefs.Save();
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
        if (!IsCompleted(levelNumber))
            Hope = Mathf.Max(Hope, levelNumber);

        if (levelNumber >= MaxUnlockedLevel)
            MaxUnlockedLevel = levelNumber + 1;
    }

    public static void ResetProgress()
    {
        ResetToFirstLevelIncomplete();
    }

    public static void ResetToFirstLevelIncomplete()
    {
        PlayerPrefs.SetInt(MaxUnlockedKey, 1);
        PlayerPrefs.SetInt(HopeKey, 0);
        PlayerPrefs.DeleteKey(LegacyMaxUnlockedKey);
        PlayerPrefs.DeleteKey(LegacyHopeKey);
        PlayerPrefs.Save();
    }

    public static void ResetHope()
    {
        PlayerPrefs.SetInt(HopeKey, 0);
        PlayerPrefs.DeleteKey(LegacyHopeKey);
        PlayerPrefs.Save();
    }

    public static void SetCompletedLevelCount(int completedCount)
    {
        completedCount = Mathf.Max(0, completedCount);
        Hope = completedCount;
        MaxUnlockedLevel = Mathf.Max(1, completedCount + 1);
    }
}
