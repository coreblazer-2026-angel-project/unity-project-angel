using Game.Story;
using UnityEngine;

/// <summary>
/// Hides a HUD element while story dialogue is playing.
/// Add this to HopeProgress or any always-present HUD object.
/// </summary>
public class HideDuringStory : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Leave empty to hide this object visually with a CanvasGroup.")]
    public CanvasGroup targetGroup;

    [Tooltip("Optional root. If set and targetGroup is empty, a CanvasGroup is added here.")]
    public GameObject targetRoot;

    [Header("Hide Conditions")]
    public bool hideWhenStoryPlaying = true;

    [Tooltip("Usually drag DialogRoot here, not the outer DialogWidget object.")]
    public GameObject hideWhenObjectActive;

    [Tooltip("Optional. If set, this script checks panel.root.activeInHierarchy.")]
    public StoryDialoguePanel dialoguePanel;

    [Header("Display")]
    [Tooltip("Keep a little transparency instead of fully hidden.")]
    [Range(0f, 1f)] public float hiddenAlpha = 0f;

    [Tooltip("Fade speed. Set to 0 for instant hide/show.")]
    public float fadeSpeed = 12f;

    float _targetAlpha = 1f;
    bool _subscribed;

    void Awake()
    {
        EnsureTargetGroup();
        if (dialoguePanel == null)
            dialoguePanel = FindObjectOfType<StoryDialoguePanel>();
    }

    void OnEnable()
    {
        TrySubscribe();
    }

    void OnDisable()
    {
        if (_subscribed && StoryManager.Instance != null) {
            StoryManager.Instance.OnStoryStarted -= Hide;
            StoryManager.Instance.OnStoryEnded -= Show;
        }
        _subscribed = false;
    }

    void Update()
    {
        TrySubscribe();
        RefreshVisibilityFromState();

        if (targetGroup == null)
            return;

        if (fadeSpeed <= 0f) {
            targetGroup.alpha = _targetAlpha;
            return;
        }

        targetGroup.alpha = Mathf.MoveTowards(
            targetGroup.alpha,
            _targetAlpha,
            Time.unscaledDeltaTime * fadeSpeed);
    }

    void Hide()
    {
        SetVisible(false, false);
    }

    void Show()
    {
        RefreshVisibilityFromState();
    }

    void SetVisible(bool visible, bool instant)
    {
        EnsureTargetGroup();
        if (targetGroup == null)
            return;

        _targetAlpha = visible ? 1f : hiddenAlpha;
        targetGroup.blocksRaycasts = visible;
        targetGroup.interactable = visible;

        if (instant)
            targetGroup.alpha = _targetAlpha;
    }

    void RefreshVisibilityFromState()
    {
        bool shouldHide = false;

        if (hideWhenStoryPlaying && StoryManager.Instance != null && StoryManager.Instance.IsPlaying)
            shouldHide = true;

        if (hideWhenObjectActive != null && hideWhenObjectActive.activeInHierarchy)
            shouldHide = true;

        if (dialoguePanel != null && dialoguePanel.root != null && dialoguePanel.root.activeInHierarchy)
            shouldHide = true;

        SetVisible(!shouldHide, false);
    }

    void EnsureTargetGroup()
    {
        if (targetGroup != null)
            return;

        GameObject root = targetRoot != null ? targetRoot : gameObject;
        targetGroup = root.GetComponent<CanvasGroup>();
        if (targetGroup == null)
            targetGroup = root.AddComponent<CanvasGroup>();
    }

    void TrySubscribe()
    {
        if (_subscribed || StoryManager.Instance == null)
            return;

        StoryManager.Instance.OnStoryStarted += Hide;
        StoryManager.Instance.OnStoryEnded += Show;
        _subscribed = true;
        RefreshVisibilityFromState();
    }
}
