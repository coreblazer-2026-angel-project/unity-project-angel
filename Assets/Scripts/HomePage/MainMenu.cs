using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OnStartClicked()
    {
        LevelProgress.ResetProgress();
        SceneTransition.Load("WalkingScene");
    }

    public void OnContinueClicked()
    {
        SceneTransition.Load("WalkingScene");
    }

    public void OnSettingsClicked()
    {
        // 弹出设置面板
    }

    public void OnExitClicked()
    {
        Application.Quit();
    }
}
