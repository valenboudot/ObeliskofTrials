using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public static class FinishBroadcaster
{
    public static void BroadcastFinish(double elapsedSeconds)
    {
        var props = new Hashtable
        {
            // Por jugador (no se pisan)
            [$"finish_{PhotonNetwork.LocalPlayer.ActorNumber}"] = elapsedSeconds
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }
}
