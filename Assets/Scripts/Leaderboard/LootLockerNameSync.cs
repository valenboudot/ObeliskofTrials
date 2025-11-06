using UnityEngine;
using Photon.Pun;

public class LootLockerNameSync : MonoBehaviour
{
    void Start()
    {
        // Intentar una vez por segundo hasta que LootLocker esté listo
        InvokeRepeating(nameof(TrySet), 0.5f, 1f);
    }

    void TrySet()
    {
        if (!LootLockerBootstrap.SessionStarted) return; // espera al login de LootLocker
        var nick = PhotonNetwork.NickName;
        if (!string.IsNullOrEmpty(nick))
            PlayerNameHelper.SetPlayerName(nick);
        CancelInvoke(nameof(TrySet));
    }
}