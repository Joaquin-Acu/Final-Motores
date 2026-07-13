using UnityEngine;
using UnityEngine.InputSystem;

namespace DungeonEscape
{
    public class PauseController : MonoBehaviour
    {
        private void Update()
        {
            // Detectar la tecla Escape (Teclado) o el botón Start (Gamepad) directamente a través de la API del Input System
            bool escPressed = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
            bool startPressed = Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;

            if (escPressed || startPressed)
            {
                TriggerPause();
            }
        }

        private void TriggerPause()
        {
            if (GameManager.Instance != null)
            {
                // Solo alternar pausa si estamos jugando o si ya está pausado
                if (GameManager.Instance.CurrentState == GameState.Playing || 
                    GameManager.Instance.CurrentState == GameState.Paused)
                {
                    GameManager.Instance.TogglePause();
                }
            }
        }
    }
}
