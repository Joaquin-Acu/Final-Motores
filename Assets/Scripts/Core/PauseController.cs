using UnityEngine;
using UnityEngine.InputSystem;

namespace DungeonEscape
{
    public class PauseController : MonoBehaviour
    {
        [Header("Input Action Reference")]
        [SerializeField] private InputActionReference pauseActionReference;

        private void OnEnable()
        {
            if (pauseActionReference != null)
            {
                pauseActionReference.action.Enable();
                pauseActionReference.action.performed += OnPausePerformed;
            }
        }

        private void OnDisable()
        {
            if (pauseActionReference != null)
            {
                pauseActionReference.action.performed -= OnPausePerformed;
                pauseActionReference.action.Disable();
            }
        }

        private void OnPausePerformed(InputAction.CallbackContext context)
        {
            if (GameManager.Instance != null)
            {
                // Solo pausar si estamos jugando o si ya está pausado
                if (GameManager.Instance.CurrentState == GameState.Playing || 
                    GameManager.Instance.CurrentState == GameState.Paused)
                {
                    GameManager.Instance.TogglePause();
                }
            }
        }
    }
}
