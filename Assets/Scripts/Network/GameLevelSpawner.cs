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
    private const string SPAWN_OCCUPIED_KEY = "SpawnUsed"; // Clave para las Propiedades de la Sala
    private Transform[] availableSpawnPoints;

    [System.Serializable]
    public class SpawnSet
    {
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
        else
        {
            // Fallback si no hay contenedor, usa un array vacío
            availableSpawnPoints = new Transform[0];
            Debug.LogWarning("SpawnPointsContainer no está asignado. Los jugadores aparecerán en (0,0,0).");
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
                spawnPoint ? spawnPoint.position : Vector3.zero, // Usa Vector3.zero si el spawn es nulo
                spawnPoint ? spawnPoint.rotation : Quaternion.identity
            );

            // Si el spawnPoint es válido y no es el transform de este spawner...
            if (spawnPoint != null && spawnPoint != transform)
            {
                // ...intenta reclamarlo.
                int index = System.Array.IndexOf(availableSpawnPoints, spawnPoint);
                if (index != -1)
                {
                    ClaimSpawnPoint(index);
                }
            }
        }

        SpawnFromSets();
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
        if (PhotonNetwork.IsMasterClient)
        {
            Hashtable props = new Hashtable { { SPAWN_OCCUPIED_KEY, null } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }

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
        if (availableSpawnPoints.Length == 0)
        {
            Debug.LogWarning("No hay spawn points disponibles en el container.");
            return transform;
        }

        bool[] usedSpawns = null;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(SPAWN_OCCUPIED_KEY, out object usedSpawnsObject))
        {
            usedSpawns = usedSpawnsObject as bool[];
        }

        List<Transform> freeSpawns = new List<Transform>();
        for (int i = 0; i < availableSpawnPoints.Length; i++)
        {
            bool isUsed = usedSpawns != null && i < usedSpawns.Length && usedSpawns[i];

            if (!isUsed)
            {
                freeSpawns.Add(availableSpawnPoints[i]);
            }
        }

        if (freeSpawns.Count > 0)
        {
            return freeSpawns[Random.Range(0, freeSpawns.Count)];
        }
        else
        {
            return availableSpawnPoints[Random.Range(0, availableSpawnPoints.Length)];
        }
    }

    private void ClaimSpawnPoint(int index)
    {
        if (index < 0 || index >= availableSpawnPoints.Length) return;

        Hashtable propertiesToSet = new Hashtable();
        Hashtable expectedProperties = new Hashtable();

        bool[] currentUsedSpawns;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(SPAWN_OCCUPIED_KEY, out object currentSpawnsObj))
        {
            currentUsedSpawns = currentSpawnsObj as bool[];
            if (currentUsedSpawns == null || currentUsedSpawns.Length != availableSpawnPoints.Length)
            {
                currentUsedSpawns = new bool[availableSpawnPoints.Length];
            }
        }
        else
        {
            currentUsedSpawns = new bool[availableSpawnPoints.Length];
        }

        expectedProperties[SPAWN_OCCUPIED_KEY] = currentUsedSpawns;

        bool[] newUsedSpawns = new bool[availableSpawnPoints.Length];
        currentUsedSpawns.CopyTo(newUsedSpawns, 0);
        newUsedSpawns[index] = true;

        propertiesToSet[SPAWN_OCCUPIED_KEY] = newUsedSpawns;

        PhotonNetwork.CurrentRoom.SetCustomProperties(propertiesToSet, expectedProperties);
    }
}