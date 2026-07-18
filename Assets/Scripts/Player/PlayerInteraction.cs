using UnityEngine;
using UnityEngine.InputSystem;

namespace DungeonEscape
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float interactionDistance = 3f;
        [SerializeField] private LayerMask interactableMask;

        private PlayerInput playerInput;
        private InputAction interactAction;
        private IInteractable currentInteractable;

        private void Start()
        {
            playerInput = GetComponent<PlayerInput>();
            if (playerInput != null && playerInput.actions != null)
            {
                interactAction = playerInput.actions.FindAction("Interact");
            }

            if (cameraTransform == null)
            {
                cameraTransform = GetComponentInChildren<Camera>()?.transform;
                if (cameraTransform == null)
                {
                    cameraTransform = Camera.main?.transform;
                }
            }

            int interactableLayer = LayerMask.NameToLayer("Interactable");
            if (interactableLayer != -1)
            {
                interactableMask |= (1 << interactableLayer);
            }
            interactableMask |= (1 << LayerMask.NameToLayer("Default"));
        }

        private void Update()
        {
            CheckForInteractable();
            
            //Detectar si se presiona la tecla de interactuar
            if (interactAction != null && interactAction.WasPressedThisFrame())
            {
                TriggerInteraction();
            }
        }

        private void CheckForInteractable()
        {
            if (cameraTransform == null) return;

            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
            RaycastHit hit;

            Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.green);

            if (Physics.Raycast(ray, out hit, interactionDistance, interactableMask))
            {
                IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
                
                if (interactable != null)
                {
                    if (interactable != currentInteractable)
                    {
                        currentInteractable = interactable;
                        DungeonEvents.OnInteractLook?.Invoke(true, currentInteractable.GetInteractionPrompt());
                    }
                    return;
                }
            }

            if (currentInteractable != null)
            {
                currentInteractable = null;
                DungeonEvents.OnInteractLook?.Invoke(false, string.Empty);
            }
        }

        private void TriggerInteraction()
        {
            if (currentInteractable != null)
            {
                currentInteractable.Interact();
                
                CheckForInteractable();
            }
        }
    }
}
