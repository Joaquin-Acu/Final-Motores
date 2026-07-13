using UnityEngine;
using UnityEngine.InputSystem;

namespace DungeonEscape
{
    public class MouseLook : MonoBehaviour
    {
        [Header("Camera & Rotation Settings")]
        [SerializeField] private Transform playerCamera;
        [SerializeField] private float mouseSensitivity = 20f;
        [SerializeField] private float clampAngle = 85f;

        private PlayerInput playerInput;
        private InputAction lookAction;
        private float xRotation = 0f;
        private bool isCursorLocked = true;

        private void Start()
        {
            playerInput = GetComponent<PlayerInput>();
            if (playerInput != null && playerInput.actions != null)
            {
                lookAction = playerInput.actions.FindAction("Look");
            }
            else
            {
                Debug.LogWarning("No se encontró PlayerInput en MouseLook.");
            }

            // Bloquear el cursor al iniciar
            SetCursorLock(true);
            
            // Suscribirse al evento de cambio de estado de juego para liberar el cursor
            DungeonEvents.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDestroy()
        {
            DungeonEvents.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void Update()
        {
            // Solo rotar la cámara si el cursor está bloqueado y el juego está en marcha
            if (isCursorLocked && lookAction != null)
            {
                Vector2 lookInput = lookAction.ReadValue<Vector2>();

                // En la versión 6000.x del Input System, el mouse delta se procesa
                float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
                float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, -clampAngle, clampAngle);

                // Rotar la cámara sobre el eje X (arriba/abajo)
                playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

                // Rotar el cuerpo del jugador sobre el eje Y (izquierda/derecha)
                transform.Rotate(Vector3.up * mouseX);
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            // Bloquear cursor si está jugando, liberarlo si está en menú, pausa o fin de juego
            if (state == GameState.Playing)
            {
                SetCursorLock(true);
            }
            else
            {
                SetCursorLock(false);
            }
        }

        private void SetCursorLock(bool locked)
        {
            isCursorLocked = locked;
            if (locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
