using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [SerializeField] private float fadeDuration = 0.4f;

    private Canvas _canvas;
    private CanvasGroup _group;
    private bool _transitioning;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitCanvas();
    }

    void InitCanvas()
    {
        _canvas = GetComponent<Canvas>();
        if (_canvas == null) _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 9999;

        _group = GetComponent<CanvasGroup>();
        if (_group == null) _group = gameObject.AddComponent<CanvasGroup>();
        _group.alpha = 0f;
        _group.blocksRaycasts = false;

        var image = GetComponent<UnityEngine.UI.Image>();
        if (image == null) image = gameObject.AddComponent<UnityEngine.UI.Image>();
        image.color = Color.black;
        image.raycastTarget = true;
    }

    static void EnsureInstance()
    {
        if (Instance != null) return;

        var go = new GameObject("SceneTransition");
        go.AddComponent<SceneTransition>();
    }

    public static void Load(string sceneName)
    {
        EnsureInstance();

        if (!Instance._transitioning)
            Instance.StartCoroutine(Instance.FadeAndLoad(sceneName));
    }

    IEnumerator FadeAndLoad(string sceneName)
    {
        _transitioning = true;
        _group.blocksRaycasts = true;

        // 淡入黑屏
        yield return FadeTo(1f);

        // 后台加载场景
        var op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;
        while (op.progress < 0.9f) yield return null;
        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;

        // 等新场景渲染一帧再淡出，避免掉帧卡在淡出动画里
        yield return null;

        // 淡出黑屏
        yield return FadeTo(0f);

        _group.blocksRaycasts = false;
        _transitioning = false;
    }

    IEnumerator FadeTo(float target)
    {
        float start = _group.alpha;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _group.alpha = Mathf.Lerp(start, target, elapsed / fadeDuration);
            yield return null;
        }
        _group.alpha = target;
    }
}
