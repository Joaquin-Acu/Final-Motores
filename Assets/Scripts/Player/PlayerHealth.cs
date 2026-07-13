using System.Collections;
using UnityEngine;

namespace DungeonEscape
{
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private float invulnerabilityDuration = 1f;

        [Header("Visual Feedback (Coroutines)")]
        [SerializeField] private Renderer playerRenderer; // Opcional, si queremos hacer parpadear un objeto (en 1ª persona puede ser el arma o los brazos, o podemos aplicar destello rojo en UI)
        [SerializeField] private float flashInterval = 0.1f;

        private int currentHealth;
        private bool isInvulnerable = false;

        private void Start()
        {
            currentHealth = maxHealth;
            // Retrasamos levemente la inicialización para asegurar que los componentes de UI se hayan registrado
            StartCoroutine(InitHealthEvent());
        }

        private IEnumerator InitHealthEvent()
        {
            yield return new WaitForEndOfFrame();
            DungeonEvents.OnPlayerMaxHealthInit?.Invoke(maxHealth);
            DungeonEvents.OnPlayerDamage?.Invoke(currentHealth);
        }

        public void TakeDamage(int damage)
        {
            if (isInvulnerable || currentHealth <= 0) return;

            currentHealth -= damage;
            currentHealth = Mathf.Max(currentHealth, 0);

            DungeonEvents.OnPlayerDamage?.Invoke(currentHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                StartCoroutine(InvulnerabilityCoroutine());
            }
        }

        public void Heal(int amount)
        {
            if (currentHealth <= 0) return;

            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);

            DungeonEvents.OnPlayerHeal?.Invoke(currentHealth);
        }

        private IEnumerator InvulnerabilityCoroutine()
        {
            isInvulnerable = true;
            
            // Efecto visual de parpadeo (si hay un render asignado)
            float elapsed = 0f;
            bool isVisible = true;

            while (elapsed < invulnerabilityDuration)
            {
                if (playerRenderer != null)
                {
                    isVisible = !isVisible;
                    playerRenderer.enabled = isVisible;
                }
                
                yield return new WaitForSeconds(flashInterval);
                elapsed += flashInterval;
            }

            if (playerRenderer != null)
            {
                playerRenderer.enabled = true;
            }

            isInvulnerable = false;
        }

        private void Die()
        {
            // Detener el juego e ir a pantalla de derrota
            DungeonEvents.OnGameStateChanged?.Invoke(GameState.GameOver);
            gameObject.SetActive(false); // Desactivar el jugador
        }
    }
}
