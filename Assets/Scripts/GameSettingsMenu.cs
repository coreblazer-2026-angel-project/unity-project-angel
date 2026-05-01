using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class GameSettingsMenu : MonoBehaviour
{
    private const string MasterVolumeKey = "Settings.MasterVolume";
    private const string ResolutionWidthKey = "Settings.ResolutionWidth";
    private const string ResolutionHeightKey = "Settings.ResolutionHeight";
    private const string FullScreenModeKey = "Settings.FullScreenMode";

    [Header("Audio")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string masterVolumeParameter = "MasterVolume";

    [Header("Video")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown windowModeDropdown;

    private readonly List<Resolution> uniqueResolutions = new List<Resolution>();

    private void Awake()
    {
        SetupVolume();
        SetupResolutions();
        SetupWindowModes();
    }

    private void OnEnable()
    {
        RefreshControls();
    }

    public void SetMasterVolume(float value)
    {
        value = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MasterVolumeKey, value);

        if (audioMixer != null)
        {
            float decibels = value <= 0.0001f ? -80f : Mathf.Log10(value) * 20f;
            audioMixer.SetFloat(masterVolumeParameter, decibels);
        }
        else
        {
            AudioListener.volume = value;
        }
    }

    public void SetResolution(int index)
    {
        if (index < 0 || index >= uniqueResolutions.Count)
        {
            return;
        }

        Resolution resolution = uniqueResolutions[index];
        PlayerPrefs.SetInt(ResolutionWidthKey, resolution.width);
        PlayerPrefs.SetInt(ResolutionHeightKey, resolution.height);
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
    }

    public void SetWindowMode(int index)
    {
        FullScreenMode mode = index switch
        {
            0 => FullScreenMode.ExclusiveFullScreen,
            1 => FullScreenMode.FullScreenWindow,
            _ => FullScreenMode.Windowed
        };

        PlayerPrefs.SetInt(FullScreenModeKey, (int)mode);

        int width = PlayerPrefs.GetInt(ResolutionWidthKey, Screen.width);
        int height = PlayerPrefs.GetInt(ResolutionHeightKey, Screen.height);
        Screen.SetResolution(width, height, mode);
    }

    public void Save()
    {
        PlayerPrefs.Save();
    }

    private void SetupVolume()
    {
        float volume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        SetMasterVolume(volume);

        if (masterVolumeSlider == null)
        {
            return;
        }

        masterVolumeSlider.minValue = 0f;
        masterVolumeSlider.maxValue = 1f;
        masterVolumeSlider.SetValueWithoutNotify(volume);
        masterVolumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
    }

    private void SetupResolutions()
    {
        if (resolutionDropdown == null)
        {
            return;
        }

        EnsureDropdownTextBindings(resolutionDropdown);
        uniqueResolutions.Clear();
        resolutionDropdown.ClearOptions();

        foreach (Resolution resolution in Screen.resolutions)
        {
            bool exists = uniqueResolutions.Exists(item =>
                item.width == resolution.width && item.height == resolution.height);

            if (!exists)
            {
                uniqueResolutions.Add(resolution);
            }
        }

        if (uniqueResolutions.Count == 0)
        {
            uniqueResolutions.Add(new Resolution
            {
                width = Screen.currentResolution.width,
                height = Screen.currentResolution.height
            });
        }

        List<string> options = new List<string>();
        int savedWidth = PlayerPrefs.GetInt(ResolutionWidthKey, Screen.width);
        int savedHeight = PlayerPrefs.GetInt(ResolutionHeightKey, Screen.height);
        int selectedIndex = 0;

        for (int i = 0; i < uniqueResolutions.Count; i++)
        {
            Resolution resolution = uniqueResolutions[i];
            options.Add($"{resolution.width} x {resolution.height}");

            if (resolution.width == savedWidth && resolution.height == savedHeight)
            {
                selectedIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.SetValueWithoutNotify(selectedIndex);
        resolutionDropdown.RefreshShownValue();
        resolutionDropdown.onValueChanged.RemoveListener(SetResolution);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    private void SetupWindowModes()
    {
        if (windowModeDropdown == null)
        {
            return;
        }

        EnsureDropdownTextBindings(windowModeDropdown);
        windowModeDropdown.ClearOptions();
        windowModeDropdown.AddOptions(new List<string>
        {
            "独占全屏",
            "无边框全屏",
            "窗口模式"
        });

        FullScreenMode mode = (FullScreenMode)PlayerPrefs.GetInt(
            FullScreenModeKey,
            (int)Screen.fullScreenMode
        );

        int selectedIndex = mode switch
        {
            FullScreenMode.ExclusiveFullScreen => 0,
            FullScreenMode.FullScreenWindow => 1,
            _ => 2
        };

        windowModeDropdown.SetValueWithoutNotify(selectedIndex);
        windowModeDropdown.RefreshShownValue();
        windowModeDropdown.onValueChanged.RemoveListener(SetWindowMode);
        windowModeDropdown.onValueChanged.AddListener(SetWindowMode);
    }

    private void EnsureDropdownTextBindings(TMP_Dropdown dropdown)
    {
        if (dropdown.captionText == null)
        {
            TMP_Text label = dropdown.transform.Find("Label")?.GetComponent<TMP_Text>();
            if (label != null)
            {
                dropdown.captionText = label;
            }
        }

        if (dropdown.itemText == null)
        {
            Transform itemLabel = dropdown.transform.Find("Template/Viewport/Content/Item/Item Label");
            if (itemLabel != null && itemLabel.TryGetComponent(out TMP_Text text))
            {
                dropdown.itemText = text;
            }
        }
    }

    private void RefreshControls()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(MasterVolumeKey, 1f));
        }

        if (windowModeDropdown != null)
        {
            FullScreenMode mode = Screen.fullScreenMode;
            int selectedIndex = mode switch
            {
                FullScreenMode.ExclusiveFullScreen => 0,
                FullScreenMode.FullScreenWindow => 1,
                _ => 2
            };

            windowModeDropdown.SetValueWithoutNotify(selectedIndex);
        }
    }
}
