using UnityEngine;
using UnityEngine.UI;

public class EscBackToMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    void Awake()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        if (yesButton != null)
            yesButton.onClick.AddListener(OnYes);
        if (noButton != null)
            noButton.onClick.AddListener(OnNo);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (confirmPanel != null)
                confirmPanel.SetActive(!confirmPanel.activeSelf);
        }
    }

    void OnYes()
    {
        confirmPanel.SetActive(false);
        SceneTransition.Load("HomePage_end");
    }

    void OnNo()
    {
        confirmPanel.SetActive(false);
    }
}
