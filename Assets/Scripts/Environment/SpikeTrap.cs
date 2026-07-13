using System.Collections;
using UnityEngine;

namespace DungeonEscape
{
    public class SpikeTrap : MonoBehaviour
    {
        [Header("Trap Settings")]
        [SerializeField] private int damageAmount = 20;
        [SerializeField] private float activeDuration = 2f;   // Tiempo que los pinchos están fuera
        [SerializeField] private float inactiveDuration = 2f; // Tiempo que los pinchos están escondidos
        [SerializeField] private float transitionSpeed = 5f;  // Velocidad de movimiento visual

        [Header("Visual Meshes")]
        [SerializeField] private Transform spikesMesh; // Las puntas de los pinchos que suben/bajan
        [SerializeField] private float localRetractedY = -0.5f;
        [SerializeField] private float localExtendedY = 0.2f;

        private bool areSpikesActive = false;
        private bool isPlayerInside = false;
        private PlayerHealth cachedPlayerHealth;

        private void Start()
        {
            if (spikesMesh != null)
            {
                spikesMesh.localPosition = new Vector3(spikesMesh.localPosition.x, localRetractedY, spikesMesh.localPosition.z);
            }
            StartCoroutine(TrapCycleCoroutine());
        }

        private IEnumerator TrapCycleCoroutine()
        {
            while (true)
            {
                // 1. Estado Inactivo
                areSpikesActive = false;
                yield return StartCoroutine(MoveSpikes(localRetractedY));
                yield return new WaitForSeconds(inactiveDuration);

                // 2. Estado Activo (Peligro!)
                areSpikesActive = true;
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySpikeTrapSfx(transform.position);
                }
                yield return StartCoroutine(MoveSpikes(localExtendedY));
                
                // Si el jugador ya estaba parado encima cuando salieron, hacerle daño
                if (isPlayerInside && cachedPlayerHealth != null)
                {
                    cachedPlayerHealth.TakeDamage(damageAmount);
                }

                yield return new WaitForSeconds(activeDuration);
            }
        }

        private IEnumerator MoveSpikes(float targetY)
        {
            if (spikesMesh == null) yield break;

            Vector3 startPos = spikesMesh.localPosition;
            Vector3 targetPos = new Vector3(startPos.x, targetY, startPos.z);
            float elapsed = 0f;

            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime * transitionSpeed;
                spikesMesh.localPosition = Vector3.Lerp(startPos, targetPos, elapsed);
                yield return null;
            }
            spikesMesh.localPosition = targetPos;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerInside = true;
                cachedPlayerHealth = other.GetComponent<PlayerHealth>();

                // Si los pinchos ya están activos al entrar, hacer daño inmediatamente
                if (areSpikesActive && cachedPlayerHealth != null)
                {
                    cachedPlayerHealth.TakeDamage(damageAmount);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerInside = false;
                cachedPlayerHealth = null;
            }
        }
    }
}
