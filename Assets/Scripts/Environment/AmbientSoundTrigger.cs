using UnityEngine;
using UnityEngine.Audio;

namespace DungeonEscape
{
    [RequireComponent(typeof(Collider))]
    public class AmbientSoundTrigger : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField] private AudioClip whisperClip; // El sonido .wav del susurro fantasmal
        [SerializeField] private bool playIn3D = false; // Si es true, suena en la posición del trigger. Si es false, suena directo en la cabeza del jugador (2D)

        private bool hasPlayed = false;

        private void Start()
        {
            // Asegurarse de que el colisionador esté configurado como Trigger
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasPlayed) return;

            // Detectar si el objeto que entra al trigger tiene la etiqueta del jugador
            if (other.CompareTag("Player"))
            {
                hasPlayed = true;

                if (whisperClip != null)
                {
                    if (playIn3D)
                    {
                        // Reproducir en 3D posicional en la posición física de este objeto
                        AudioSource.PlayClipAtPoint(whisperClip, transform.position);
                    }
                    else
                    {
                        // Reproducir en 2D estéreo global (ideal para voces/susurros que susurran en la mente del jugador)
                        if (AudioManager.Instance != null)
                        {
                            AudioManager.Instance.PlaySFX(whisperClip);
                        }
                    }
                }
            }
        }
    }
}
