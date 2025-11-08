using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SyncPuzzleManager : MonoBehaviourPunCallbacks
{
    public ButtonSyncTrigger triggerA;
    public ButtonSyncTrigger triggerB;
    public GameObject objectToDeactivate;
    public float activationWindow = 2f;

    private double triggerATime = 0;
    private double triggerBTime = 0;

    private bool isDoorOpen = false;

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

    private void CheckTriggers()
    {
        if (triggerATime <= 0 || triggerBTime <= 0)
        {
            return;
        }

        double timeDifference = System.Math.Abs(triggerATime - triggerBTime);

        Debug.Log(" Diferencia de " + timeDifference + " MS");
        Debug.Log(" A "+ triggerATime+" B "+triggerBTime+" ");

        if (timeDifference <= activationWindow)
        {
            ExecuteAction();
        }
        else
        {

        }

        ResetTriggers();
    }

    private void ExecuteAction()
    {
        isDoorOpen = true;
        photonView.RPC("Rpc_OpenTheDoor", RpcTarget.All);
    }

    private void ResetTriggers()
    {
        triggerATime = 0;
        triggerBTime = 0;
    }

    [PunRPC]
    private void Rpc_OpenTheDoor()
    {
        if (objectToDeactivate != null)
        {
            objectToDeactivate.SetActive(false);
        }
        isDoorOpen = true;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient && isDoorOpen)
        {
            photonView.RPC("Rpc_OpenTheDoor", newPlayer);
        }
    }
}