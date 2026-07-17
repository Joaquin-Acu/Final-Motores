using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonEscape
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.MainMenu;
        [SerializeField] private int initialKeys = 0;

        private int collectedKeys = 0;

        public GameState CurrentState => currentState;
        public int CollectedKeys => collectedKeys;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            DungeonEvents.OnKeyCollected += AddKeys;
        }

        private void OnDisable()
        {
            DungeonEvents.OnKeyCollected -= AddKeys;
        }

        private void Start()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            if (currentSceneName.Contains("Menu"))
            {
                ChangeState(GameState.MainMenu);
            }
            else
            {
                ChangeState(GameState.Playing);
            }
        }

        public void ChangeState(GameState newState)
        {
            currentState = newState;

            switch (currentState)
            {
                case GameState.MainMenu:
                    Time.timeScale = 1f;
                    collectedKeys = 0;
                    break;
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.GameOver:
                    Time.timeScale = 0f;
                    break;
                case GameState.Victory:
                    Time.timeScale = 0f;
                    break;
            }

            DungeonEvents.OnGameStateChanged?.Invoke(currentState);
        }

        private void AddKeys(int amount)
        {
            collectedKeys += amount;
            DungeonEvents.OnKeyCountChanged?.Invoke(collectedKeys);
        }

        public bool TryUseKeys(int amount)
        {
            if (collectedKeys >= amount)
            {
                collectedKeys -= amount;
                DungeonEvents.OnKeyCountChanged?.Invoke(collectedKeys);
                return true;
            }
            return false;
        }

        public void TogglePause()
        {
            if (currentState == GameState.Playing)
            {
                ChangeState(GameState.Paused);
            }
            else if (currentState == GameState.Paused)
            {
                ChangeState(GameState.Playing);
            }
        }

        public void WinGame()
        {
            ChangeState(GameState.Victory);
        }

        public void RestartLevel()
        {
            collectedKeys = initialKeys;
            ChangeState(GameState.Playing);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void LoadSceneByName(string sceneName)
        {
            if (sceneName.Contains("Menu"))
            {
                ChangeState(GameState.MainMenu);
            }
            else
            {
                ChangeState(GameState.Playing);
            }
            SceneManager.LoadScene(sceneName);
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
