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
            gameObject.layer = LayerMask.NameToLayer("Interactable");
        }

        public void Interact()
        {
            if (isOpen) return;

            isOpen = true;
            DungeonEvents.OnChestOpened?.Invoke();
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
                
                Vector3 popDirection = Vector3.up + transform.forward * 0.5f;
                keyRb.AddForce(popDirection.normalized * keyPopForce, ForceMode.Impulse);
            }
            else
            {
                DungeonEvents.OnKeyCollected?.Invoke(1);
            }
        }
    }
}
