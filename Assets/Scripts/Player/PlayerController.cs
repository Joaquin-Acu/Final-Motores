using UnityEngine;
using UnityEngine.InputSystem;

namespace DungeonEscape
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float sprintMultiplier = 1.5f;
        [SerializeField] private float jumpForce = 5f;
        
        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundDistance = 0.4f;
        [SerializeField] private LayerMask groundMask;

        private Rigidbody rb;
        private PlayerInput playerInput;
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction sprintAction;

        private bool isGrounded;
        private Vector2 moveInput;
        private bool isSprinting;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            playerInput = GetComponent<PlayerInput>();
            
            // Asegurar que el Rigidbody no se caiga ni rote con la física
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void Start()
        {
            // Inicializar las acciones desde el PlayerInput
            if (playerInput != null && playerInput.actions != null)
            {
                moveAction = playerInput.actions.FindAction("Move");
                jumpAction = playerInput.actions.FindAction("Jump");
                sprintAction = playerInput.actions.FindAction("Sprint");
            }
            else
            {
                Debug.LogError("PlayerInput o sus acciones no están configuradas en el PlayerController.");
            }
        }

        private void Update()
        {
            // Leer inputs de las acciones configuradas
            if (moveAction != null)
            {
                moveInput = moveAction.ReadValue<Vector2>();
            }

            if (sprintAction != null)
            {
                isSprinting = sprintAction.IsPressed();
            }

            // Ground Check
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

            // Saltar
            if (jumpAction != null && jumpAction.WasPressedThisFrame() && isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }

        private void FixedUpdate()
        {
            MovePlayer();
        }

        private void MovePlayer()
        {
            // Calcular dirección del movimiento relativa al jugador (su rotación local)
            Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
            moveDirection.Normalize();

            // Calcular velocidad
            float currentSpeed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);
            Vector3 targetVelocity = moveDirection * currentSpeed;

            // Mantener la velocidad vertical del Rigidbody (gravedad)
            targetVelocity.y = rb.linearVelocity.y;

            // Aplicar velocidad al Rigidbody
            rb.linearVelocity = targetVelocity;
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
            }
        }
    }
}
