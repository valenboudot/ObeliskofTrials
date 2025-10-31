using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections;

[RequireComponent(typeof(PhotonView))]
public class SynchronicityPuzzleManager : MonoBehaviourPunCallbacks
{
    [Header("Objetivo del Puzzle (Puerta)")]
    [Tooltip("El objeto de la puerta que se moverá.")]
    public Transform doorToMove;
    public Vector3 doorOpenOffset = new Vector3(0, 5, 0); // Mover 5u hacia arriba
    public float doorMoveSpeed = 1f;

    [Header("Configuración del Puzzle")]
    [Tooltip("Tiempo máximo (segundos) permitido entre la primera y la última presión.")]
    public float allowedDelay = 1.0f;
    [Tooltip("El número de botones que deben presionarse (ej: 2)")]
    public int requiredButtonPresses = 2;

    private const string BUTTON_TIMES_KEY = "PuzzleTimes";
    private PhotonView pv;
    private Vector3 doorClosedPosition;

    void Start()
    {
        pv = GetComponent<PhotonView>();
        if (doorToMove != null)
        {
            doorClosedPosition = doorToMove.position;
        }
    }

    // ----------------------------------------------------------------------
    // 1. EL MASTERCLIENT RECIBE LA PRESIÓN DEL BOTÓN
    // ----------------------------------------------------------------------

    [PunRPC]
    private void Rpc_RegisterButtonPress(int buttonID, double pressTime)
    {
        // Solo el MasterClient ejecuta esta lógica
        if (!PhotonNetwork.IsMasterClient) return;

        // Obtener la lista actual de tiempos de la sala
        Hashtable buttonTimes = new Hashtable();
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(BUTTON_TIMES_KEY))
        {
            buttonTimes = (Hashtable)PhotonNetwork.CurrentRoom.CustomProperties[BUTTON_TIMES_KEY];
        }

        // Registrar o sobrescribir el tiempo de este botón
        buttonTimes[buttonID] = pressTime;

        // Actualizar las propiedades de la sala para que OnRoomPropertiesUpdate se dispare
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { BUTTON_TIMES_KEY, buttonTimes } });

        // Iniciar un temporizador de reseteo (si es el primer botón presionado)
        if (buttonTimes.Count == 1)
        {
            StartCoroutine(ResetPuzzleAfterDelay());
        }
    }

    // ----------------------------------------------------------------------
    // 2. EL MASTERCLIENT JUZGA EL RESULTADO
    // ----------------------------------------------------------------------

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!propertiesThatChanged.ContainsKey(BUTTON_TIMES_KEY)) return;

        Hashtable buttonTimes = (Hashtable)PhotonNetwork.CurrentRoom.CustomProperties[BUTTON_TIMES_KEY];

        // Si tenemos suficientes presiones registradas, juzgar
        if (buttonTimes.Count == requiredButtonPresses)
        {
            // Detener el temporizador de reseteo
            StopAllCoroutines();

            // Extraer los tiempos
            double earliestTime = double.MaxValue;
            double latestTime = double.MinValue;

            foreach (var timeEntry in buttonTimes.Values)
            {
                double time = (double)timeEntry;
                if (time < earliestTime) earliestTime = time;
                if (time > latestTime) latestTime = time;
            }

            // Comparar
            float difference = (float)(latestTime - earliestTime);

            if (difference <= allowedDelay)
            {
                // ¡ÉXITO!
                Debug.Log($"PUZZLE ÉXITO! Diferencia: {difference}s");
                pv.RPC("Rpc_ExecuteSuccess", RpcTarget.All);
            }
            else
            {
                // ¡FALLO!
                Debug.Log($"PUZZLE FALLO! Diferencia: {difference}s");
                pv.RPC("Rpc_ExecuteFail", RpcTarget.All);
            }
        }
    }

    // ----------------------------------------------------------------------
    // 3. EJECUCIÓN DEL RESULTADO (EN TODOS LOS CLIENTES)
    // ----------------------------------------------------------------------

    [PunRPC]
    private void Rpc_ExecuteSuccess()
    {
        // Todos ejecutan esto
        Debug.Log("Abriendo la puerta...");
        if (doorToMove != null)
        {
            StartCoroutine(MoveDoorCoroutine(doorClosedPosition + doorOpenOffset));
        }

        // El MasterClient limpia las propiedades
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(ResetPuzzleAfterDelay(3f)); // Resetea el puzzle después de 3s
        }
    }

    [PunRPC]
    private void Rpc_ExecuteFail()
    {
        // Todos ejecutan esto (ej. sonido de error)
        Debug.Log("Sonido de fallo...");

        // El MasterClient limpia las propiedades
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(ResetPuzzleAfterDelay(1f)); // Resetea rápido
        }
    }

    // ----------------------------------------------------------------------
    // 4. LÓGICA DE MOVIMIENTO Y RESETEO
    // ----------------------------------------------------------------------

    private IEnumerator MoveDoorCoroutine(Vector3 targetPosition)
    {
        float t = 0;
        Vector3 startPos = doorToMove.position;
        while (t < 1)
        {
            t += Time.deltaTime * doorMoveSpeed;
            doorToMove.position = Vector3.Lerp(startPos, targetPosition, t);
            yield return null;
        }
        doorToMove.position = targetPosition;
    }

    // Resetea el puzzle si no se completa a tiempo
    private IEnumerator ResetPuzzleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (PhotonNetwork.IsMasterClient)
        {
            // Limpia las propiedades de la sala
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { BUTTON_TIMES_KEY, null } });

            // Opcional: Llama a un RPC para cerrar la puerta si estaba abierta
            // pv.RPC("Rpc_CloseDoor", RpcTarget.All);
        }
    }

    // Sobrecarga del método para el reseteo por timeout
    private IEnumerator ResetPuzzleAfterDelay()
    {
        // Espera el tiempo límite + un pequeño margen
        yield return new WaitForSeconds(allowedDelay + 0.2f);

        if (PhotonNetwork.IsMasterClient)
        {
            Hashtable buttonTimes = (Hashtable)PhotonNetwork.CurrentRoom.CustomProperties[BUTTON_TIMES_KEY];

            // Si el puzzle no se completó (sigue con 1 sola presión), falla.
            if (buttonTimes != null && buttonTimes.Count < requiredButtonPresses)
            {
                pv.RPC("Rpc_ExecuteFail", RpcTarget.All);
            }
        }
    }
}