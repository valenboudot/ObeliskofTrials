using UnityEngine;
using Photon.Pun;

public class StartTimerOnLocalSpawn : MonoBehaviourPun
{
    void Start()
    {
        if (!photonView.IsMine) return;
        if (LocalRunTimer.Instance != null)
            LocalRunTimer.Instance.StartRun();
    }
}
