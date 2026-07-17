using UnityEngine;

namespace DungeonEscape
{
    [RequireComponent(typeof(Collider))]
    public class DungeonChest : MonoBehaviour, IInteractable
    {
        [Header("Chest Settings")]
        [SerializeField] private GameObject keyPrefabToSpawn;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float keyPopForce = 4f;

        private bool isOpen = false;

        private void Start()
        {
            // Asegurarse de que tenga colisión
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                // El cofre debe ser un objeto físico para bloquear el paso, pero tener una capa interactuable
                gameObject.layer = LayerMask.NameToLayer("Interactable");
            }
        }

        public void Interact()
        {
            if (isOpen) return;

            isOpen = true;
            SpawnKey();
        }

        public string GetInteractionPrompt()
        {
            return isOpen ? string.Empty : "Presiona E para abrir el cofre";
        }

        private void SpawnKey()
        {
            if (keyPrefabToSpawn != null && spawnPoint != null)
            {
                GameObject spawnedKey = Instantiate(keyPrefabToSpawn, spawnPoint.position, Quaternion.identity);
                Rigidbody keyRb = spawnedKey.GetComponent<Rigidbody>();
                if (keyRb == null)
                {
                    keyRb = spawnedKey.AddComponent<Rigidbody>();
                }
                
                // Aplicar una fuerza hacia arriba y adelante para que "salte" del cofre
                Vector3 popDirection = Vector3.up + transform.forward * 0.5f;
                keyRb.AddForce(popDirection.normalized * keyPopForce, ForceMode.Impulse);
            }
            else
            {
                // Fallback: Si no hay prefab asignado, le sumamos directamente una llave al jugador
                DungeonEvents.OnKeyCollected?.Invoke(1);
                Debug.Log("Cofre abierto: Llave añadida directamente al inventario (Prefab no asignado).");
            }
        }
    }
}
