using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameLevelSpawner : MonoBehaviourPunCallbacks // Renombrado de CharacterSpawner
{
    // --- VARIABLES DE TU ANTIGUO CHARACTERSPAWNER ---
    [Header("Player Prefab")]
    [Tooltip("El único prefab de jugador (debe estar en Resources).")]
    public GameObject playerPrefab;

    [Header("Configuración de Spawn Points")]
    [Tooltip("Objeto padre que contiene todos los puntos de spawn.")]
    public Transform spawnPointsContainer;
    private const string SPAWN_OCCUPIED_KEY = "SpawnUsed";
    private Transform[] availableSpawnPoints;

    // --- VARIABLES COPIADAS DEL SERVERMANAGER ---
    [Header("Objetos Estáticos (Solo MasterClient)")]
    public GameObject wandPrefab;
    public Transform wandSpawn;

    [System.Serializable]
    public class SpawnSet
    {
        [Tooltip("Prefab a instanciar (debe estar en Assets/Resources/)")]
        public GameObject prefab;
        public List<Transform> waypoints = new List<Transform>();
        public bool onlyMaster = true;
        [Min(1)] public int amountPerWaypoint = 1;
        public bool randomizeYaw = false;
    }

    [Header("Spawns por lista - SOLO MASTER")]
    public List<SpawnSet> networkSpawns = new List<SpawnSet>();

    // --- SETUP Y LIFECYCLE ---

    void Awake()
    {
        // La lógica de Awake() para llenar el array availableSpawnPoints se mantiene
        if (spawnPointsContainer != null)
        {
            availableSpawnPoints = spawnPointsContainer.GetComponentsInChildren<Transform>()
                .Where(t => t != spawnPointsContainer.transform)
                .ToArray();
        }
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Suscripción a eventos de escena para la instanciación
    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ----------------------------------------------------------------------
    // LÓGICA DE INSTANCIACIÓN PRINCIPAL
    // ----------------------------------------------------------------------

    public void InstantiateMyCharacterAndItems() // Función principal del nivel
    {
        // 1. Instanciar el jugador (Lógica de tu antiguo CharacterSpawner)
        if (playerPrefab != null)
        {
            Transform spawnPoint = GetRandomFreeSpawnPoint();
            // ... (Lógica de instanciación del playerPrefab en el spawnPoint) ...

            PhotonNetwork.Instantiate(
                playerPrefab.name,
                spawnPoint ? spawnPoint.position : Vector3.zero,
                spawnPoint ? spawnPoint.rotation : Quaternion.identity
            );

            // Reclamar el spawn point
            if (spawnPoint != transform)
            {
                int index = System.Array.IndexOf(availableSpawnPoints, spawnPoint);
                if (index != -1) ClaimSpawnPoint(index);
            }
        }

        // 2. Instanciar Ítems y Sets (Lógica del ServerManager)
        SpawnWand();
        SpawnFromSets();
    }

    private void SpawnWand()
    {
        // Instanciar la varita (solo MasterClient)
        if (PhotonNetwork.IsMasterClient && wandPrefab != null)
        {
            if (ValidatePrefab(wandPrefab, "Wand"))
            {
                PhotonNetwork.InstantiateSceneObject(
                    wandPrefab.name,
                    wandSpawn ? wandSpawn.position : Vector3.zero,
                    wandSpawn ? wandSpawn.rotation : Quaternion.identity
                );
            }
        }
    }

    private void SpawnFromSets()
    {
        // Instanciar sets de enemigos/ítems (solo MasterClient)
        if (!PhotonNetwork.IsMasterClient) return;

        foreach (var set in networkSpawns)
        {
            if (set == null || set.prefab == null || set.waypoints.Count == 0) continue;

            if (!ValidatePrefab(set.prefab, set.prefab.name)) continue;

            foreach (var wp in set.waypoints)
            {
                if (wp == null) continue;
                int count = Mathf.Max(1, set.amountPerWaypoint);

                for (int i = 0; i < count; i++)
                {
                    Quaternion rot = wp.rotation;
                    if (set.randomizeYaw)
                        rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                    PhotonNetwork.InstantiateSceneObject(set.prefab.name, wp.position, rot);
                }
            }
        }
    }

    // ----------------------------------------------------------------------
    // CALLBACK DE INICIO DE JUEGO (Disparador)
    // ----------------------------------------------------------------------

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // La constante GAME_SCENE debe estar accesible (ej: en RoomManager)
        // Usamos el nombre de la escena de juego que usaste en RoomManager.cs
        if (scene.name == "TowerEntrance")
        {
            InstantiateMyCharacterAndItems(); // Llama a la nueva función
        }
    }

    // ----------------------------------------------------------------------
    // FUNCIONES AUXILIARES (Validación y Spawn Points)
    // ----------------------------------------------------------------------

    private bool ValidatePrefab(GameObject prefab, string label)
    {
        // Copia y pega la función ValidatePrefab completa del ServerManager aquí
        // Es vital para verificar el PhotonView y la carpeta Resources.
        if (prefab == null || prefab.GetComponent<PhotonView>() == null || Resources.Load<GameObject>(prefab.name) == null)
        {
            Debug.LogError($"[{label}] Error de Prefab: Faltan componentes, no está en Resources, o es un objeto de escena.");
            return false;
        }
        return true;
    }

    private Transform GetRandomFreeSpawnPoint()
    {
        // (La lógica completa de GetRandomFreeSpawnPoint va aquí)
        // ...
        return null; // Retorno de ejemplo
    }

    private void ClaimSpawnPoint(int index)
    {
        // (La lógica completa de ClaimSpawnPoint va aquí)
        // ...
    }
}