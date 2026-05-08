using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 存档数据：当前进度（章节索引、关卡索引）+ 每个章节是否通关的 flag。
/// </summary>
[System.Serializable]
public class SaveData {
    [Tooltip("当前章节索引")]
    public int currentChapterIndex = 0;

    [Tooltip("当前章节内的关卡索引")]
    public int currentLevelIndex = 0;

    [Tooltip("已解锁到第几关。1 表示第一关已解锁但未通关。")]
    public int maxUnlockedLevel = 1;

    [Tooltip("希望值/已通关关卡数。0 表示第一关未收集。")]
    public int hope = 0;

    [Tooltip("是否有从关卡选择场景传入的待加载关卡。")]
    public bool hasPendingLevelSelection = false;

    public int pendingChapterIndex = 0;
    public int pendingLevelIndex = 0;
    public int pendingLevelNumber = 1;

    [Tooltip("每个章节是否通过的 flag，索引对应章节索引")]
    public List<bool> chapterCompleted = new();
}

/// <summary>
/// 存档系统：使用 JSON 持久化进度到游戏目录的 save.json。
/// 路径规则：Editor → 项目根目录；Build → 可执行文件同级目录。
/// 不依赖 PlayerPrefs / Application.persistentDataPath。
/// </summary>
public static class SaveSystem {

    const string SAVE_FILENAME = "save.json";

    /// <summary>存档文件绝对路径（游戏目录下）</summary>
    public static string SaveFilePath =>
        Path.GetFullPath(Path.Combine(Application.dataPath, "..", SAVE_FILENAME));

    /// <summary>从磁盘读取存档；不存在或解析失败时返回新建的空 SaveData</summary>
    public static SaveData Load() {
        try {
            if (!File.Exists(SaveFilePath)) {
                Debug.Log($"SaveSystem: 存档不存在，返回空数据 ({SaveFilePath})");
                return new SaveData();
            }
            string json = File.ReadAllText(SaveFilePath);
            var data = JsonUtility.FromJson<SaveData>(json);
            return data ?? new SaveData();
        } catch (System.Exception e) {
            Debug.LogError($"SaveSystem.Load 失败: {e.Message}");
            return new SaveData();
        }
    }

    /// <summary>把 SaveData 写到磁盘</summary>
    public static void Save(SaveData data) {
        if (data == null) return;
        try {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SaveFilePath, json);
            Debug.Log($"SaveSystem: 已保存 → {SaveFilePath}");
        } catch (System.Exception e) {
            Debug.LogError($"SaveSystem.Save 失败: {e.Message}");
        }
    }

    /// <summary>删除存档文件</summary>
    public static void Delete() {
        try {
            if (File.Exists(SaveFilePath)) {
                File.Delete(SaveFilePath);
                Debug.Log($"SaveSystem: 已删除存档 ({SaveFilePath})");
            }
        } catch (System.Exception e) {
            Debug.LogError($"SaveSystem.Delete 失败: {e.Message}");
        }
    }

    /// <summary>查询某章节是否已通关（独立工具函数，方便不持有 LevelFlowManager 的代码使用）</summary>
    public static bool IsChapterCompleted(int chapterIndex) {
        if (chapterIndex < 0) return false;
        var data = Load();
        if (data == null || data.chapterCompleted == null) return false;
        if (chapterIndex >= data.chapterCompleted.Count) return false;
        return data.chapterCompleted[chapterIndex];
    }
}

