using UnityEngine;
using Photon.Pun;

public class PuzzleButton : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("ID único para este botón (ej: 0 para el botón A, 1 para el botón B)")]
    public int buttonID = 0;

    [Tooltip("Tiempo (segundos) para que el botón se reactive después de presionarlo.")]
    public float cooldown = 2.0f;

    private bool canBePressed = true;
    private SynchronicityPuzzleManager puzzleManager;
    private PhotonView managerPhotonView;

    void Start()
    {
        // Encuentra el "cerebro" del puzzle en la escena
        puzzleManager = FindObjectOfType<SynchronicityPuzzleManager>();
        if (puzzleManager != null)
        {
            managerPhotonView = puzzleManager.GetComponent<PhotonView>();
        }
        else
        {
            Debug.LogError("No se encontró SynchronicityPuzzleManager en la escena.");
        }
    }

    // Esta función es llamada por PlayerInteractor.cs
    public void OnPressedByPlayer()
    {
        if (!canBePressed || managerPhotonView == null) return;

        canBePressed = false;

        // 1. Obtener el tiempo de red sincronizado
        double pressTime = PhotonNetwork.Time;

        // 2. Enviar un RPC al MasterClient, pasándole nuestro ID y el tiempo de presión
        managerPhotonView.RPC("Rpc_RegisterButtonPress", RpcTarget.MasterClient, buttonID, pressTime);

        // 3. Iniciar el cooldown para reactivar el botón
        StartCoroutine(CooldownCoroutine());
    }

    private System.Collections.IEnumerator CooldownCoroutine()
    {
        yield return new WaitForSeconds(cooldown);
        canBePressed = true;
    }
}