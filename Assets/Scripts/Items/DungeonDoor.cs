using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace DungeonEscape
{
    [RequireComponent(typeof(Collider))]
    public class DungeonDoor : MonoBehaviour, IInteractable
    {
        [Header("Door Settings")]
        [SerializeField] private int keysRequired = 1;
        [SerializeField] private float openAngle = -90f;
        [SerializeField] private float openSpeed = 2f;

        [Header("Visual Meshes")]
        [SerializeField] private Transform doorMeshTransform;

        private bool isOpen = false;
        private Quaternion closedRotation;
        private Quaternion openRotation;

        private void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("Interactable");

            if (doorMeshTransform == null)
            {
                doorMeshTransform = transform;
            }

            closedRotation = doorMeshTransform.localRotation;
            openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);
        }

        public void Interact()
        {
            if (isOpen) return;

            if (GameManager.Instance != null && GameManager.Instance.TryUseKeys(keysRequired))
            {
                OpenDoor();
            }
            else
            {
                Debug.Log($"Llaves insuficientes. Requeridas: {keysRequired}");
            }
        }

        public string GetInteractionPrompt()
        {
            if (isOpen) return string.Empty;
            return $"Presiona E para abrir (Requiere {keysRequired} llave/s)";
        }

        private void OpenDoor()
        {
            isOpen = true;
            DungeonEvents.OnDoorUnlocked?.Invoke();

            // Desactivar obstáculo NavMesh al abrir
            NavMeshObstacle obstacle = GetComponent<NavMeshObstacle>();
            if (obstacle != null)
            {
                obstacle.enabled = false;
            }

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

            Collider col = GetComponent<Collider>();
            if (col != null && doorMeshTransform == transform)
            {
                col.enabled = false;
            }
        }
    }
}
