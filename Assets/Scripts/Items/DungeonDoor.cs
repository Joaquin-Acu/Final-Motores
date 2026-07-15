using System.Collections;
using UnityEngine;

namespace DungeonEscape
{
    [RequireComponent(typeof(Collider))]
    public class DungeonDoor : MonoBehaviour, IInteractable
    {
        [Header("Door Settings")]
        [SerializeField] private int keysRequired = 1;
        [SerializeField] private float openAngle = -90f;     // Ángulo de apertura en el eje Y (ej: 90 o -90)
        [SerializeField] private float openSpeed = 2f;

        [Header("Visual Meshes")]
        [SerializeField] private Transform doorMeshTransform; // El objeto de la hoja de la puerta que rotará

        private bool isOpen = false;
        private Quaternion closedRotation;
        private Quaternion openRotation;

        private void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("Interactable");
            if (gameObject.layer == -1) gameObject.layer = 0;

            if (doorMeshTransform == null)
            {
                doorMeshTransform = transform; // Si no hay referencia, rotar todo el objeto
            }

            closedRotation = doorMeshTransform.localRotation;
            openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);
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
                Debug.Log($"No tienes suficientes llaves. Necesitas {keysRequired} llave/s.");
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
            StartCoroutine(SwingOpenCoroutine());
        }

        private IEnumerator SwingOpenCoroutine()
        {
            float elapsed = 0f;
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime * openSpeed;
                doorMeshTransform.localRotation = Quaternion.Slerp(closedRotation, openRotation, elapsed);
                yield return null;
            }
            doorMeshTransform.localRotation = openRotation;

            // Si rotamos todo el GameObject, apagamos el colisionador para poder pasar libremente.
            // Si el colisionador está en un hijo que rotó con la puerta, el paso ya queda libre físicamente.
            Collider col = GetComponent<Collider>();
            if (col != null && doorMeshTransform == transform)
            {
                col.enabled = false;
            }
        }
    }
}
