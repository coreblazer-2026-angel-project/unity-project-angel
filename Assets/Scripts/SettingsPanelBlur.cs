using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsPanelBlur : MonoBehaviour
{
    public static SettingsPanelBlur Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Material blurMaterial;

    [Header("Scene Loading")]
    [SerializeField] private bool persistAcrossScenes = true;
    [SerializeField] private bool closeOnSceneLoaded = true;

    [Header("Blur")]
    [SerializeField, Range(0f, 6f)] private float openRadius = 2.5f;
    [SerializeField, Range(0f, 1f)] private float openDarken = 0.25f;

    private static readonly int RadiusId = Shader.PropertyToID("_Radius");
    private static readonly int DarkenId = Shader.PropertyToID("_Darken");

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            ResetBlurMaterial();
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (persistAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        SetOpen(false);
    }

    private void OnDisable()
    {
        ResetBlurMaterial();
    }

    private void OnApplicationQuit()
    {
        ResetBlurMaterial();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        ResetBlurMaterial();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void ResetBlurMaterial()
    {
        if (blurMaterial != null)
        {
            blurMaterial.SetFloat(RadiusId, 0f);
            blurMaterial.SetFloat(DarkenId, 0f);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (closeOnSceneLoaded)
        {
            SetOpen(false);
        }
    }

    public void OpenSettings()
    {
        SetOpen(true);
    }

    public void CloseSettings()
    {
        SetOpen(false);
    }

    public void ToggleSettings()
    {
        SetOpen(settingsPanel == null || !settingsPanel.activeSelf);
    }

    public void SetOpen(bool open)
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(open);
        }

        if (blurMaterial != null)
        {
            blurMaterial.SetFloat(RadiusId, open ? openRadius : 0f);
            blurMaterial.SetFloat(DarkenId, open ? openDarken : 0f);
        }
    }
}
