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
        [SerializeField] private AudioClip[] footstepClips;
        [SerializeField] private AudioSource footstepAudioSource;
        [SerializeField] private AudioMixerGroup sfxGroup;

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

            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            if (footstepAudioSource == null)
            {
                footstepAudioSource = gameObject.AddComponent<AudioSource>();
                footstepAudioSource.playOnAwake = false;
                footstepAudioSource.loop = true;
            }

            footstepAudioSource.spatialBlend = 0f;

            if (sfxGroup != null)
            {
                footstepAudioSource.outputAudioMixerGroup = sfxGroup;
            }
        }

        private void Start()
        {
            if (playerInput != null && playerInput.actions != null)
            {
                moveAction = playerInput.actions.FindAction("Move");
                sprintAction = playerInput.actions.FindAction("Sprint");
            }
            else
            {
                Debug.LogError("Acciones de PlayerInput no configuradas.");
            }
        }

        private void Update()
        {
            if (moveAction != null) moveInput = moveAction.ReadValue<Vector2>();
            if (sprintAction != null) isSprinting = sprintAction.IsPressed();

            HandleFootsteps();
        }

        private void FixedUpdate()
        {
            MovePlayer();
        }

        private void MovePlayer()
        {
            Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
            moveDirection.Normalize();

            float currentSpeed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);
            Vector3 targetVelocity = moveDirection * currentSpeed;
            targetVelocity.y = rb.linearVelocity.y;

            rb.linearVelocity = targetVelocity;
        }

        private void HandleFootsteps()
        {
            if (footstepClips == null || footstepClips.Length == 0) return;

            if (moveInput.magnitude > 0.1f)
            {
                if (footstepAudioSource != null)
                {
                    footstepAudioSource.loop = true;

                    if (!footstepAudioSource.isPlaying)
                    {
                        AudioClip clip = footstepClips[0];
                        if (clip != null)
                        {
                            footstepAudioSource.clip = clip;
                            footstepAudioSource.volume = isSprinting ? 0.9f : 0.6f;
                            footstepAudioSource.pitch = isSprinting ? 1.25f : 1.0f;
                            footstepAudioSource.Play();
                        }
                    }
                    else
                    {
                        footstepAudioSource.volume = isSprinting ? 0.9f : 0.6f;
                        footstepAudioSource.pitch = isSprinting ? 1.25f : 1.0f;
                    }
                }
            }
            else
            {
                if (footstepAudioSource != null && footstepAudioSource.isPlaying)
                {
                    footstepAudioSource.Stop();
                }
            }
        }
    }
}
