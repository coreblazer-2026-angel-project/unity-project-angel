using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 关卡流程管理器：维护关卡列表，监听胜利条件（所有 Light 被点亮），
/// 自动清理当前关卡并加载下一关。
/// </summary>
public class LevelFlowManager : MonoBehaviour {

    [Header("关卡列表（按顺序播放）")]
    public List<LevelData> levels = new();

    [Header("当前关卡索引")]
    public int currentIndex = 0;

    [Tooltip("胜利后延迟多少秒再加载下一关（让玩家看到通关效果）")]
    public float advanceDelay = 1.5f;

    [Tooltip("勾选后，Awake/Start 时会自动加载第一关")]
    public bool autoStart = true;

    [Tooltip("胜利条件检测间隔（秒）")]
    public float checkInterval = 0.3f;

    LevelManager _lm;
    bool _advancing;
    float _checkTimer;

    void Awake() {
        _lm = FindObjectOfType<LevelManager>();
    }

    void Start() {
        if (autoStart && levels.Count > 0)
            LoadCurrent();
    }

    void Update() {
        if (_advancing || levels.Count == 0) return;

        _checkTimer += Time.deltaTime;
        if (_checkTimer < checkInterval) return;
        _checkTimer = 0f;

        if (CheckWinCondition()) {
            _advancing = true;
            Invoke(nameof(Advance), advanceDelay);
        }
    }

    /// <summary>胜利条件：场上至少有一个 Light，且所有 Light 都被点亮（邻居 Wire intensity > 0）</summary>
    bool CheckWinCondition() {
        var em = ElectricManager.Instance;
        if (em == null) return false;

        bool hasLight = false;
        foreach (var element in em.ElectricElements.Values) {
            if (element is Light light) {
                hasLight = true;
                bool lit = false;
                foreach (var neighbor in light.neighborElements) {
                    if (neighbor is Wire && neighbor.intensity > 0) {
                        lit = true;
                        break;
                    }
                }
                if (!lit) return false;
            }
        }
        return hasLight;
    }

    /// <summary>推进到下一关</summary>
    public void Advance() {
        _advancing = false;
        currentIndex++;

        if (currentIndex >= levels.Count) {
            Debug.Log("LevelFlowManager: 所有关卡已完成！");
            return;
        }

        // LevelManager.LoadLevel 会自己调用 ElectricManager.ClearAll()，无需在此处重复清理
        LoadCurrent();
    }

    /// <summary>清理当前关卡（保留方法用于外部手动调用）</summary>
    void ClearLevel() {
        ElectricManager.Instance?.ClearAll();
    }

    /// <summary>加载当前索引指向的关卡</summary>
    public void LoadCurrent() {
        if (_lm == null) _lm = FindObjectOfType<LevelManager>();
        if (_lm == null) {
            Debug.LogError("LevelFlowManager: 未找到 LevelManager");
            return;
        }
        if (currentIndex < 0 || currentIndex >= levels.Count) {
            Debug.LogWarning($"LevelFlowManager: currentIndex {currentIndex} 越界");
            return;
        }
        _lm.LoadLevel(levels[currentIndex]);
    }

    [ContextMenu("Manually Advance")]
    public void ManuallyAdvance() => Advance();

    [ContextMenu("Restart Current Level")]
    public void Restart() {
        ClearLevel();
        LoadCurrent();
    }
}
