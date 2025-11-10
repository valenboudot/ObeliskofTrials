using UnityEngine;
using Photon.Pun;

public class LootLockerNameSync : MonoBehaviour
{
    void Start()
    {
        InvokeRepeating(nameof(TrySet), 0.5f, 1f);
    }

    void TrySet()
    {
        if (!LootLockerBootstrap.SessionStarted) return;
        var nick = PhotonNetwork.NickName;
        if (!string.IsNullOrEmpty(nick))
            PlayerNameHelper.SetPlayerName(nick);
        CancelInvoke(nameof(TrySet));
    }
}