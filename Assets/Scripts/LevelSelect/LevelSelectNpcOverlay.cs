using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Game.Story;

public class LevelSelectNpcOverlay : MonoBehaviour
{
    [Header("关卡信息")]
    public int levelNumber = 1;
    public int chapterIndex = 0;
    public int levelIndex = 0;
    public string npcName = "摔掉糖果的小男孩";

    [Header("按名称匹配 LevelFlowManager")]
    [Tooltip("LevelFlowManager 中 Chapter.name，留空则用 chapterIndex")]
    public string chapterName = "";
    [Tooltip("LevelFlowManager 中 LevelData 的资产名（ScriptableObject.name），留空则用 levelIndex")]
    public string levelName = "";

    [Header("进入关卡")]
    public string levelSceneName = "Levels";
    public AudioClip interactSound;
    public AudioSource audioSource;
    [Tooltip("按 F 后音效播放多久再切换场景（秒）。设 0 则立即切换。")]
    public float transitionDelay = 0.4f;

    [Header("剧情")]
    [Tooltip("对应 Ink 剧情的 Resources 路径，如 Chapter1_LittleBoy（不含 .json）。留空则跳过剧情直接进入。")]
    public string storyFile = "";
    [Tooltip("通关后返回 WalkingScene 自动触发的对话。留空则不触发。")]
    public string completionStoryFile = "";
    [Tooltip("剧情播完后是否自动进入关卡。false 则需要再按一次 F。")]
    public bool autoEnterAfterStory = false;

    [Header("交互")]
    public Transform player;
    [Tooltip("玩家和 NPC 的 X 距离小于这个值时，算靠近。")]
    public float interactHorizontalRange = 1.6f;
    [Tooltip("玩家和 NPC 的 Y 距离小于这个值时，算靠近。")]
    public float interactVerticalRange = 2.4f;
    [Tooltip("交互中心相对 NPC 根节点的偏移。")]
    public Vector2 interactCenterOffset = Vector2.zero;
    public Vector3 worldOffset = new Vector3(0f, 2.15f, 0f);

    [Header("NPC 外观")]
    public SpriteRenderer bodyRenderer;
    public bool affectChildRenderers = true;
    [Tooltip("解锁后才显示的脚下影子或装饰。")]
    public GameObject[] showOnlyWhenUnlocked;
    public Color lockedSilhouetteColor = new Color32(22, 29, 42, 225);
    public Color completedTintColor = new Color32(255, 255, 255, 255);

    [Header("屏幕 UI")]
    public Canvas overlayCanvas;
    public RectTransform promptRoot;
    public bool clonePromptForThisNpc = true;
    public bool hidePromptTemplateAfterClone = true;
    public CanvasGroup promptGroup;
    public TMP_Text markerText;
    public TMP_Text leftArrowText;
    public TMP_Text titleText;
    public TMP_Text rightArrowText;
    public TMP_Text statusText;

    [Header("显示文案")]
    public bool showLevelNumber = false;
    public string titleFormat = "{0}  {1}";
    public bool titleAlwaysVisible = true;
    public KeyCode interactKey = KeyCode.F;
    public string availableStatus = "按 F 进入";
    public string completedStatus = "希望已收集";
    public string lockedStatus = "请先完成前置关卡";

    [Header("状态颜色")]
    public Color availableStatusColor = new Color32(255, 230, 85, 255);
    public Color completedStatusColor = new Color32(105, 255, 135, 255);
    public Color lockedStatusColor = new Color32(255, 98, 86, 255);

    [Header("提示动画")]
    public bool useFade = false;
    public float fadeSpeed = 12f;
    public float followSmooth = 18f;
    public Vector2 screenOffset = Vector2.zero;

    [Header("自动排版")]
    public bool autoArrangePrompt = true;
    public float markerY = 34f;
    public float titleY = 0f;
    public float arrowDistance = 86f;
    public float statusY = -30f;

    [Header("自动字体样式")]
    public bool autoApplyTextStyle = true;
    public float titleFontSize = 28f;
    public float arrowFontSize = 32f;
    public float markerFontSize = 30f;
    public float statusFontSize = 17f;
    public Color mainTextColor = new Color32(255, 246, 180, 255);
    public Color markerColor = new Color32(255, 203, 40, 255);
    public Color outlineColor = new Color32(28, 45, 64, 255);
    [Range(0f, 1f)]
    public float outlineWidth = 0.24f;

    // 缓存
    Camera _mainCamera;
    RectTransform _canvasRect;
    SpriteRenderer[] _visualRenderers;
    Color[] _originalRendererColors;

    // 状态机：0=hidden, 1=titleOnly, 2=fullPrompt, 3=enterPrompt
    int _promptState = -1;
    bool _playerNear;
    bool _lastUnlocked;
    bool _lastCompleted;
    bool _transitioning;
    bool _storyPlayed;
    bool _completionDialogMode;

    // 状态缓存（避免重复设置同一个值）
    string _lastStatusText;
    Color _lastStatusColor;

    void Awake()
    {
        _mainCamera = Camera.main;

        if (bodyRenderer == null)
            bodyRenderer = GetComponent<SpriteRenderer>();

        CacheVisualRenderers();

        if (promptRoot != null && clonePromptForThisNpc)
            ClonePromptForThisNpc();

        if (overlayCanvas == null && promptRoot != null)
            overlayCanvas = promptRoot.GetComponentInParent<Canvas>();

        _canvasRect = overlayCanvas?.transform as RectTransform;
    }

    void Start()
    {
        ResolvePlayer();
        RefreshVisual();
        // 初始状态：设为 -1 强制首次 Apply
        _promptState = -1;
        ApplyState(titleAlwaysVisible ? 1 : 0);

        // 检测从关卡场景返回：匹配 chapterName 时自动触发完成对话
        CheckChapterReturn();
    }

    void OnEnable()
    {
        var sm = StoryManager.Instance;
        if (sm != null) sm.OnStoryEnded += OnStoryEnded;
    }

    void OnDisable()
    {
        var sm = StoryManager.Instance;
        if (sm != null) sm.OnStoryEnded -= OnStoryEnded;
    }

    void Update()
    {
        if (_mainCamera == null) _mainCamera = Camera.main;

        ResolvePlayerOnce();
        CheckProgressChanged();

        // 兜底：_transitioning 中检测故事已结束但事件没触发
        if (_transitioning && !string.IsNullOrEmpty(storyFile))
        {
            var sm = StoryManager.Instance;
            if (sm == null || !sm.IsPlaying)
            {
                Debug.Log("[NPC] 兜底检测：故事已结束，手动触发 OnStoryEnded");
                OnStoryEnded();
                return;
            }
        }

        bool storyPlaying = StoryManager.Instance != null && StoryManager.Instance.IsPlaying;
        bool near = !storyPlaying && IsPlayerNear();

        if (near != _playerNear)
            _playerNear = near;

        // 计算目标状态
        int target;
        if (storyPlaying)
            target = 0; // 剧情播放时隐藏 prompt
        else if (!_playerNear)
            target = titleAlwaysVisible ? 1 : 0;
        else if (_storyPlayed || string.IsNullOrEmpty(storyFile))
            target = 3; // 剧情已播或无剧情，显示"按 F 进入"
        else
            target = 2; // 显示"按 F 对话"

        ApplyState(target);

        // prompt 可见时持续跟随
        if (promptRoot != null && promptRoot.gameObject.activeSelf)
        {
            FollowNpcOnScreen();
            UpdateFade();
        }

        if (_playerNear && !_transitioning && Input.GetKeyDown(interactKey))
            TryEnterLevel();
    }

    #region State Machine

    void ApplyState(int target)
    {
        if (target == _promptState) return;
        _promptState = target;

        switch (target)
        {
            case 0: SetHidden(); break;
            case 1: SetTitleOnly(); break;
            case 2: SetFullPrompt(false); break;
            case 3: SetFullPrompt(true); break;
        }
    }

    void SetHidden()
    {
        if (promptRoot) promptRoot.gameObject.SetActive(false);
    }

    void SetTitleOnly()
    {
        if (promptRoot) promptRoot.gameObject.SetActive(true);
        if (promptGroup) promptGroup.alpha = 1f;

        SetActive(markerText, false);
        SetActive(leftArrowText, false);
        SetActive(rightArrowText, false);
        SetActive(statusText, false);

        SetText(titleText, GetTitleText());
        ApplyPromptLayout();
        ApplyTextStyle();
    }

    void SetFullPrompt(bool showEnter)
    {
        if (promptRoot) promptRoot.gameObject.SetActive(true);

        SetActive(markerText, true);
        SetText(markerText, "▼");
        SetActive(leftArrowText, true);
        SetText(leftArrowText, ">");
        SetActive(rightArrowText, true);
        SetText(rightArrowText, "<");
        SetText(titleText, GetTitleText());

        if (statusText != null)
        {
            statusText.gameObject.SetActive(true);
            if (showEnter)
                SetStatus(availableStatus, availableStatusColor);
            else if (!_lastUnlocked)
                SetStatus(lockedStatus, lockedStatusColor);
            else if (_lastCompleted)
                SetStatus(completedStatus, completedStatusColor);
            else
                SetStatus("按 F 对话", availableStatusColor);
        }

        ApplyPromptLayout();
        ApplyTextStyle();
    }

    void SetStatus(string text, Color color)
    {
        if (statusText == null) return;
        if (_lastStatusText == text && _lastStatusColor == color) return;
        _lastStatusText = text;
        _lastStatusColor = color;
        statusText.text = text;
        statusText.color = color;
    }

    static void SetActive(TMP_Text t, bool on) { if (t != null) t.gameObject.SetActive(on); }
    static void SetText(TMP_Text t, string v) { if (t != null) t.text = v; }

    string GetTitleText()
    {
        if (!showLevelNumber) return npcName;
        string fmt = string.IsNullOrWhiteSpace(titleFormat) || titleFormat == "{1}" ? "{0}  {1}" : titleFormat;
        return string.Format(fmt, levelNumber, npcName);
    }

    #endregion

    #region Player Detection

    void ResolvePlayer()
    {
        if (player != null && player != transform) return;

        var go = GameObject.FindGameObjectWithTag("Player");
        if (go != null && go.transform != transform)
        {
            player = go.transform;
            return;
        }

        var found = FindObjectOfType<SideScrollPlayer>();
        if (found != null && found.transform != transform)
            player = found.transform;
    }

    void ResolvePlayerOnce()
    {
        if (player != null) return;
        ResolvePlayer();
    }

    bool IsPlayerNear()
    {
        if (player == null) return false;
        Vector2 center = (Vector2)transform.position + interactCenterOffset;
        Vector2 p = player.position;
        return Mathf.Abs(p.x - center.x) <= interactHorizontalRange
            && Mathf.Abs(p.y - center.y) <= interactVerticalRange;
    }

    #endregion

    #region Visual

    void CacheVisualRenderers()
    {
        _visualRenderers = affectChildRenderers
            ? GetComponentsInChildren<SpriteRenderer>(true)
            : (bodyRenderer != null ? new[] { bodyRenderer } : System.Array.Empty<SpriteRenderer>());

        _originalRendererColors = new Color[_visualRenderers.Length];
        for (int i = 0; i < _visualRenderers.Length; i++)
            _originalRendererColors[i] = _visualRenderers[i] != null ? _visualRenderers[i].color : Color.white;
    }

    void CheckProgressChanged()
    {
        bool unlocked = LevelProgress.IsUnlocked(levelNumber);
        bool completed = LevelProgress.IsCompleted(levelNumber);

        if (unlocked != _lastUnlocked || completed != _lastCompleted)
        {
            _lastUnlocked = unlocked;
            _lastCompleted = completed;
            RefreshVisual();

            // 进度变了，强制刷新当前状态
            int current = _promptState;
            _promptState = -1;
            ApplyState(current);
        }
    }

    void RefreshVisual()
    {
        for (int i = 0; i < _visualRenderers.Length; i++)
        {
            if (_visualRenderers[i] == null) continue;
            Color baseColor = i < _originalRendererColors.Length ? _originalRendererColors[i] : Color.white;
            _visualRenderers[i].color = _lastUnlocked
                ? (_lastCompleted ? MultiplyColor(baseColor, completedTintColor) : baseColor)
                : lockedSilhouetteColor;
        }

        if (showOnlyWhenUnlocked != null)
        {
            for (int i = 0; i < showOnlyWhenUnlocked.Length; i++)
                if (showOnlyWhenUnlocked[i] != null)
                    showOnlyWhenUnlocked[i].SetActive(_lastUnlocked);
        }
    }

    static Color MultiplyColor(Color a, Color b) =>
        new Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);

    #endregion

    #region Screen Position

    void FollowNpcOnScreen()
    {
        if (_mainCamera == null || promptRoot == null) return;

        Vector3 screenPos = _mainCamera.WorldToScreenPoint(transform.position + worldOffset);

        if (_canvasRect != null && overlayCanvas.renderMode != RenderMode.WorldSpace)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRect, screenPos,
                overlayCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _mainCamera,
                out Vector2 target);
            target += screenOffset;
            promptRoot.anchoredPosition = target;
        }
        else
        {
            promptRoot.position = screenPos;
        }
    }

    #endregion

    #region Layout & Style

    void ApplyPromptLayout()
    {
        if (!autoArrangePrompt) return;
        SetPos(markerText, 0f, markerY);
        SetPos(titleText, 0f, titleY);
        SetPos(leftArrowText, -arrowDistance, titleY);
        SetPos(rightArrowText, arrowDistance, titleY);
        SetPos(statusText, 0f, statusY);
    }

    static void SetPos(TMP_Text text, float x, float y)
    {
        if (text == null) return;
        RectTransform r = text.rectTransform;
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = new Vector2(x, y);
        text.alignment = TextAlignmentOptions.Center;
    }

    void ApplyTextStyle()
    {
        if (!autoApplyTextStyle) return;
        ApplyFont(markerText, markerFontSize, markerColor);
        ApplyFont(leftArrowText, arrowFontSize, mainTextColor);
        ApplyFont(titleText, titleFontSize, mainTextColor);
        ApplyFont(rightArrowText, arrowFontSize, mainTextColor);
        if (statusText != null) ApplyFont(statusText, statusFontSize, statusText.color);
    }

    void ApplyFont(TMP_Text text, float size, Color color)
    {
        if (text == null) return;
        text.fontSize = size;
        text.color = color;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;
        var mat = text.fontMaterial;
        mat.SetColor(ShaderUtilities.ID_FaceColor, color);
        mat.SetColor(ShaderUtilities.ID_OutlineColor, outlineColor);
        mat.SetFloat(ShaderUtilities.ID_OutlineWidth, outlineWidth);
    }

    #endregion

    #region Fade

    void UpdateFade()
    {
        if (!useFade)
        {
            if (promptGroup) promptGroup.alpha = 1f;
            return;
        }

        if (promptGroup == null) return;

        promptGroup.alpha = Mathf.Lerp(promptGroup.alpha, 1f, 1f - Mathf.Exp(-fadeSpeed * Time.deltaTime));

        bool vis = promptGroup.alpha > 0.01f;
        if (promptRoot && promptRoot.gameObject.activeSelf != vis)
            promptRoot.gameObject.SetActive(vis);
    }

    #endregion

    #region Clone

    void ClonePromptForThisNpc()
    {
        RectTransform src = promptRoot;
        RectTransform clone = Instantiate(src, src.parent);
        clone.name = $"{src.name}_{levelNumber}_{npcName}";
        clone.SetSiblingIndex(src.GetSiblingIndex() + 1);

        promptRoot = clone;
        promptGroup = clone.GetComponent<CanvasGroup>();
        if (promptGroup == null) promptGroup = clone.gameObject.AddComponent<CanvasGroup>();

        markerText = FindText(clone, "MarkerText");
        leftArrowText = FindText(clone, "LeftArrowText");
        titleText = FindText(clone, "TitleText");
        rightArrowText = FindText(clone, "RightArrowText");
        statusText = FindText(clone, "StatusText");

        clone.gameObject.SetActive(false);
        if (hidePromptTemplateAfterClone) src.gameObject.SetActive(false);
    }

    static TMP_Text FindText(RectTransform root, string name)
    {
        var texts = root.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
            if (texts[i].name == name) return texts[i];
        return null;
    }

    #endregion

    #region Story & Level Entry

    [Header("Dialog 面板")]
    public GameObject dialogPanel;

    void TryEnterLevel()
    {
        if (_transitioning) return;
        if (!_lastUnlocked) return;

        _transitioning = true;
        PlaySound();

        // 无剧情文件 → 直接进关卡
        if (string.IsNullOrEmpty(storyFile))
        {
            GoToLevel();
            return;
        }

        if (dialogPanel != null)
            dialogPanel.SetActive(true);

        var sm = StoryManager.Instance;
        if (sm != null)
        {
            var inkJson = StoryLoader.Instance.LoadInkJson(storyFile);
            if (inkJson == null)
            {
                Debug.LogError($"[NPC] StoryLoader 找不到文件: '{storyFile}'，跳过剧情直接进入关卡");
                _transitioning = false;
                GoToLevel();
                return;
            }
            Debug.Log($"[NPC] 加载剧情: '{inkJson.name}', 内容前50字: {inkJson.text.Substring(0, Mathf.Min(50, inkJson.text.Length))}");
            sm.PlayStory(inkJson);
        }
        else
        {
            Debug.LogWarning("[NPC] StoryManager 不存在，跳过剧情直接进入关卡");
            _transitioning = false;
            GoToLevel();
        }
    }

    void OnStoryEnded()
    {
        if (!_transitioning && !_completionDialogMode) return;

        if (dialogPanel != null)
            dialogPanel.SetActive(false);

        // 完成对话模式：播完就结束，不进关卡
        if (_completionDialogMode)
        {
            _completionDialogMode = false;
            _storyPlayed = true;
            _transitioning = false;
            return;
        }

        _storyPlayed = true;
        _transitioning = false;
        GoToLevel();
    }

    void CheckChapterReturn()
    {
        var data = SaveSystem.Load();
        if (data == null || !data.hasChapterReturn) return;

        // 匹配当前 NPC 的 chapterName
        if (!string.IsNullOrEmpty(data.returnChapterName)
            && !string.IsNullOrEmpty(chapterName)
            && data.returnChapterName != chapterName) return;

        // 消费标记
        data.hasChapterReturn = false;
        SaveSystem.Save(data);

        if (string.IsNullOrEmpty(completionStoryFile)) return;

        // 延迟一帧触发，等 StoryManager 准备好
        _completionDialogMode = true;
        _transitioning = true;
        StartCoroutine(PlayCompletionStoryDelayed());
    }

    IEnumerator PlayCompletionStoryDelayed()
    {
        yield return null;

        if (dialogPanel != null)
            dialogPanel.SetActive(true);

        var sm = StoryManager.Instance;
        if (sm != null)
        {
            var inkJson = StoryLoader.Instance.LoadInkJson(completionStoryFile);
            if (inkJson != null)
            {
                sm.PlayStory(inkJson);
                yield break;
            }
        }

        // 兜底：找不到剧情就恢复正常
        _completionDialogMode = false;
        _transitioning = false;
        if (dialogPanel != null)
            dialogPanel.SetActive(false);
    }

    void GoToLevel()
    {
        if (!string.IsNullOrEmpty(chapterName) || !string.IsNullOrEmpty(levelName))
            LevelProgress.SetPendingLevelByName(chapterName, levelName, levelNumber);
        else
            LevelProgress.SetPendingLevelSelection(chapterIndex, levelIndex, levelNumber);

        SceneTransition.Load(levelSceneName);
    }

    void PlaySound()
    {
        if (interactSound == null) return;
        if (audioSource != null) audioSource.PlayOneShot(interactSound);
        else AudioSource.PlayClipAtPoint(interactSound, transform.position);
    }

    IEnumerator LoadSceneAfterDelay()
    {
        if (transitionDelay > 0f)
            yield return new WaitForSeconds(transitionDelay);

        LevelProgress.SetPendingLevelSelection(chapterIndex, levelIndex, levelNumber);
        SceneTransition.Load(levelSceneName);
    }

    #endregion

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = _playerNear ? Color.green : Color.yellow;
        Vector3 center = transform.position + new Vector3(interactCenterOffset.x, interactCenterOffset.y, 0f);
        Gizmos.DrawWireCube(center, new Vector3(interactHorizontalRange * 2f, interactVerticalRange * 2f, 0.1f));
    }
#endif
}
