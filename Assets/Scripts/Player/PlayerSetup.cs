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

            PlayerInput playerInput = GetComponent<PlayerInput>();
            if (playerInput == null) playerInput = gameObject.AddComponent<PlayerInput>();

            InputActionAsset inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            if (inputAsset != null)
            {
                playerInput.actions = inputAsset;
                playerInput.defaultActionMap = "Player";
            }

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            CapsuleCollider col = GetComponent<CapsuleCollider>();
            if (col == null) col = gameObject.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 1f, 0f);
            col.height = 2f;
            col.radius = 0.5f;

            PhysicsMaterial frictionlessMat = new PhysicsMaterial("PlayerFrictionless");
            frictionlessMat.dynamicFriction = 0f;
            frictionlessMat.staticFriction = 0f;
            frictionlessMat.frictionCombine = PhysicsMaterialCombine.Minimum;
            col.material = frictionlessMat;

            Transform groundCheck = transform.Find("GroundCheck");
            if (groundCheck == null)
            {
                GameObject gcGo = new GameObject("GroundCheck");
                gcGo.transform.SetParent(transform);
                gcGo.transform.localPosition = new Vector3(0f, 0f, 0f);
                groundCheck = gcGo.transform;
            }

            Transform head = transform.Find("Head");
            if (head == null)
            {
                GameObject headGo = new GameObject("Head");
                headGo.transform.SetParent(transform);
                headGo.transform.localPosition = new Vector3(0f, 1.6f, 0f);
                head = headGo.transform;
            }

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

            PlayerController controller = GetComponent<PlayerController>();
            if (controller == null) controller = gameObject.AddComponent<PlayerController>();

            var controllerSerialized = new SerializedObject(controller);
            controllerSerialized.FindProperty("groundCheck").objectReferenceValue = groundCheck;
            controllerSerialized.ApplyModifiedProperties();

            MouseLook look = GetComponent<MouseLook>();
            if (look == null) look = gameObject.AddComponent<MouseLook>();

            var lookSerialized = new SerializedObject(look);
            lookSerialized.FindProperty("playerCamera").objectReferenceValue = mainCam.transform;
            lookSerialized.ApplyModifiedProperties();

            PlayerInteraction interaction = GetComponent<PlayerInteraction>();
            if (interaction == null) interaction = gameObject.AddComponent<PlayerInteraction>();

            var interactionSerialized = new SerializedObject(interaction);
            interactionSerialized.FindProperty("cameraTransform").objectReferenceValue = mainCam.transform;
            interactionSerialized.FindProperty("interactableMask").intValue = LayerMask.GetMask("Interactable", "Default");
            interactionSerialized.ApplyModifiedProperties();

            PlayerHealth health = GetComponent<PlayerHealth>();
            if (health == null) health = gameObject.AddComponent<PlayerHealth>();
        }
        #endif
    }
}
