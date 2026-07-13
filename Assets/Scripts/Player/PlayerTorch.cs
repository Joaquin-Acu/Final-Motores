using UnityEngine;

namespace DungeonEscape
{
    public class PlayerTorch : MonoBehaviour
    {
        [Header("Torch Settings")]
        [SerializeField] private Light torchLight;
        [SerializeField] private float baseIntensity = 1.0f;
        [SerializeField] private float flickerSpeed = 5f;
        [SerializeField] private float flickerAmount = 0.1f;
        
        [Header("Sway/Follow Settings")]
        [SerializeField] private float swayAmount = 0.05f;
        [SerializeField] private float maxSwayAmount = 0.1f;
        [SerializeField] private float swaySmoothness = 3f;

        private Vector3 defaultLocalPosition;

        private void Start()
        {
            if (torchLight == null)
            {
                torchLight = GetComponentInChildren<Light>();
            }

            if (torchLight != null)
            {
                baseIntensity = torchLight.intensity;
            }

            defaultLocalPosition = transform.localPosition;
        }

        private void Update()
        {
            // 1. Efecto de parpadeo (Flicker)
            if (torchLight != null)
            {
                float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 10f); // Usamos offset de 10 para diferenciar de otras antorchas
                torchLight.intensity = baseIntensity + (noise - 0.5f) * 2f * flickerAmount;
            }

            // 2. Efecto de oscilación suave al mirar (Sway)
            float mouseX = 0f;
            float mouseY = 0f;

            if (UnityEngine.InputSystem.Mouse.current != null)
            {
                Vector2 mouseDelta = UnityEngine.InputSystem.Mouse.current.delta.ReadValue();
                mouseX = mouseDelta.x * swayAmount;
                mouseY = mouseDelta.y * swayAmount;
            }

            mouseX = Mathf.Clamp(mouseX, -maxSwayAmount, maxSwayAmount);
            mouseY = Mathf.Clamp(mouseY, -maxSwayAmount, maxSwayAmount);

            Vector3 targetPosition = new Vector3(
                defaultLocalPosition.x - mouseX,
                defaultLocalPosition.y - mouseY,
                defaultLocalPosition.z
            );

            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * swaySmoothness);
        }
    }
}
