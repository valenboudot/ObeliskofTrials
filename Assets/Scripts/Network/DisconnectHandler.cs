using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class DisconnectHandler : MonoBehaviourPunCallbacks
{
    [Header("UI de Desconexión")]
    public GameObject disconnectPanel;
    public TextMeshProUGUI disconnectMessageText;

    [Header("Escena de Destino")]
    public string lobbySceneName = "Lobby";

    private void Start()
    {
        if (disconnectPanel != null)
        {
            disconnectPanel.SetActive(false);
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (cause == DisconnectCause.DisconnectByClientLogic)
        {
            return;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (disconnectPanel != null)
        {
            disconnectPanel.SetActive(true);
        }

        if (disconnectMessageText != null)
        {
            disconnectMessageText.text = $"Se perdió la conexión:\n{cause} y boludo";
        }
    }

    public void ReturnToLobby()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }

        SceneManager.LoadScene(lobbySceneName);
    }
}