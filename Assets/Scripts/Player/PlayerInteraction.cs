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
            else
            {
                Debug.LogWarning("No se encontró PlayerInput en PlayerInteraction.");
            }
        }

        private void Update()
        {
            CheckForInteractable();
            
            // Detectar si se presiona la tecla de interactuar
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

            if (Physics.Raycast(ray, out hit, interactionDistance, interactableMask))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                
                if (interactable != null)
                {
                    // Si encontramos un interactuable nuevo o diferente
                    if (interactable != currentInteractable)
                    {
                        currentInteractable = interactable;
                        DungeonEvents.OnInteractLook?.Invoke(true, currentInteractable.GetInteractionPrompt());
                    }
                    return;
                }
            }

            // Si el Raycast no golpea nada o golpea algo no interactuable
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
                
                // Forzar actualización inmediata tras interactuar por si el estado cambia
                CheckForInteractable();
            }
        }
    }
}
