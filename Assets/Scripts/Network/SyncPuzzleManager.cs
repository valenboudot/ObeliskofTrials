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

    private bool triggerAActivated = false;
    private bool triggerBActivated = false;
    private Coroutine timerCoroutine;

    private bool isDoorOpen = false;

    public override void OnEnable()
    {
        base.OnEnable();

        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        if (triggerA != null)
            triggerA.OnInteracted += OnTriggerAActivated;

        if (triggerB != null)
            triggerB.OnInteracted += OnTriggerBActivated;
    }

    public override void OnDisable()
    {
        base.OnDisable();

        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        if (triggerA != null)
            triggerA.OnInteracted -= OnTriggerAActivated;

        if (triggerB != null)
            triggerB.OnInteracted -= OnTriggerBActivated;
    }

    private void OnTriggerAActivated()
    {
        triggerAActivated = true;
        CheckTriggers();
    }

    private void OnTriggerBActivated()
    {
        triggerBActivated = true;
        CheckTriggers();
    }

    private void CheckTriggers()
    {
        if (triggerAActivated && triggerBActivated)
        {
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }
            ExecuteAction();
            ResetTriggers();
        }
        else
        {
            if (timerCoroutine == null)
                timerCoroutine = StartCoroutine(ActivationTimer());
        }
    }

    private IEnumerator ActivationTimer()
    {
        yield return new WaitForSeconds(activationWindow);
        ResetTriggers();
        timerCoroutine = null;
    }

    private void ExecuteAction()
    {
        isDoorOpen = true;

        photonView.RPC("Rpc_OpenTheDoor", RpcTarget.All);
    }

    private void ResetTriggers()
    {
        triggerAActivated = false;
        triggerBActivated = false;
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