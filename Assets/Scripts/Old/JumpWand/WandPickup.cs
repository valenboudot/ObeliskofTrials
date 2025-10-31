using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class WandPickup : MonoBehaviourPun
{
    [SerializeField] private bool onlyLocalPlayerCanTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        var playerPV = other.GetComponentInParent<PhotonView>();
        if (playerPV == null) return;

        
        if (onlyLocalPlayerCanTrigger && !playerPV.IsMine) return;

        var interactor = other.GetComponentInParent<PlayerWandInteractor>();
        if (interactor == null) return;

        
        playerPV.RPC("RPC_SetHasWand", RpcTarget.All, true);

        
        photonView.RPC(nameof(RPC_RequestDestroy), RpcTarget.MasterClient);
    }

    [PunRPC]
    private void RPC_RequestDestroy()
    {
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.Destroy(gameObject);
    }
}
