using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 挂在 WalkingScene 的关卡 NPC 上：
/// 玩家进入触发范围时显示头顶文字，按 E 进入对应关卡。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LevelSelectNpc : MonoBehaviour
{
    [Header("关卡信息")]
    public int levelNumber = 1;
    public int chapterIndex = 0;
    public int levelIndex = 0;
    public string npcName = "摔掉糖果的小男孩";

    [Header("进入关卡")]
    public string levelSceneName = "Levels";

    [Header("交互")]
    public Transform player;
    public float interactDistance = 1.5f;

    [Header("显示")]
    public SpriteRenderer bodyRenderer;
    public GameObject promptRoot;
    public TextMesh titleText;
    public TextMesh completedText;
    public TextMesh lockedText;

    bool _playerNear;

    void Reset()
    {
        bodyRenderer = GetComponent<SpriteRenderer>();
        var trigger = GetComponent<Collider2D>();
        trigger.isTrigger = true;
    }

    void Awake()
    {
        var trigger = GetComponent<Collider2D>();
        trigger.isTrigger = true;

        if (bodyRenderer == null)
            bodyRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (player == null) {
            var foundPlayer = FindObjectOfType<SideScrollPlayer>();
            if (foundPlayer != null)
                player = foundPlayer.transform;
        }

        RefreshVisual();
        HidePrompt();
    }

    void Update()
    {
        UpdatePlayerNear();

        if (!_playerNear) {
            HidePrompt();
            return;
        }

        ShowPrompt();

        if (Input.GetKeyDown(KeyCode.E))
            TryEnterLevel();
    }

    void UpdatePlayerNear()
    {
        if (player == null) {
            _playerNear = false;
            return;
        }

        _playerNear = Vector2.Distance(transform.position, player.position) <= interactDistance;
    }

    void RefreshVisual()
    {
        if (bodyRenderer == null) return;

        bool unlocked = LevelProgress.IsUnlocked(levelNumber);
        bodyRenderer.color = unlocked ? Color.white : Color.black;
    }

    void ShowPrompt()
    {
        bool unlocked = LevelProgress.IsUnlocked(levelNumber);
        bool completed = LevelProgress.IsCompleted(levelNumber);

        if (promptRoot != null)
            promptRoot.SetActive(true);

        if (titleText != null)
            titleText.text = $"{levelNumber} -- {npcName}";

        if (completedText != null)
        {
            completedText.gameObject.SetActive(completed);
            completedText.text = "希望已收集";
        }

        if (lockedText != null)
        {
            lockedText.gameObject.SetActive(!unlocked);
            lockedText.text = "请先完成前置关卡";
        }
    }

    void HidePrompt()
    {
        if (promptRoot != null)
            promptRoot.SetActive(false);
    }

    void TryEnterLevel()
    {
        if (!LevelProgress.IsUnlocked(levelNumber))
        {
            ShowPrompt();
            return;
        }

        PlayerPrefs.SetInt("LevelSelect.PendingChapterIndex", chapterIndex);
        PlayerPrefs.SetInt("LevelSelect.PendingLevelIndex", levelIndex);
        PlayerPrefs.SetInt("LevelSelect.PendingLevelNumber", levelNumber);
        PlayerPrefs.Save();

        SceneManager.LoadScene(levelSceneName);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
#endif
}
