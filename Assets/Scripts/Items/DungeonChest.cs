using System.Collections;
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

        [Header("Animation Settings")]
        [SerializeField] private Transform chestLid; // Tapa del cofre a rotar
        [SerializeField] private float openAngle = -80f;
        [SerializeField] private float openSpeed = 2f;

        private bool isOpen = false;

        private void Start()
        {
            // Asegurarse de que tenga colisión
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                // El cofre debe ser un objeto físico para bloquear el paso, pero tener una capa interactuable
                gameObject.layer = LayerMask.NameToLayer("Interactable");
                if (gameObject.layer == -1)
                {
                    gameObject.layer = 0; // Fallback a Default si no existe la capa
                }
            }
        }

        public void Interact()
        {
            if (isOpen) return;

            isOpen = true;
            StartCoroutine(OpenChestCoroutine());
        }

        public string GetInteractionPrompt()
        {
            return isOpen ? string.Empty : "Presiona E para abrir el cofre";
        }

        private IEnumerator OpenChestCoroutine()
        {
            // Rotar la tapa gradualmente
            if (chestLid != null)
            {
                Quaternion startRotation = chestLid.localRotation;
                Quaternion targetRotation = Quaternion.Euler(openAngle, 0f, 0f);
                float elapsed = 0f;

                while (elapsed < 1f)
                {
                    elapsed += Time.deltaTime * openSpeed;
                    chestLid.localRotation = Quaternion.Slerp(startRotation, targetRotation, elapsed);
                    yield return null;
                }
                chestLid.localRotation = targetRotation;
            }

            // Esperar un instante y hacer aparecer la llave (Zelda style!)
            yield return new WaitForSeconds(0.2f);
            SpawnKey();
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
