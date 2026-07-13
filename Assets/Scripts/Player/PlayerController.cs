using UnityEngine;
using UnityEngine.InputSystem;

namespace DungeonEscape
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
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

        [Header("Input Actions")]
        [SerializeField] private InputActionReference moveActionReference;
        [SerializeField] private InputActionReference jumpActionReference;
        [SerializeField] private InputActionReference sprintActionReference;

        private Rigidbody rb;
        private bool isGrounded;
        private Vector2 moveInput;
        private bool isSprinting;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            
            // Asegurar que el Rigidbody no se caiga ni rote con la física
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        private void OnEnable()
        {
            if (moveActionReference != null) moveActionReference.action.Enable();
            if (jumpActionReference != null) jumpActionReference.action.Enable();
            if (sprintActionReference != null) sprintActionReference.action.Enable();
        }

        private void OnDisable()
        {
            if (moveActionReference != null) moveActionReference.action.Disable();
            if (jumpActionReference != null) jumpActionReference.action.Disable();
            if (sprintActionReference != null) sprintActionReference.action.Disable();
        }

        private void Update()
        {
            // Leer inputs del Input System
            if (moveActionReference != null)
            {
                moveInput = moveActionReference.action.ReadValue<Vector2>();
            }

            if (sprintActionReference != null)
            {
                isSprinting = sprintActionReference.action.IsPressed();
            }

            // Ground Check
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

            // Saltar
            if (jumpActionReference != null && jumpActionReference.action.WasPressedThisFrame() && isGrounded)
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

            // Mantener la velocidad vertical actual del Rigidbody (gravedad)
            targetVelocity.y = rb.linearVelocity.y; // En Unity 6 se usa rb.linearVelocity en lugar de rb.velocity (aunque rb.velocity sigue existiendo, linearVelocity es el estándar nuevo)

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
