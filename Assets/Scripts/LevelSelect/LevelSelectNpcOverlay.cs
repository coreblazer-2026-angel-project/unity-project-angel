using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Screen Space Overlay 版本的关卡 NPC 提示。
/// 提示 UI 放在屏幕 Canvas 里，脚本把 UI 位置跟随到 NPC 头顶，
/// 这样不会被 SpriteRenderer、Sorting Layer、2D Light 遮住。
/// </summary>
public class LevelSelectNpcOverlay : MonoBehaviour
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
    [Tooltip("玩家和 NPC 的 X 距离小于这个值时，算靠近。WalkingScene 的 X 总范围约 -10 到 10，建议 1.2~1.8。")]
    public float interactHorizontalRange = 1.6f;
    [Tooltip("玩家和 NPC 的 Y 距离小于这个值时，算靠近。横版场景人物脚底/中心点不同，建议 2.0~2.8。")]
    public float interactVerticalRange = 2.4f;
    [Tooltip("交互中心相对 NPC 根节点的偏移。一般保持 0,0；如果 NPC 根节点不在身体中心，再微调。")]
    public Vector2 interactCenterOffset = Vector2.zero;
    public Vector3 worldOffset = new Vector3(0f, 2.15f, 0f);

    [Header("NPC 外观")]
    public SpriteRenderer bodyRenderer;
    public bool affectChildRenderers = true;
    [Tooltip("解锁后才显示的脚下影子或装饰。未解锁剪影状态会隐藏这些对象。")]
    public GameObject[] showOnlyWhenUnlocked;
    public Color lockedSilhouetteColor = new Color32(22, 29, 42, 225);
    public Color completedTintColor = new Color32(255, 255, 255, 255);

    [Header("屏幕 UI")]
    public Canvas overlayCanvas;
    public RectTransform promptRoot;
    [Tooltip("勾上后，每个 NPC 运行时会克隆一份自己的 NpcPrompt，避免多个 NPC 共用同一个 UI 导致标题和位置错乱。")]
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

    Camera _mainCamera;
    bool _playerNear;
    bool _promptPositionInitialized;
    float _targetAlpha;
    SpriteRenderer[] _visualRenderers;
    Color[] _originalRendererColors;
    bool _lastUnlocked;
    bool _lastCompleted;

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
    }

    void Start()
    {
        ResolveRuntimeReferences();

        RefreshVisual();
        if (titleAlwaysVisible)
            ShowTitleOnly();
        else
            HidePromptImmediate();

        ApplyPromptLayout();
        ApplyTextStyle();
    }

    void Update()
    {
        if (_mainCamera == null)
            _mainCamera = Camera.main;

        ResolveRuntimeReferences();
        RefreshVisualIfProgressChanged();
        UpdatePlayerNear();

        if (_playerNear) {
            ShowPrompt();
        } else {
            if (titleAlwaysVisible)
                ShowTitleOnly();
            else
                HidePromptImmediate();
            return;
        }

        FollowNpcOnScreen();
        UpdateFade();

        if (_playerNear && Input.GetKeyDown(interactKey))
            TryEnterLevel();
    }

    void UpdatePlayerNear()
    {
        if (player == null) {
            _playerNear = false;
            return;
        }

        Vector2 center = (Vector2)transform.position + interactCenterOffset;
        Vector2 playerPos = player.position;

        float dx = Mathf.Abs(playerPos.x - center.x);
        float dy = Mathf.Abs(playerPos.y - center.y);
        _playerNear = dx <= interactHorizontalRange && dy <= interactVerticalRange;
    }

    void ResolveRuntimeReferences()
    {
        if (player == null || player == transform || player.GetComponent<LevelSelectNpcOverlay>() != null) {
            GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
            if (taggedPlayer != null && taggedPlayer.transform != transform) {
                player = taggedPlayer.transform;
            } else {
                var foundPlayer = FindObjectOfType<SideScrollPlayer>();
                if (foundPlayer != null && foundPlayer.transform != transform)
                    player = foundPlayer.transform;
            }
        }

        if (player == transform)
            player = null;
    }

    void RefreshVisual()
    {
        if (_visualRenderers == null || _visualRenderers.Length == 0)
            CacheVisualRenderers();

        bool unlocked = LevelProgress.IsUnlocked(levelNumber);
        bool completed = LevelProgress.IsCompleted(levelNumber);
        _lastUnlocked = unlocked;
        _lastCompleted = completed;

        for (int i = 0; i < _visualRenderers.Length; i++) {
            if (_visualRenderers[i] == null) continue;

            Color baseColor = i < _originalRendererColors.Length
                ? _originalRendererColors[i]
                : Color.white;

            _visualRenderers[i].color = unlocked
                ? (completed ? MultiplyColor(baseColor, completedTintColor) : baseColor)
                : lockedSilhouetteColor;
        }

        SetUnlockedOnlyObjects(unlocked);
    }

    void RefreshVisualIfProgressChanged()
    {
        bool unlocked = LevelProgress.IsUnlocked(levelNumber);
        bool completed = LevelProgress.IsCompleted(levelNumber);

        if (unlocked != _lastUnlocked || completed != _lastCompleted)
            RefreshVisual();
    }

    void ClonePromptForThisNpc()
    {
        RectTransform sourceRoot = promptRoot;
        RectTransform clonedRoot = Instantiate(sourceRoot, sourceRoot.parent);
        clonedRoot.name = $"{sourceRoot.name}_{levelNumber}_{npcName}";
        clonedRoot.SetSiblingIndex(sourceRoot.GetSiblingIndex() + 1);

        promptRoot = clonedRoot;
        promptGroup = clonedRoot.GetComponent<CanvasGroup>();
        if (promptGroup == null)
            promptGroup = clonedRoot.gameObject.AddComponent<CanvasGroup>();

        markerText = FindPromptText(clonedRoot, "MarkerText");
        leftArrowText = FindPromptText(clonedRoot, "LeftArrowText");
        titleText = FindPromptText(clonedRoot, "TitleText");
        rightArrowText = FindPromptText(clonedRoot, "RightArrowText");
        statusText = FindPromptText(clonedRoot, "StatusText");

        clonedRoot.gameObject.SetActive(false);
        if (hidePromptTemplateAfterClone)
            sourceRoot.gameObject.SetActive(false);
    }

    TMP_Text FindPromptText(RectTransform root, string childName)
    {
        TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++) {
            if (texts[i].name == childName)
                return texts[i];
        }
        return null;
    }

    void CacheVisualRenderers()
    {
        if (affectChildRenderers) {
            _visualRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        } else if (bodyRenderer != null) {
            _visualRenderers = new[] { bodyRenderer };
        } else {
            _visualRenderers = new SpriteRenderer[0];
        }

        _originalRendererColors = new Color[_visualRenderers.Length];
        for (int i = 0; i < _visualRenderers.Length; i++) {
            _originalRendererColors[i] = _visualRenderers[i] != null
                ? _visualRenderers[i].color
                : Color.white;
        }
    }

    Color MultiplyColor(Color a, Color b)
    {
        return new Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);
    }

    void SetUnlockedOnlyObjects(bool unlocked)
    {
        if (showOnlyWhenUnlocked == null)
            return;

        for (int i = 0; i < showOnlyWhenUnlocked.Length; i++) {
            if (showOnlyWhenUnlocked[i] != null)
                showOnlyWhenUnlocked[i].SetActive(unlocked);
        }
    }

    void ShowPrompt()
    {
        bool unlocked = LevelProgress.IsUnlocked(levelNumber);
        bool completed = LevelProgress.IsCompleted(levelNumber);

        SetPromptScreenPosition(true);

        if (promptRoot != null)
            promptRoot.gameObject.SetActive(true);

        _targetAlpha = 1f;

        if (markerText != null) {
            markerText.gameObject.SetActive(true);
            markerText.text = "▼";
        }

        if (leftArrowText != null) {
            leftArrowText.gameObject.SetActive(true);
            leftArrowText.text = ">";
        }

        if (titleText != null) {
            titleText.gameObject.SetActive(true);
            titleText.text = GetTitleText();
        }

        if (rightArrowText != null) {
            rightArrowText.gameObject.SetActive(true);
            rightArrowText.text = "<";
        }

        if (statusText != null) {
            statusText.gameObject.SetActive(true);

            if (!unlocked) {
                statusText.text = lockedStatus;
                statusText.color = lockedStatusColor;
            } else if (completed) {
                statusText.text = completedStatus;
                statusText.color = completedStatusColor;
            } else {
                statusText.text = availableStatus;
                statusText.color = availableStatusColor;
            }
        }

        ApplyPromptLayout();
        ApplyTextStyle();
    }

    void HidePrompt()
    {
        _targetAlpha = 0f;

        if (!useFade)
            HidePromptImmediate();
    }

    string GetTitleText()
    {
        if (!showLevelNumber)
            return npcName;

        string format = string.IsNullOrWhiteSpace(titleFormat) || titleFormat == "{1}"
            ? "{0}  {1}"
            : titleFormat;

        return string.Format(format, levelNumber, npcName);
    }

    void HidePromptImmediate()
    {
        _targetAlpha = 0f;
        _promptPositionInitialized = false;

        if (promptGroup != null)
            promptGroup.alpha = 0f;

        if (promptRoot != null)
            promptRoot.gameObject.SetActive(false);
    }

    void ShowTitleOnly()
    {
        SetPromptScreenPosition(!_promptPositionInitialized);

        if (promptRoot != null)
            promptRoot.gameObject.SetActive(true);

        _targetAlpha = 1f;

        if (promptGroup != null)
            promptGroup.alpha = 1f;

        if (titleText != null) {
            titleText.gameObject.SetActive(true);
            titleText.text = GetTitleText();
        }

        if (markerText != null)
            markerText.gameObject.SetActive(false);

        if (leftArrowText != null)
            leftArrowText.gameObject.SetActive(false);

        if (rightArrowText != null)
            rightArrowText.gameObject.SetActive(false);

        if (statusText != null)
            statusText.gameObject.SetActive(false);

        ApplyPromptLayout();
        ApplyTextStyle();
        FollowNpcOnScreen();
    }

    void FollowNpcOnScreen()
    {
        SetPromptScreenPosition(false);
    }

    void SetPromptScreenPosition(bool instant)
    {
        if (_mainCamera == null || promptRoot == null) return;

        Vector3 screenPos = _mainCamera.WorldToScreenPoint(transform.position + worldOffset);
        Vector2 targetPosition;

        if (overlayCanvas != null && overlayCanvas.renderMode != RenderMode.WorldSpace) {
            RectTransform canvasRect = overlayCanvas.transform as RectTransform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                overlayCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _mainCamera,
                out targetPosition
            );
            targetPosition += screenOffset;

            if (instant || !_promptPositionInitialized) {
                promptRoot.anchoredPosition = targetPosition;
            } else {
                promptRoot.anchoredPosition = Vector2.Lerp(
                    promptRoot.anchoredPosition,
                    targetPosition,
                    1f - Mathf.Exp(-followSmooth * Time.deltaTime)
                );
            }
        } else {
            if (instant || !_promptPositionInitialized) {
                promptRoot.position = screenPos;
            } else {
                promptRoot.position = Vector3.Lerp(
                    promptRoot.position,
                    screenPos,
                    1f - Mathf.Exp(-followSmooth * Time.deltaTime)
                );
            }
        }

        _promptPositionInitialized = true;
    }

    void ApplyPromptLayout()
    {
        if (!autoArrangePrompt) return;

        SetTextPosition(markerText, 0f, markerY);
        SetTextPosition(titleText, 0f, titleY);
        SetTextPosition(leftArrowText, -arrowDistance, titleY);
        SetTextPosition(rightArrowText, arrowDistance, titleY);
        SetTextPosition(statusText, 0f, statusY);
    }

    void SetTextPosition(TMP_Text text, float x, float y)
    {
        if (text == null) return;

        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(x, y);
        text.alignment = TextAlignmentOptions.Center;
    }

    void ApplyTextStyle()
    {
        if (!autoApplyTextStyle) return;

        ApplyTextStyle(markerText, markerFontSize, markerColor);
        ApplyTextStyle(leftArrowText, arrowFontSize, mainTextColor);
        ApplyTextStyle(titleText, titleFontSize, mainTextColor);
        ApplyTextStyle(rightArrowText, arrowFontSize, mainTextColor);

        if (statusText != null)
            ApplyTextStyle(statusText, statusFontSize, statusText.color);
    }

    void ApplyTextStyle(TMP_Text text, float fontSize, Color faceColor)
    {
        if (text == null) return;

        text.fontSize = fontSize;
        text.color = faceColor;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;
        text.fontMaterial.SetColor(ShaderUtilities.ID_FaceColor, faceColor);
        text.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, outlineColor);
        text.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, outlineWidth);
    }

    void UpdateFade()
    {
        if (!useFade) {
            if (promptGroup != null)
                promptGroup.alpha = _targetAlpha;
            return;
        }

        if (promptGroup == null) {
            if (promptRoot != null)
                promptRoot.gameObject.SetActive(_targetAlpha > 0f);
            return;
        }

        promptGroup.alpha = Mathf.Lerp(
            promptGroup.alpha,
            _targetAlpha,
            1f - Mathf.Exp(-fadeSpeed * Time.deltaTime)
        );

        bool visible = promptGroup.alpha > 0.01f || _targetAlpha > 0f;
        if (promptRoot != null && promptRoot.gameObject.activeSelf != visible)
            promptRoot.gameObject.SetActive(visible);
    }

    void TryEnterLevel()
    {
        if (!LevelProgress.IsUnlocked(levelNumber)) {
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
        Gizmos.color = _playerNear ? Color.green : Color.yellow;
        Vector3 center = transform.position + new Vector3(interactCenterOffset.x, interactCenterOffset.y, 0f);
        Gizmos.DrawWireCube(center, new Vector3(interactHorizontalRange * 2f, interactVerticalRange * 2f, 0.1f));
    }
#endif
}
