using UnityEngine;

namespace DungeonEscape
{
    [RequireComponent(typeof(Collider))]
    public class ExitPortal : MonoBehaviour
    {
        [Header("Portal Settings")]
        [SerializeField] private GameObject activeVfx;

        private void Start()
        {
            Collider col = GetComponent<Collider>();
            col.isTrigger = true;

            if (activeVfx != null)
            {
                activeVfx.SetActive(true);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                TriggerVictory();
            }
        }

        private void TriggerVictory()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.WinGame();
            }
            else
            {
                DungeonEvents.OnGameStateChanged?.Invoke(GameState.Victory);
            }
        }
    }
}
