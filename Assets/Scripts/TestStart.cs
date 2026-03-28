using UnityEngine;

public class TestStart : MonoBehaviour {
    [Tooltip("要加载的关卡资源；必须在 Inspector 里拖入 LevelData")]
    public LevelData level;

    [Tooltip("若为空，会先在本物体上找 LevelManager，再在场景里查找")]
    public LevelManager levelManager;

    void Start() {
        if (level == null) {
            Debug.LogError("TestStart: 请在 Inspector 中为 LevelData 赋值。");
            return;
        }

        LevelManager lm = levelManager != null
            ? levelManager
            : GetComponent<LevelManager>();

        if (lm == null)
            lm = FindObjectOfType<LevelManager>();

        if (lm == null) {
            Debug.LogError("TestStart: 未找到 LevelManager。请把 LevelManager 挂在同物体上，或拖到 levelManager 字段。");
            return;
        }

        lm.LoadLevel(level);
    }
}
