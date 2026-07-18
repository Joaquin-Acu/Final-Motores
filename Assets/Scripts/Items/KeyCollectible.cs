using UnityEngine;

namespace DungeonEscape
{
    [RequireComponent(typeof(Collider))]
    public class KeyCollectible : MonoBehaviour
    {
        [Header("Key Settings")]
        [SerializeField] private int keyCountValue = 1;

        private Vector3 startPosition;

        private void Start()
        {
            startPosition = transform.position;
            
            // Asegurarse de que el colisionador sea un trigger
            Collider col = GetComponent<Collider>();
            col.isTrigger = true;
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Collect();
            }
        }

        private void Collect()
        {
            
            DungeonEvents.OnKeyCollected?.Invoke(keyCountValue);           

            
            Destroy(gameObject);
        }
    }
}
