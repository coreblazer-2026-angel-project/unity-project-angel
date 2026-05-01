using UnityEngine;

public class OpenSettingsButton : MonoBehaviour
{
    [SerializeField] private SettingsPanelBlur settingsPanelBlur;

    public void OpenSettings()
    {
        SettingsPanelBlur target = settingsPanelBlur;

        if (target == null)
        {
            target = SettingsPanelBlur.Instance;
        }

        if (target == null)
        {
            target = FindObjectOfType<SettingsPanelBlur>();
        }

        if (target != null)
        {
            target.OpenSettings();
            return;
        }

        Debug.LogWarning("OpenSettingsButton could not find a SettingsPanelBlur in this scene.");
    }
}
