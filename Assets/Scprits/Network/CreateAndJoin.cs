using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class CreateAndJoin : MonoBehaviourPunCallbacks
{
    public TMP_InputField InputCreate;
    public TMP_InputField InputJoin;

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(InputCreate.text);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(InputJoin.text);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("TowerEntrance");
    }
}
