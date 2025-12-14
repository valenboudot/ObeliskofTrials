using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class InteractableWand : MonoBehaviourPun, IInteractable
{
    public enum WandType { Ice, Jump }

    [SerializeField] private WandType typeOfWand;

    public void Interact()
    {
        ModularWandInteractor localPlayerInteractor = FindLocalPlayerInteractor();

        if (localPlayerInteractor == null)
        {
            //Debug.LogWarning("No se encontró el ModularWandInteractor del jugador local.");
            return;
        }

        if (localPlayerInteractor.HasIceWand || localPlayerInteractor.HasJumpWand)
        {
            //Debug.Log("El jugador ya tiene una varita, no puede recoger otra.");
            return;
        }

        PhotonView playerPV = localPlayerInteractor.GetComponent<PhotonView>();

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

    private ModularWandInteractor FindLocalPlayerInteractor()
    {
        var allInteractors = FindObjectsOfType<ModularWandInteractor>();

        foreach (var interactor in allInteractors)
        {
            if (interactor.photonView.IsMine)
            {
                return interactor;
            }
        }
        return null;
    }
}