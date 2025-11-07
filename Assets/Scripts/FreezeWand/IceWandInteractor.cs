using Photon.Pun;
using UnityEngine;

// [RequireComponent(typeof(PhotonView))]
public class IceWandInteractor : MonoBehaviourPun
{
    [Header("Estado")]
    [SerializeField] private bool hasWand;

    [Header("Freeze Settings")]
    [SerializeField] private float freezeDuration = 3f;
    [SerializeField] private float clickMaxDistance = 50f;

    [Header("Raycast")]
    [SerializeField] private Camera cam;

    [Header("Uso")]
    [SerializeField] private bool consumeOnUse = false;

    [Header("Visuals")]
    [SerializeField] private GameObject freezeBeamPrefab;
    [SerializeField] private float beamDuration = 1.0f;

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

                    targetPV.RPC("RPC_StartFreeze", RpcTarget.All, now, freezeDuration);

                    photonView.RPC("RPC_DrawFreezeBeam", RpcTarget.All, photonView.ViewID, targetPV.ViewID);

                    if (consumeOnUse)
                        photonView.RPC(nameof(RPC_SetHasWand), RpcTarget.All, false);
                }
            }
        }
    }

    [PunRPC]
    private void RPC_DrawFreezeBeam(int casterViewID, int targetViewID)
    {
        if (freezeBeamPrefab == null)
        {
            Debug.LogError("FreezeBeamPrefab no está asignado en el IceWandInteractor.");
            return;
        }

        PhotonView casterPV = PhotonView.Find(casterViewID);
        PhotonView targetPV = PhotonView.Find(targetViewID);

        if (casterPV != null && targetPV != null)
        {
            GameObject beamObj = Instantiate(freezeBeamPrefab, Vector3.zero, Quaternion.identity);

            TemporaryBeamController beamScript = beamObj.GetComponent<TemporaryBeamController>();

            beamScript.targetA = casterPV.transform;
            beamScript.targetB = targetPV.transform;
            beamScript.lifeTime = beamDuration;
        }
    }
}
