using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class WandPickup : MonoBehaviourPun
{
    public enum WandType { Ice, Jump }

    [Header("Tipo de Varita")]
    [SerializeField] private WandType typeOfWand;

    [Header("Configuración")]
    [SerializeField] private bool onlyLocalPlayerCanTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        var playerPV = other.GetComponentInParent<PhotonView>();
        if (playerPV == null) return;

        if (onlyLocalPlayerCanTrigger && !playerPV.IsMine) return;

        var interactor = other.GetComponentInParent<ModularWandInteractor>();
        if (interactor == null) return;

        if (interactor.HasIceWand || interactor.HasJumpWand)
        {
            Debug.Log("El jugador ya tiene una varita, no puede recoger otra.");
            return;
        }

        switch (typeOfWand)
        {
            case WandType.Ice:
                playerPV.RPC(nameof(ModularWandInteractor.RPC_SetHasIceWand), RpcTarget.All, true);
                break;

            case WandType.Jump:
                playerPV.RPC(nameof(ModularWandInteractor.RPC_SetHasJumpWand), RpcTarget.All, true);
                break;
        }

        photonView.RPC(nameof(RPC_RequestDestroy), RpcTarget.MasterClient);
    }

    [PunRPC]
    private void RPC_RequestDestroy()
    {
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.Destroy(gameObject);
    }
}