using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DungeonEscape
{
    public class UIManager : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject victoryPanel;

        [Header("HUD Elements")]
        [SerializeField] private Slider healthSlider;
        
        [SerializeField] private TextMeshProUGUI keyCountText;
        [SerializeField] private TextMeshProUGUI interactPromptText;

        [Header("Button References (Optional)")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButtonLose;
        [SerializeField] private Button restartButtonWin;
        [SerializeField] private Button menuButtonLose;
        [SerializeField] private Button menuButtonWin;
        [SerializeField] private Button menuButtonPause;

        private int maxHealth = 100;

        private void OnEnable()
        {
            // Suscribirse a eventos globales
            DungeonEvents.OnGameStateChanged += HandleGameStateChanged;
            DungeonEvents.OnPlayerMaxHealthInit += HandleMaxHealthInit;
            DungeonEvents.OnPlayerDamage += UpdateHealthUI;
            DungeonEvents.OnPlayerHeal += UpdateHealthUI;
            DungeonEvents.OnKeyCountChanged += UpdateKeyCountUI;
            DungeonEvents.OnInteractLook += UpdateInteractPrompt;
        }

        private void OnDisable()
        {
            // Desuscribirse
            DungeonEvents.OnGameStateChanged -= HandleGameStateChanged;
            DungeonEvents.OnPlayerMaxHealthInit -= HandleMaxHealthInit;
            DungeonEvents.OnPlayerDamage -= UpdateHealthUI;
            DungeonEvents.OnPlayerHeal -= UpdateHealthUI;
            DungeonEvents.OnKeyCountChanged -= UpdateKeyCountUI;
            DungeonEvents.OnInteractLook -= UpdateInteractPrompt;
        }

        private void Start()
        {
            SetupButtonListeners();
            
            // Ocultar texto de interacción al iniciar
            if (interactPromptText != null)
            {
                interactPromptText.gameObject.SetActive(false);
            }

            // Inicializar el contador de llaves en el HUD de forma segura
            if (keyCountText != null)
            {
                int keys = (GameManager.Instance != null) ? GameManager.Instance.CollectedKeys : 0;
                keyCountText.text = $"Llaves: {keys}";
            }
        }

        private void SetupButtonListeners()
        {
            // Vincular botones a las funciones del GameManager con seguridad null
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(() => {
                    if (GameManager.Instance != null) GameManager.Instance.TogglePause();
                });
            }
                
            if (restartButtonLose != null)
            {
                restartButtonLose.onClick.AddListener(() => {
                    if (GameManager.Instance != null) GameManager.Instance.RestartLevel();
                });
            }
                
            if (restartButtonWin != null)
            {
                restartButtonWin.onClick.AddListener(() => {
                    if (GameManager.Instance != null) GameManager.Instance.RestartLevel();
                });
            }

            if (menuButtonLose != null)
            {
                menuButtonLose.onClick.AddListener(() => {
                    if (GameManager.Instance != null) GameManager.Instance.LoadSceneByName("MainMenu");
                });
            }

            if (menuButtonWin != null)
            {
                menuButtonWin.onClick.AddListener(() => {
                    if (GameManager.Instance != null) GameManager.Instance.LoadSceneByName("MainMenu");
                });
            }

            if (menuButtonPause != null)
            {
                menuButtonPause.onClick.AddListener(() => {
                    if (GameManager.Instance != null) GameManager.Instance.LoadSceneByName("MainMenu");
                });
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            // Desactivar todos los paneles primero
            if (hudPanel != null) hudPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);

            // Activar el panel según el estado
            switch (state)
            {
                case GameState.MainMenu:
                    // En el menú principal generalmente se maneja otra escena con su propio UIManager
                    break;
                case GameState.Playing:
                    if (hudPanel != null) hudPanel.SetActive(true);
                    break;
                case GameState.Paused:
                    if (hudPanel != null) hudPanel.SetActive(true); // Dejar HUD visible detrás
                    if (pausePanel != null) pausePanel.SetActive(true);
                    break;
                case GameState.GameOver:
                    if (gameOverPanel != null) gameOverPanel.SetActive(true);
                    break;
                case GameState.Victory:
                    if (victoryPanel != null) victoryPanel.SetActive(true);
                    break;
            }
        }

        private void HandleMaxHealthInit(int maxHp)
        {
            maxHealth = maxHp;
            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = maxHealth;
            }
        }

        private void UpdateHealthUI(int currentHealth)
        {
            if (healthSlider != null)
            {
                healthSlider.value = currentHealth;
            }
        }

        private void UpdateKeyCountUI(int totalKeys)
        {
            if (keyCountText != null)
            {
                keyCountText.text = $"Llaves: {totalKeys}";
            }
        }

        private void UpdateInteractPrompt(bool show, string promptText)
        {
            if (interactPromptText != null)
            {
                interactPromptText.gameObject.SetActive(show);
                interactPromptText.text = promptText;
            }
        }
    }
}
