using UnityEngine;

public class TestStart : MonoBehaviour {
    [Header("测试开关")]
    [Tooltip("勾选后进入 Play 立即加载下方的测试关卡")]
    public bool autoLoadTestLevel;

    [Tooltip("测试用关卡；仅在 autoLoadTestLevel 勾选时自动加载")]
    public LevelData testLevel;

    [Tooltip("若为空，会先在本物体上找 LevelManager，再在场景里查找")]
    public LevelManager levelManager;

    LevelManager _lm;

    void Awake() {
        _lm = levelManager != null
            ? levelManager
            : GetComponent<LevelManager>();

        if (_lm == null)
            _lm = FindObjectOfType<LevelManager>();
    }

    void Start() {
        if (autoLoadTestLevel && testLevel != null)
            StartLevel(testLevel);
    }

    /// <summary>
    /// 由外部（如选关 UI 按钮）调用，传入选中的关卡数据。
    /// </summary>
    public void StartLevel(LevelData levelData) {
        if (levelData == null) {
            Debug.LogError("TestStart: LevelData 为空，无法加载关卡");
            return;
        }

        if (_lm == null) {
            Debug.LogError("TestStart: 未找到 LevelManager");
            return;
        }

        _lm.LoadLevel(levelData);
    }
}
