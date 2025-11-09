using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; // 1. ¡IMPORTANTE! Añadir esto
using Photon.Pun;
using Photon.Realtime;

public class SyncPuzzleManager : MonoBehaviourPunCallbacks
{
    public ButtonSyncTrigger triggerA;
    public ButtonSyncTrigger triggerB;
    public float activationWindow = 2f;

    public UnityEvent OnPuzzleSolved;

    private double triggerATime = 0;
    private double triggerBTime = 0;
    private bool isDoorOpen = false;

    #region Enable/Disable y Trigger Handlers
    public override void OnEnable()
    {
        base.OnEnable();
        if (!PhotonNetwork.IsMasterClient) return;

        if (triggerA != null)
            triggerA.OnInteracted += OnTriggerAActivated;
        if (triggerB != null)
            triggerB.OnInteracted += OnTriggerBActivated;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (!PhotonNetwork.IsMasterClient) return;

        if (triggerA != null)
            triggerA.OnInteracted -= OnTriggerAActivated;
        if (triggerB != null)
            triggerB.OnInteracted -= OnTriggerBActivated;
    }

    private void OnTriggerAActivated(PhotonMessageInfo info)
    {
        triggerATime = info.SentServerTime;
        CheckTriggers();
    }

    private void OnTriggerBActivated(PhotonMessageInfo info)
    {
        triggerBTime = info.SentServerTime;
        CheckTriggers();
    }
    #endregion

    private void CheckTriggers()
    {
        if (triggerATime <= 0 || triggerBTime <= 0)
        {
            return;
        }

        double timeDifference = System.Math.Abs(triggerATime - triggerBTime);

        if (timeDifference <= activationWindow)
        {
            ExecuteAction();
        }

        ResetTriggers();
    }

    private void ExecuteAction()
    {
        if (isDoorOpen) return;

        isDoorOpen = true;
        photonView.RPC("Rpc_ExecutePuzzleAction", RpcTarget.AllBuffered); // Cambiado el nombre para más claridad
    }

    private void ResetTriggers()
    {
        triggerATime = 0;
        triggerBTime = 0;
    }

    [PunRPC]
    private void Rpc_ExecutePuzzleAction()
    {
        if (OnPuzzleSolved != null)
        {
            OnPuzzleSolved.Invoke();
        }

        isDoorOpen = true;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient && isDoorOpen)
        {
            photonView.RPC("Rpc_ExecutePuzzleAction", newPlayer);
        }
    }
}