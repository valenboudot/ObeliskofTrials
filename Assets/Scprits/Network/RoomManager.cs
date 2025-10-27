using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomManager : MonoBehaviourPunCallbacks
{
    // --- UI create room ---
    public TMP_InputField InputJoin;

    // --- UI wait room ---
    public GameObject CreatedLobbyPanel;
    public TMP_InputField RoomIDText;
    public Button StartGameButton;
    public GameObject PlaseWaitText;

    // --- UI Lista de Jugadores ---
    public RectTransform PlayerListContent; // El panel vacío que contendrá los nombres
    public GameObject PlayerListItemPrefab;
    private Dictionary<int, GameObject> playerListItems = new Dictionary<int, GameObject>();

    // --- Texts ---
    public GameObject LowNumberOfPlayersText;
    public GameObject InvalidCodeText;
    public GameObject CopiedText;

    private const int ROOM_ID_LENGTH = 6;
    private const string GAME_SCENE = "TowerEntrance";
    public UIManager uimanager;

    public void CreateRoomWithMaxPlayers(int maxPlayers)
    {
        byte maxPlayersByte = (byte)Mathf.Clamp(maxPlayers, 2, 4);

        string roomID = GenerateRandomRoomID(ROOM_ID_LENGTH);

        RoomOptions roomOptions = new RoomOptions()
        {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = maxPlayersByte
        };

        PhotonNetwork.CreateRoom(roomID, roomOptions);

        uimanager.CloseCreateRoomPopup();
    }

    private string GenerateRandomRoomID(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        char[] id = new char[length];

        for (int i = 0; i < length; i++)
        {
            id[i] = chars[Random.Range(0, chars.Length)];
        }
        return new string(id);
    }

    public void JoinRoom()
    {
        if (InputJoin.text.Length >= 4)
        {
            PhotonNetwork.JoinRoom(InputJoin.text.ToUpper());
        }
        else
        {
            Debug.LogWarning("Codigo invalido.");
            StartCoroutine(ShowWarningTemporarily(3f, InvalidCodeText));
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"¡Entraste a la sala: {PhotonNetwork.CurrentRoom.Name}!");

        CreatedLobbyPanel.SetActive(true);

        RoomIDText.text = $"{PhotonNetwork.CurrentRoom.Name}";

        StartGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);

        PlaseWaitText.gameObject.SetActive(!PhotonNetwork.IsMasterClient);

        uimanager.CreatePanel.SetActive(false);
        uimanager.JoinPanel.SetActive(false);

        UpdatePlayerList();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"{newPlayer.NickName} se ha unido a la sala.");
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"{otherPlayer.NickName} ha abandonado la sala.");
        UpdatePlayerList();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        StartGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        PlaseWaitText.gameObject.SetActive(!PhotonNetwork.IsMasterClient);
    }

    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount >= 2)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                Debug.Log("Sala cerrada perras.");

                PhotonNetwork.LoadLevel(GAME_SCENE);
            }
            else
            {
                Debug.LogWarning("Necesitas al menos 2 jugadores para empezar.");
                StartCoroutine(ShowWarningTemporarily(2f, LowNumberOfPlayersText));
            }
        }
    }

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public void CopyRoomID()
    {
        GUIUtility.systemCopyBuffer = PhotonNetwork.CurrentRoom.Name;
        Debug.Log($"ID de sala copiado: {PhotonNetwork.CurrentRoom.Name}");
        StartCoroutine(ShowWarningTemporarily(1f, CopiedText));
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"Fallo al unirse a la sala (Código: {returnCode}): {message}");

        StartCoroutine(ShowWarningTemporarily(3f, InvalidCodeText));
    }

    private IEnumerator ShowWarningTemporarily(float duration, GameObject Mensaje)
    {
        Mensaje.SetActive(true);

        yield return new WaitForSeconds(duration);

        Mensaje.SetActive(false);
    }


    private void UpdatePlayerList()
    {
        foreach (var item in playerListItems.Values)
        {
            Destroy(item);
        }
        playerListItems.Clear();

        if (PhotonNetwork.CurrentRoom != null)
        {
            foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                GameObject listItem = Instantiate(PlayerListItemPrefab, PlayerListContent);

                TMP_Text textComponent = listItem.GetComponent<TMP_Text>();

                if (player.IsMasterClient)
                {
                    textComponent.text = $"{player.NickName} (Master)";
                }
                else
                {
                    textComponent.text = player.NickName;
                }

                playerListItems.Add(player.ActorNumber, listItem);
            }
        }
    }
}