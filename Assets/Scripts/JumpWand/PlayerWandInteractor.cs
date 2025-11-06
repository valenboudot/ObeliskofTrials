using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class PlayerWandInteractor : MonoBehaviourPun
{
    [Header("Estado")]
    [SerializeField] private bool hasWand;

    [Header("Buff Settings")]
    [SerializeField] private float buffDuration = 6f;
    [SerializeField] private float jumpMultiplier = 1.8f;
    [SerializeField] private float clickMaxDistance = 50f;

    [Header("Raycast")]
    [SerializeField] private Camera cam; 

    [Header("Uso")]
    [SerializeField] private bool consumeOnUse = false; 

    private void Start()
    {
        if (!photonView.IsMine) return;
        if (!cam) cam = Camera.main;
    }

    public bool HasWand => hasWand;

    [PunRPC]
    public void RPC_SetHasWand(bool value)
    {
        hasWand = value;
    }

    private void Update()
    {
        if (!photonView.IsMine) return;
        if (!hasWand) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, clickMaxDistance))
            {
                PhotonView targetPV = hit.collider.GetComponentInParent<PhotonView>();
                if (targetPV != null && targetPV != photonView)
                {
                    var now = PhotonNetwork.Time;
                    targetPV.RPC("RPC_StartJumpBuff", RpcTarget.All, now, buffDuration, jumpMultiplier);

                    
                    if (consumeOnUse)
                        photonView.RPC(nameof(RPC_SetHasWand), RpcTarget.All, false);
                }
            }
        }
    }
}
