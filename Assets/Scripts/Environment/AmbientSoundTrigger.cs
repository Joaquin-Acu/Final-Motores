using UnityEngine;

namespace DungeonEscape
{
    [RequireComponent(typeof(Collider))]
    public class AmbientSoundTrigger : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField] private AudioClip whisperClip;
        [SerializeField] private bool playIn3D = false;

        private bool hasPlayed = false;

        private void Start()
        {
            Collider col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasPlayed) return;

            if (other.CompareTag("Player"))
            {
                hasPlayed = true;

                if (whisperClip != null)
                {
                    if (playIn3D)
                    {
                        AudioSource.PlayClipAtPoint(whisperClip, transform.position);
                    }
                    else if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySFX(whisperClip);
                    }
                }
            }
        }
    }
}
