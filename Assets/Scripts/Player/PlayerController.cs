using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Audio;

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

        [Header("Footsteps Settings")]
        [SerializeField] private AudioClip[] footstepClips; // Array de sonidos de pisadas (.wav)
        [SerializeField] private AudioSource footstepAudioSource; // AudioSource en el jugador
        [SerializeField] private AudioMixerGroup sfxGroup; // Enrutar al canal de SFX

        private Rigidbody rb;
        private PlayerInput playerInput;
        private InputAction moveAction;
        private InputAction sprintAction;

        private Vector2 moveInput;
        private bool isSprinting;

        public bool IsMakingNoise => moveInput.magnitude > 0.1f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            playerInput = GetComponent<PlayerInput>();
            
            // Asegurar que el Rigidbody no se caiga ni rote con la física
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            // Inicializar AudioSource de pisadas si no está asignado
            if (footstepAudioSource == null)
            {
                footstepAudioSource = gameObject.AddComponent<AudioSource>();
                footstepAudioSource.playOnAwake = false;
                footstepAudioSource.loop = true;
            }

            // Forzar siempre a 2D para que se escuche claro en primera persona
            footstepAudioSource.spatialBlend = 0f;

            if (sfxGroup != null)
            {
                footstepAudioSource.outputAudioMixerGroup = sfxGroup;
            }
        }

        private void Start()
        {
            // Inicializar las acciones desde el PlayerInput
            if (playerInput != null && playerInput.actions != null)
            {
                moveAction = playerInput.actions.FindAction("Move");
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

            // Procesar las pisadas
            HandleFootsteps();
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

        private void HandleFootsteps()
        {
            if (footstepClips == null || footstepClips.Length == 0) return;

            // Solo reproducir pisadas si nos movemos usando el input
            if (moveInput.magnitude > 0.1f)
            {
                if (footstepAudioSource != null)
                {
                    // Asegurar loop
                    footstepAudioSource.loop = true;
                    
                    if (!footstepAudioSource.isPlaying)
                    {
                        AudioClip clip = footstepClips[0];
                        if (clip != null)
                        {
                            footstepAudioSource.clip = clip;
                            // Configurar volumen y pitch
                            footstepAudioSource.volume = isSprinting ? 0.9f : 0.6f;
                            footstepAudioSource.pitch = isSprinting ? 1.25f : 1.0f; // Acelerar los pasos al correr
                            footstepAudioSource.Play();
                        }
                    }
                    else
                    {
                        // Modular volumen y pitch en tiempo real si el estado cambia mientras camina
                        footstepAudioSource.volume = isSprinting ? 0.9f : 0.6f;
                        footstepAudioSource.pitch = isSprinting ? 1.25f : 1.0f;
                    }
                }
            }
            else
            {
                // Detener la caminata en el instante en que el jugador frena
                if (footstepAudioSource != null && footstepAudioSource.isPlaying)
                {
                    footstepAudioSource.Stop();
                }
            }
        }
    }
}
