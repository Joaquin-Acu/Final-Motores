using System.Collections;
using UnityEngine;

namespace DungeonEscape
{
    [RequireComponent(typeof(Collider))]
    public class DungeonDoor : MonoBehaviour, IInteractable
    {
        [Header("Door Settings")]
        [SerializeField] private int keysRequired = 1;
        [SerializeField] private float slideHeight = 3.5f;
        [SerializeField] private float openSpeed = 2f;

        [Header("Visual Meshes")]
        [SerializeField] private Transform doorMeshTransform; // El objeto de la puerta que subirá

        private bool isOpen = false;
        private Vector3 closedPosition;
        private Vector3 openPosition;

        private void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("Interactable");
            if (gameObject.layer == -1) gameObject.layer = 0;

            if (doorMeshTransform == null)
            {
                doorMeshTransform = transform; // Si no hay referencia asignada, mover todo el GameObject
            }

            closedPosition = doorMeshTransform.localPosition;
            openPosition = closedPosition + Vector3.up * slideHeight;
        }

        public void Interact()
        {
            if (isOpen) return;

            // Preguntar al GameManager si el jugador tiene suficientes llaves
            if (GameManager.Instance != null && GameManager.Instance.TryUseKeys(keysRequired))
            {
                OpenDoor();
            }
            else
            {
                Debug.Log($"No tienes suficientes llaves. Necesitas {keysRequired} llave(s).");
                // Podríamos disparar un evento de sonido o UI que muestre "Llaves insuficientes"
            }
        }

        public string GetInteractionPrompt()
        {
            if (isOpen) return string.Empty;
            return $"Presiona E para abrir la puerta (Requiere {keysRequired} llave/s)";
        }

        private void OpenDoor()
        {
            isOpen = true;
            DungeonEvents.OnDoorUnlocked?.Invoke();
            StartCoroutine(SlideOpenCoroutine());
        }

        private IEnumerator SlideOpenCoroutine()
        {
            float elapsed = 0f;
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime * openSpeed;
                doorMeshTransform.localPosition = Vector3.Lerp(closedPosition, openPosition, elapsed);
                yield return null;
            }
            doorMeshTransform.localPosition = openPosition;
            
            // Opcional: Desactivar colisiones para asegurar paso libre si no es parte del mesh que subió
            Collider col = GetComponent<Collider>();
            if (col != null && doorMeshTransform == transform)
            {
                col.enabled = false;
            }
        }
    }
}
