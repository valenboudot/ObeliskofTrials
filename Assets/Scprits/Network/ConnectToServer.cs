using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using TMPro;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    public TMP_InputField nameInputField;
    public GameObject NamePanel;
    public GameObject LoadingPanel;
    public GameObject ErrorText;

    private void Start()
    {

    }

    public void SetPlayerNameAndConnect()
    {
        string playerName = nameInputField.text;

        if (string.IsNullOrWhiteSpace(playerName))
        {
            Debug.LogError("El nombre de usuario no puede estar vacío.");
            ErrorText.SetActive(true);
            return;
        }

        PhotonNetwork.NickName = playerName;
        Debug.Log($"Nombre de usuario establecido: {playerName}");

        Debug.Log("Conectando a la nube...");
        PhotonNetwork.ConnectUsingSettings();

        nameInputField.interactable = false;
        NamePanel.SetActive(false);
        LoadingPanel.SetActive(true);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        SceneManager.LoadScene("MainMenu");
    }
}