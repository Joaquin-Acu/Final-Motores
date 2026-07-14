using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using Unity.AI.Navigation;

namespace DungeonEscape
{
    public class DungeonGenerator : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private float cellSize = 3f;
        [SerializeField] [TextArea(8, 20)] private string mapLayout = 
            "W W W W W W W W W W W W\n" +
            "W P . . . W . . . . K W\n" +
            "W . W W . W . W W W . W\n" +
            "W . . W . . . W . . . W\n" +
            "W W . W W W D W . W W W\n" +
            "W . . . . . . . . W . W\n" +
            "W . W W W W W W . W . W\n" +
            "W E W . . . . W . . . W\n" +
            "W . W . W W . W W W . W\n" +
            "W . . . W X . . . . C W\n" +
            "W W W W W W W W W W W W";

        [Header("Custom Prefabs (Optional)")]
        [SerializeField] private GameObject wallPrefab;
        [SerializeField] private GameObject floorPrefab;
        [SerializeField] private GameObject ceilingPrefab;
        [SerializeField] private GameObject keyPrefab;
        [SerializeField] private GameObject doorPrefab;
        [SerializeField] private GameObject chestPrefab;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject trapPrefab;
        [SerializeField] private GameObject portalPrefab;
        [SerializeField] private GameObject torchPrefab; // Prefab de antorcha (con modelo 3D y partículas de fuego)

        [Header("Lighting Settings")]
        [SerializeField] private bool generateTorches = true;
        [SerializeField] private float torchHeight = 2.0f;
        [SerializeField] private Color torchColor = new Color(1.0f, 0.6f, 0.2f); // Luz cálida de fuego

        // Materiales por defecto para fallback (prototipado rápido)
        private Material wallMaterial;
        private Material floorMaterial;
        private Material ceilingMaterial;

        [Header("Editor & Generation Options")]
        [SerializeField] private bool generateOnPlay = true; // Si es falso, no se borrará ni regenerará al dar Play (permite diseñar a mano en el Editor)

        private GameObject playerInstance;
        private List<GameObject> generatedObjects = new List<GameObject>();
        private NavMeshSurface navMeshSurface;

        private void Awake()
        {
            if (generateOnPlay)
            {
                CreateFallbackMaterials();
                GenerateDungeon();
            }
            SetupNavMesh(); // Sigue horneando el NavMesh al iniciar para adaptarse a cualquier cambio manual
        }

        private void CreateFallbackMaterials()
        {
            // Crear materiales básicos si no hay prefabs asignados
            wallMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            wallMaterial.color = new Color(0.15f, 0.15f, 0.15f); // Gris oscuro piedra

            floorMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            floorMaterial.color = new Color(0.25f, 0.2f, 0.15f); // Marrón tierra

            ceilingMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            ceilingMaterial.color = new Color(0.1f, 0.1f, 0.1f); // Techo muy oscuro
        }

        private void ConfigureDungeonLighting()
        {
            // 1. Apagar o atenuar al mínimo la luz direccional de la escena (el sol)
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light l in lights)
            {
                if (l.type == LightType.Directional)
                {
                    l.intensity = 0.02f; // Casi apagada para simular oscuridad de mazmorra
                    l.color = new Color(0.1f, 0.15f, 0.25f); // Tono azulado frío de penumbra
                }
            }

            // 2. Cambiar la iluminación ambiental de la escena a color plano y ponerla casi en negro
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientSkyColor = new Color(0.01f, 0.01f, 0.015f); // Prácticamente negro absoluto
            
            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(gameObject);
            }
            #endif
        }

        [ContextMenu("Generate Dungeon")]
        public void GenerateDungeon()
        {
            // Asegurar que los materiales estén creados y la iluminación esté configurada si se genera en el Editor
            CreateFallbackMaterials();
            ConfigureDungeonLighting();

            // Limpiar si ya hay objetos generados (por si se ejecuta en Editor)
            ClearDungeon();

            // Encontrar al jugador en la escena si ya existe
            playerInstance = GameObject.FindWithTag("Player");

            string[] lines = mapLayout.Split('\n');
            int rows = lines.Length;

            for (int r = 0; r < rows; r++)
            {
                string[] cells = lines[r].Trim().Split(' ');
                int cols = cells.Length;

                for (int c = 0; c < cols; c++)
                {
                    string cellType = cells[c];
                    Vector3 position = new Vector3(c * cellSize, 0f, -r * cellSize);

                    // Siempre generar suelo y techo para pasillos y salas
                    if (cellType != " ")
                    {
                        SpawnFloorAndCeiling(position);
                    }

                    switch (cellType)
                    {
                        case "W": // Pared
                            SpawnWall(position);
                            break;
                        case "P": // Jugador
                            if (playerInstance != null)
                            {
                                Vector3 spawnTarget = position + Vector3.up * 0.5f;
                                playerInstance.transform.position = spawnTarget;
                                
                                Rigidbody playerRb = playerInstance.GetComponent<Rigidbody>();
                                if (playerRb != null)
                                {
                                    playerRb.position = spawnTarget;
                                    playerRb.linearVelocity = Vector3.zero;
                                    playerRb.angularVelocity = Vector3.zero;
                                }
                                Physics.SyncTransforms(); // Forzar sincronización del motor físico inmediatamente
                            }
                            else
                            {
                                Debug.LogWarning("No se encontró un GameObject con el Tag 'Player' en la escena. Colócalo antes de jugar.");
                            }
                            break;
                        case "K": // Llave
                            GameObject keyObj = SpawnObject(keyPrefab, position + Vector3.up * 0.5f, "KeyCollectible", Color.yellow, PrimitiveType.Cylinder);
                            if (keyPrefab == null && keyObj != null)
                            {
                                keyObj.AddComponent<KeyCollectible>();
                            }
                            break;
                        case "D": // Puerta
                            GameObject doorObj = SpawnObject(doorPrefab, position, "DungeonDoor", Color.cyan, PrimitiveType.Cube, new Vector3(cellSize, cellSize, 0.3f));
                            if (doorPrefab == null && doorObj != null)
                            {
                                doorObj.AddComponent<DungeonDoor>();
                            }
                            break;
                        case "C": // Cofre
                            GameObject chestObj = SpawnObject(chestPrefab, position, "DungeonChest", Color.magenta, PrimitiveType.Cube, new Vector3(0.8f * cellSize, 0.6f * cellSize, 0.5f * cellSize));
                            if (chestPrefab == null && chestObj != null)
                            {
                                chestObj.AddComponent<DungeonChest>();
                            }
                            break;
                        case "E": // Enemigo
                            GameObject enemyObj = SpawnObject(enemyPrefab, position + Vector3.up * 0.5f, "Enemy", Color.red, PrimitiveType.Capsule);
                            if (enemyPrefab == null && enemyObj != null)
                            {
                                enemyObj.AddComponent<NavMeshAgent>();
                                enemyObj.AddComponent<EnemyAI>();
                            }
                            break;
                        case "S": // Trampa
                            GameObject trapObj = SpawnObject(trapPrefab, position, "SpikeTrap", Color.grey, PrimitiveType.Cube, new Vector3(cellSize * 0.8f, 0.1f, cellSize * 0.8f));
                            if (trapPrefab == null && trapObj != null)
                            {
                                trapObj.AddComponent<SpikeTrap>();
                            }
                            break;
                        case "X": // Portal
                            GameObject portalObj = SpawnObject(portalPrefab, position, "ExitPortal", Color.green, PrimitiveType.Cylinder, new Vector3(cellSize * 0.8f, 0.1f, cellSize * 0.8f));
                            if (portalPrefab == null && portalObj != null)
                            {
                                portalObj.AddComponent<ExitPortal>();
                            }
                            break;
                    }
                }
            }
        }

        private void SpawnFloorAndCeiling(Vector3 position)
        {
            // Suelo
            GameObject floor;
            if (floorPrefab != null)
            {
                floor = Instantiate(floorPrefab, position, Quaternion.identity, transform);
            }
            else
            {
                floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
                floor.transform.position = position;
                floor.transform.localScale = new Vector3(cellSize / 10f, 1f, cellSize / 10f); // Plane es 10x10 unidades por defecto
                floor.GetComponent<Renderer>().material = floorMaterial;
                floor.transform.SetParent(transform);
            }
            generatedObjects.Add(floor);

            // Techo
            GameObject ceiling;
            if (ceilingPrefab != null)
            {
                ceiling = Instantiate(ceilingPrefab, position + Vector3.up * cellSize, Quaternion.Euler(180, 0, 0), transform);
            }
            else
            {
                ceiling = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ceiling.transform.position = position + Vector3.up * cellSize;
                ceiling.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
                ceiling.transform.localScale = new Vector3(cellSize / 10f, 1f, cellSize / 10f);
                ceiling.GetComponent<Renderer>().material = ceilingMaterial;
                ceiling.transform.SetParent(transform);
            }
            generatedObjects.Add(ceiling);
        }

        private void SpawnWall(Vector3 position)
        {
            GameObject wall;
            if (wallPrefab != null)
            {
                wall = Instantiate(wallPrefab, position + Vector3.up * (cellSize / 2f), Quaternion.identity, transform);
            }
            else
            {
                wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.transform.position = position + Vector3.up * (cellSize / 2f);
                wall.transform.localScale = new Vector3(cellSize, cellSize, cellSize);
                wall.GetComponent<Renderer>().material = wallMaterial;
                wall.transform.SetParent(transform);
            }
            // En Unity 6 / URP, las paredes deben marcarse como estáticas para optimizar
            wall.isStatic = true;
            generatedObjects.Add(wall);

            // Generar antorchas/luces dinámicas en algunas paredes
            if (generateTorches && Random.value < 0.25f)
            {
                SpawnTorch(position + Vector3.up * torchHeight);
            }
        }

        private void SpawnTorch(Vector3 position)
        {
            GameObject torch;
            if (torchPrefab != null)
            {
                // Instanciar el modelo 3D de la antorcha
                torch = Instantiate(torchPrefab, position, Quaternion.identity, transform);
                
                // Buscar o añadir luz de antorcha en el prefab
                Light light = torch.GetComponentInChildren<Light>();
                if (light == null)
                {
                    light = torch.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.color = torchColor;
                    light.range = cellSize * 2f;
                    light.intensity = 1.5f;
                }
                
                light.shadows = LightShadows.None; // Sigue optimizado sin sombras pesadas
                
                // Añadir el script de parpadeo si no lo tiene en el objeto de la luz
                if (light.gameObject.GetComponent<TorchFlicker>() == null)
                {
                    light.gameObject.AddComponent<TorchFlicker>();
                }
            }
            else
            {
                // Fallback de prototipado si no hay prefab asignado (solo luz flotante)
                torch = new GameObject("WallTorch");
                torch.transform.position = position;
                torch.transform.SetParent(transform);

                Light light = torch.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = torchColor;
                light.range = cellSize * 2f;
                light.intensity = 1.5f;
                light.shadows = LightShadows.None;

                torch.AddComponent<TorchFlicker>();
            }

            generatedObjects.Add(torch);
        }

        private GameObject SpawnObject(GameObject prefab, Vector3 position, string defaultName, Color defaultColor, PrimitiveType primitive, Vector3? scale = null)
        {
            GameObject obj;
            if (prefab != null)
            {
                obj = Instantiate(prefab, position, Quaternion.identity);
            }
            else
            {
                obj = GameObject.CreatePrimitive(primitive);
                obj.transform.position = position;
                obj.name = defaultName;
                
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = defaultColor;
                obj.GetComponent<Renderer>().material = mat;

                if (scale.HasValue)
                {
                    obj.transform.localScale = scale.Value;
                }
            }
            
            obj.transform.SetParent(transform);
            generatedObjects.Add(obj);
            return obj;
        }

        private void SetupNavMesh()
        {
            // Configurar NavMeshSurface para generar el mapa de IA al iniciar
            navMeshSurface = GetComponent<NavMeshSurface>();
            if (navMeshSurface == null)
            {
                navMeshSurface = gameObject.AddComponent<NavMeshSurface>();
            }

            // Configurar NavMeshSurface para que recoja sólo la geometría de este objeto generador
            navMeshSurface.collectObjects = CollectObjects.Children;
            navMeshSurface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
            
            // Hornear NavMesh
            navMeshSurface.BuildNavMesh();
            Debug.Log("NavMesh horneado con éxito para la IA.");
        }

        public void ClearDungeon()
        {
            // 1. Borrar objetos registrados en la lista en tiempo de ejecución
            foreach (var obj in generatedObjects)
            {
                if (obj != null)
                {
                    if (Application.isPlaying)
                        Destroy(obj);
                    else
                        DestroyImmediate(obj);
                }
            }
            generatedObjects.Clear();

            // 2. Limpiar todos los hijos reales del transform. 
            // Esto asegura que si la mazmorra ya estaba generada en el Editor, se borren 
            // los objetos viejos antes de volver a instanciar la mazmorra al dar Play.
            List<GameObject> children = new List<GameObject>();
            foreach (Transform child in transform)
            {
                children.Add(child.gameObject);
            }
            foreach (GameObject child in children)
            {
                if (Application.isPlaying)
                    Destroy(child);
                else
                    DestroyImmediate(child);
            }
        }
    }

    // Script de parpadeo simple para la simulación de fuego de antorcha
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

        private System.Collections.IEnumerator FlickerCoroutine()
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
