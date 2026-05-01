using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OnStartClicked()
    {
        // SceneManager.LoadScene("GameScene"); // 真正切换场景
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