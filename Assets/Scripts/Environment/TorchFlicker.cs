using UnityEngine;
using System.Collections;

namespace DungeonEscape
{
    // Script de parpadeo simple para la simulación de fuego de antorcha
    [RequireComponent(typeof(Light))]
    public class TorchFlicker : MonoBehaviour
    {
        private Light torchLight;
        private float baseIntensity;
        [SerializeField] private float flickerSpeed = 8f;
        [SerializeField] private float flickerAmount = 0.2f;

        private void Start()
        {
            torchLight = GetComponent<Light>();
            if (torchLight != null)
            {
                baseIntensity = torchLight.intensity;
                StartCoroutine(FlickerCoroutine());
            }
            else
            {
                enabled = false;
            }
        }

        private IEnumerator FlickerCoroutine()
        {
            // Desincronizar el parpadeo inicial para que no oscilen al mismo tiempo
            yield return new WaitForSeconds(Random.Range(0f, 1f));

            WaitForSeconds delay = new WaitForSeconds(0.05f); // 20 actualizaciones por segundo bastan y ahorran CPU
            while (true)
            {
                // Variar la intensidad usando ruido de Perlin para un parpadeo natural
                float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
                torchLight.intensity = baseIntensity + (noise - 0.5f) * 2f * flickerAmount;
                yield return delay;
            }
        }
    }
}
