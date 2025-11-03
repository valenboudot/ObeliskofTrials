using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameLevelSpawner : MonoBehaviourPunCallbacks
{
    [Header("Player Prefab")]
    public GameObject playerPrefab;

    [Header("Configuración de Spawn Points")]
    public Transform spawnPointsContainer;
    private const string SPAWN_OCCUPIED_KEY = "SpawnUsed";
    private Transform[] availableSpawnPoints;

    [Header("Objetos Estáticos")]
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


    void Awake()
    {
        if (spawnPointsContainer != null)
        {
            availableSpawnPoints = spawnPointsContainer.GetComponentsInChildren<Transform>()
                .Where(t => t != spawnPointsContainer.transform)
                .ToArray();
        }
        PhotonNetwork.AutomaticallySyncScene = true;
    }

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

    public void InstantiateMyCharacterAndItems()
    {
        if (playerPrefab != null)
        {
            Transform spawnPoint = GetRandomFreeSpawnPoint();
            // Lógica de instanciación del playerPrefab en el spawnPoint

            PhotonNetwork.Instantiate(
                playerPrefab.name,
                spawnPoint ? spawnPoint.position : Vector3.zero,
                spawnPoint ? spawnPoint.rotation : Quaternion.identity
            );

            if (spawnPoint != transform)
            {
                int index = System.Array.IndexOf(availableSpawnPoints, spawnPoint);
                if (index != -1) ClaimSpawnPoint(index);
            }
        }

        SpawnWand();
        SpawnFromSets();
    }

    private void SpawnWand()
    {
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "TowerEntrance")
        {
            InstantiateMyCharacterAndItems();
        }
    }

    private bool ValidatePrefab(GameObject prefab, string label)
    {
        if (prefab == null || prefab.GetComponent<PhotonView>() == null || Resources.Load<GameObject>(prefab.name) == null)
        {
            Debug.LogError($"[{label}] Error de Prefab: Faltan componentes, no está en Resources, o es un objeto de escena.");
            return false;
        }
        return true;
    }

    private Transform GetRandomFreeSpawnPoint()
    {
        return null;
    }

    private void ClaimSpawnPoint(int index)
    {

    }
}