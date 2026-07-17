using UnityEngine;
using UnityEngine.UI;

namespace DungeonEscape
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button quitButton;

        [Header("Scene Settings")]
        [SerializeField] private string gameplaySceneName = "SampleScene";

        private void Start()
        {
            if (playButton != null) playButton.onClick.AddListener(StartGame);
            if (quitButton != null) quitButton.onClick.AddListener(QuitGame);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void StartGame()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadSceneByName(gameplaySceneName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(gameplaySceneName);
            }
        }

        private void QuitGame()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitGame();
            }
            else
            {
                Application.Quit();
            }
        }
    }
}
