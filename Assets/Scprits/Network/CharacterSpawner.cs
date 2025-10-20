using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon; // Necesario para Hashtable
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement; // Necesario para verificar la escena

public class CharacterSpawner : MonoBehaviourPunCallbacks
{
    // --- Configuración de Personaje Único ---
    [Tooltip("El único prefab de jugador (debe estar en Resources).")]
    public GameObject playerPrefab;

    // --- Configuración de Spawn Points ---
    [Header("Configuración de Spawn")]
    [Tooltip("Objeto padre que contiene todos los puntos de spawn.")]
    public Transform spawnPointsContainer;

    // Nombre de la propiedad de sala para rastrear spawn points ocupados
    private const string SPAWN_OCCUPIED_KEY = "SpawnUsed";

    private Transform[] availableSpawnPoints;

    void Awake()
    {
        // Almacenar todos los puntos de spawn hijos del contenedor
        if (spawnPointsContainer != null)
        {
            // Filtra y obtiene los transforms de los hijos (los spawn points)
            availableSpawnPoints = spawnPointsContainer.GetComponentsInChildren<Transform>()
                .Where(t => t != spawnPointsContainer.transform) // Excluye el contenedor padre
                .ToArray();
        }
        else
        {
            // Esta advertencia solo es crítica en la escena de juego.
            Debug.LogWarning("Asigna el 'Spawn Points Container' en el Inspector. Usando el Spawner como fallback.");
        }

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // ----------------------------------------------------------------------
    // LÓGICA DE SPAWN (Escena de Juego)
    // ----------------------------------------------------------------------

    // Función que inicia la instanciación en la escena de juego
    public void InstantiateMyCharacter()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("¡No se ha asignado el Player Prefab en el Inspector!");
            return;
        }

        // 1. Encontrar un Spawn Point libre
        Transform spawnPoint = GetRandomFreeSpawnPoint();

        if (spawnPoint == null)
        {
            Debug.LogError("No hay spawn points libres. Usando el origen (0,0,0).");
            spawnPoint = transform;
        }

        // 2. Instanciar el único prefab de personaje
        // Usamos el nombre del prefab (que debe estar en la carpeta Resources)
        PhotonNetwork.Instantiate(
            playerPrefab.name,
            spawnPoint.position,
            Quaternion.identity
        );

        Debug.Log($"Jugador instanciado en: {spawnPoint.name}");

        // 3. Bloquear el spawn point si se usó correctamente
        // Solo bloqueamos si encontramos un spawn point real (no el fallback 'transform')
        if (spawnPoint != transform)
        {
            int index = System.Array.IndexOf(availableSpawnPoints, spawnPoint);
            if (index != -1)
            {
                ClaimSpawnPoint(index);
            }
        }
    }

    // ----------------------------------------------------------------------
    // GESTIÓN DE SPAWN POINTS (La lógica del bloqueo)
    // ----------------------------------------------------------------------

    private Transform GetRandomFreeSpawnPoint()
    {
        Hashtable roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        bool[] occupiedArray = new bool[availableSpawnPoints.Length];

        // Obtener la matriz de ocupación de la sala si existe
        if (roomProperties.ContainsKey(SPAWN_OCCUPIED_KEY))
        {
            occupiedArray = (bool[])roomProperties[SPWN_OCCUPIED_KEY];
        }

        // Determinar qué índices están libres
        List<int> freeIndices = new List<int>();
        for (int i = 0; i < occupiedArray.Length; i++)
        {
            if (!occupiedArray[i])
            {
                freeIndices.Add(i);
            }
        }

        if (freeIndices.Count == 0)
        {
            return null; // No hay spawns libres
        }

        // Elegir un índice libre al azar y retornar el Transform
        int randomIndex = freeIndices[Random.Range(0, freeIndices.Count)];
        return availableSpawnPoints[randomIndex];
    }

    private void ClaimSpawnPoint(int index)
    {
        // Solo el MasterClient tiene autoridad para modificar las propiedades de sala
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("Solo el MasterClient puede reclamar un spawn point.");
            return;
        }

        Hashtable roomProps = PhotonNetwork.CurrentRoom.CustomProperties;
        bool[] occupiedArray;

        if (roomProps.ContainsKey(SPAWN_OCCUPIED_KEY))
        {
            occupiedArray = (bool[])roomProps[SPAWN_OCCUPIED_KEY];
        }
        else
        {
            occupiedArray = new bool[availableSpawnPoints.Length];
        }

        if (index >= 0 && index < occupiedArray.Length)
        {
            occupiedArray[index] = true; // Marcar como usado

            // Sincronizar el array actualizado con todos los clientes
            Hashtable newProps = new Hashtable { { SPAWN_OCCUPIED_KEY, occupiedArray } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(newProps);
            Debug.Log($"Spawn Point {index} reclamado por MasterClient.");
        }
    }

    // ----------------------------------------------------------------------
    // CALLBACKS DE PHOTON (Disparadores)
    // ----------------------------------------------------------------------

    public override void OnJoinedRoom()
    {
        // Si estamos en el nivel de juego (no el lobby), instanciamos.
        // Asume que la escena de juego se llama "TowerEntrance" (ajusta a tu nombre).
        if (SceneManager.GetActiveScene().name == "TowerEntrance")
        {
            InstantiateMyCharacter();
        }
    }
}