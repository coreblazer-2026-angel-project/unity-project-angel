using UnityEngine;

/// <summary>
/// 关卡选择进度。先用 Unity 原生 PlayerPrefs 存最小进度：
/// 已解锁到第几关、已经收集多少希望。
/// </summary>
public static class LevelProgress
{
    const string MaxUnlockedKey = "LevelSelect.MaxUnlockedLevel";
    const string HopeKey = "LevelSelect.Hope";

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

    public static bool IsUnlocked(int levelNumber)
    {
        return levelNumber <= MaxUnlockedLevel;
    }

    public static bool IsCompleted(int levelNumber)
    {
        return levelNumber < MaxUnlockedLevel;
    }

    public static void CompleteLevel(int levelNumber)
    {
        if (!IsCompleted(levelNumber))
            Hope += 1;

        if (levelNumber >= MaxUnlockedLevel)
            MaxUnlockedLevel = levelNumber + 1;
    }

    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(MaxUnlockedKey);
        PlayerPrefs.DeleteKey(HopeKey);
        PlayerPrefs.Save();
    }
}
