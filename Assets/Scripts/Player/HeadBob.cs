using UnityEngine;

namespace DungeonEscape
{
    public class HeadBob : MonoBehaviour
    {
        [Header("Bob Settings")]
        [SerializeField] private float bobFrequency = 5f; // Frecuencia de la oscilación
        [SerializeField] private float bobHorizontalAmount = 0.05f; // Oscilación horizontal (eje X)
        [SerializeField] private float bobVerticalAmount = 0.05f;   // Oscilación vertical (eje Y)
        [SerializeField] private float smoothRecoverySpeed = 10f;  // Retorno al centro al detenerse

        [Header("References")]
        [SerializeField] private Rigidbody playerRigidbody;

        private float timer = 0f;
        private Vector3 defaultLocalPosition;

        private void Start()
        {
            defaultLocalPosition = transform.localPosition;
            
            // Si no está asignado, intentar encontrar el Rigidbody en el transform padre
            if (playerRigidbody == null)
            {
                playerRigidbody = GetComponentInParent<Rigidbody>();
            }
        }

        private void Update()
        {
            if (playerRigidbody == null) return;

            // Calcular la velocidad horizontal despreciando la caída/gravedad (eje Y)
            Vector3 horizontalVelocity = new Vector3(playerRigidbody.linearVelocity.x, 0f, playerRigidbody.linearVelocity.z);
            float speed = horizontalVelocity.magnitude;

            if (speed > 0.1f)
            {
                // El jugador se mueve: Calcular balanceo
                timer += Time.deltaTime * bobFrequency * speed;

                float bobX = Mathf.Cos(timer / 2f) * bobHorizontalAmount;
                float bobY = Mathf.Sin(timer) * bobVerticalAmount;

                transform.localPosition = new Vector3(
                    defaultLocalPosition.x + bobX,
                    defaultLocalPosition.y + bobY,
                    defaultLocalPosition.z
                );
            }
            else
            {
                // El jugador está quieto: Recuperar posición inicial suavemente
                timer = 0f;
                transform.localPosition = Vector3.Lerp(transform.localPosition, defaultLocalPosition, Time.deltaTime * smoothRecoverySpeed);
            }
        }
    }
}
