using UnityEngine;
using Photon.Pun;

public class InteractableButton : MonoBehaviour
{
    [Header("Configuración del Puzzle")]
    [Tooltip("El 'cerebro' que maneja la lógica de tiempo (SynchronizedButtonManager)")]
    public SynchronizedButtonManager puzzleManager;

    [Tooltip("El ID único de ESTE botón (0 para el primero, 1 para el segundo)")]
    public int buttonID = 0;

    // Esto evita que el botón se presione múltiples veces seguidas
    private bool canBePressed = true;

    // Método 1: Usar un Trigger Collider (si es una placa de presión)
    private void OnTriggerEnter(Collider other)
    {
        // Verifica si el objeto que entra es un jugador local
        if (canBePressed && other.CompareTag("Player"))
        {
            PhotonView pv = other.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                Debug.Log($"Botón {buttonID} presionado por {PhotonNetwork.NickName}");

                // Llama al "cerebro" para iniciar el timer
                puzzleManager.OnActionButtonPressed(buttonID);

                // Desactiva el botón temporalmente
                canBePressed = false;
                StartCoroutine(ResetButtonDelay(2f)); // Espera 2s para reactivarse
            }
        }
    }

    // Método 2: Usar Raycast (si es un interruptor de pared)
    // (Este método requeriría un script de "interacción" en el jugador 
    // que dispare un rayo y llame a una función 'Interact()' en este script)

    private System.Collections.IEnumerator ResetButtonDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canBePressed = true;
    }
}