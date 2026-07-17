using System.Collections;
using UnityEngine;

namespace DungeonEscape
{
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private float invulnerabilityDuration = 1f;

        [Header("Visual Feedback")]
        [SerializeField] private Renderer playerRenderer;
        [SerializeField] private float flashInterval = 0.1f;

        private int currentHealth;
        private bool isInvulnerable = false;

        private void Start()
        {
            currentHealth = maxHealth;
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

            if (playerRenderer != null) playerRenderer.enabled = true;
            isInvulnerable = false;
        }

        private void Die()
        {
            DungeonEvents.OnGameStateChanged?.Invoke(GameState.GameOver);

            // Desactivar controles
            PlayerController pc = GetComponent<PlayerController>();
            if (pc != null) pc.enabled = false;

            PlayerInteraction pi = GetComponent<PlayerInteraction>();
            if (pi != null) pi.enabled = false;

            MouseLook ml = GetComponentInChildren<MouseLook>();
            if (ml != null) ml.enabled = false;

            // Desactivar colisiones y detener fisicas
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
        }
    }
}
