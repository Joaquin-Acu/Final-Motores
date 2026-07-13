using UnityEngine;

namespace DungeonEscape
{
    [RequireComponent(typeof(Collider))]
    public class KeyCollectible : MonoBehaviour
    {
        [Header("Key Settings")]
        [SerializeField] private int keyCountValue = 1;
        [SerializeField] private float rotationSpeed = 100f;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float bobHeight = 0.2f;

        [Header("Visual Effects (Optional)")]
        [SerializeField] private GameObject collectVfx;

        private Vector3 startPosition;

        private void Start()
        {
            startPosition = transform.position;
            
            // Asegurarse de que el colisionador sea un trigger
            Collider col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void Update()
        {
            // Efecto visual retro: rotación y flotación
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
            
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
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
            // Notificar al GameManager / UI que se recolectó una llave
            DungeonEvents.OnKeyCollected?.Invoke(keyCountValue);

            // Generar efectos visuales
            if (collectVfx != null)
            {
                Instantiate(collectVfx, transform.position, Quaternion.identity);
            }

            // Destruir la llave recolectada
            Destroy(gameObject);
        }
    }
}
