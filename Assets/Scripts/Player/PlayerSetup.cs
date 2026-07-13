using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.InputSystem;
#endif

namespace DungeonEscape
{
    public class PlayerSetup : MonoBehaviour
    {
        [Header("Setup Options")]
        [SerializeField] private string cameraName = "FirstPersonCamera";

        #if UNITY_EDITOR
        [ContextMenu("Setup First Person Player")]
        public void SetupFirstPersonPlayer()
        {
            Undo.RegisterCompleteObjectUndo(gameObject, "Setup First Person Player");

            // 1. Configurar PlayerInput
            PlayerInput playerInput = GetComponent<PlayerInput>();
            if (playerInput == null) playerInput = gameObject.AddComponent<PlayerInput>();

            // Cargar Input Actions Asset
            InputActionAsset inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            if (inputAsset != null)
            {
                playerInput.actions = inputAsset;
                playerInput.defaultActionMap = "Player";
            }
            else
            {
                Debug.LogWarning("No se encontró el archivo 'Assets/InputSystem_Actions.inputactions'. Por favor, asígnalo manualmente en el componente PlayerInput.");
            }

            // 2. Configurar Rigidbody
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            // 3. Configurar CapsuleCollider
            CapsuleCollider col = GetComponent<CapsuleCollider>();
            if (col == null) col = gameObject.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 1f, 0f);
            col.height = 2f;
            col.radius = 0.5f;

            // Crear y asignar material físico sin fricción para evitar tirones/fricción con paredes
            PhysicMaterial frictionlessMat = new PhysicMaterial("PlayerFrictionless");
            frictionlessMat.dynamicFriction = 0f;
            frictionlessMat.staticFriction = 0f;
            frictionlessMat.frictionCombine = PhysicMaterialCombine.Minimum;
            col.material = frictionlessMat;

            // 4. Crear GroundCheck
            Transform groundCheck = transform.Find("GroundCheck");
            if (groundCheck == null)
            {
                GameObject gcGo = new GameObject("GroundCheck");
                gcGo.transform.SetParent(transform);
                gcGo.transform.localPosition = new Vector3(0f, 0f, 0f);
                groundCheck = gcGo.transform;
            }

            // 5. Crear Head (Camera Parent)
            Transform head = transform.Find("Head");
            if (head == null)
            {
                GameObject headGo = new GameObject("Head");
                headGo.transform.SetParent(transform);
                headGo.transform.localPosition = new Vector3(0f, 1.6f, 0f);
                head = headGo.transform;
            }

            // 6. Configurar Cámara
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                mainCam = FindFirstObjectByType<Camera>();
            }

            if (mainCam != null)
            {
                Undo.RegisterCompleteObjectUndo(mainCam.gameObject, "Reparent Main Camera");
                mainCam.transform.SetParent(head);
                mainCam.transform.localPosition = Vector3.zero;
                mainCam.transform.localRotation = Quaternion.identity;
                mainCam.name = cameraName;
            }
            else
            {
                GameObject camGo = new GameObject(cameraName);
                camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
                camGo.transform.SetParent(head);
                camGo.transform.localPosition = Vector3.zero;
                camGo.transform.localRotation = Quaternion.identity;
                mainCam = camGo.GetComponent<Camera>();
                mainCam.tag = "MainCamera";
            }

            // 7. Configurar PlayerController
            PlayerController controller = GetComponent<PlayerController>();
            if (controller == null) controller = gameObject.AddComponent<PlayerController>();

            // Asignar GroundCheck
            var controllerSerialized = new SerializedObject(controller);
            controllerSerialized.FindProperty("groundCheck").objectReferenceValue = groundCheck;
            controllerSerialized.ApplyModifiedProperties();

            // 8. Configurar MouseLook
            MouseLook look = GetComponent<MouseLook>();
            if (look == null) look = gameObject.AddComponent<MouseLook>();

            var lookSerialized = new SerializedObject(look);
            lookSerialized.FindProperty("playerCamera").objectReferenceValue = mainCam.transform;
            lookSerialized.ApplyModifiedProperties();

            // 9. Configurar PlayerInteraction
            PlayerInteraction interaction = GetComponent<PlayerInteraction>();
            if (interaction == null) interaction = gameObject.AddComponent<PlayerInteraction>();

            var interactionSerialized = new SerializedObject(interaction);
            interactionSerialized.FindProperty("cameraTransform").objectReferenceValue = mainCam.transform;
            interactionSerialized.FindProperty("interactableMask").intValue = LayerMask.GetMask("Interactable", "Default"); // Capas de interacción
            interactionSerialized.ApplyModifiedProperties();

            // 10. Configurar PlayerHealth
            PlayerHealth health = GetComponent<PlayerHealth>();
            if (health == null) health = gameObject.AddComponent<PlayerHealth>();

            Debug.Log("¡Configuración del jugador en primera persona completada con éxito!");
        }
        #endif
    }
}
