using UnityEngine;
using UnityEngine.InputSystem;

namespace DungeonEscape
{
    [RequireComponent(typeof(Rigidbody))]
    public class MouseLook : MonoBehaviour
    {
        [Header("Camera & Rotation Settings")]
        [SerializeField] private Transform playerCamera;
        [SerializeField] private float mouseSensitivity = 0.08f;
        [SerializeField] private float clampAngle = 85f;

        private PlayerInput playerInput;
        private InputAction lookAction;
        private Rigidbody rb;

        private float xRotation = 0f;
        private float yRotation = 0f;
        private bool isCursorLocked = true;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

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

            // Inicializar la rotación vertical inicial
            xRotation = playerCamera.localEulerAngles.x;
            if (xRotation > 180f) xRotation -= 360f; // Ajustar a rango -180 a 180

            // Inicializar yRotation con la rotación horizontal inicial del cuerpo del jugador
            yRotation = transform.localEulerAngles.y;

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
            // Procesar la entrada en Update para una vista instantánea y sin retardo
            if (isCursorLocked && lookAction != null)
            {
                Vector2 lookInput = lookAction.ReadValue<Vector2>();

                float mouseX = lookInput.x * mouseSensitivity;
                float mouseY = lookInput.y * mouseSensitivity;

                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, -clampAngle, clampAngle);
                
                yRotation += mouseX;

                // Rotar la cámara en Update (eje X vertical) para evitar retrasos visuales
                playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            }
        }

        private void FixedUpdate()
        {
            if (isCursorLocked && rb != null)
            {
                // Aplicar la rotación del cuerpo (eje Y horizontal) en FixedUpdate usando MoveRotation.
                // Al hacerlo aquí, se sincroniza perfectamente con el motor de físicas de Unity 
                // y se beneficia del "Rigidbody Interpolation", eliminando por completo los tirones.
                rb.MoveRotation(Quaternion.Euler(0f, yRotation, 0f));
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
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
