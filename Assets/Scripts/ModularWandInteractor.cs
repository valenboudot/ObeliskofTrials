using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class ModularWandInteractor : MonoBehaviourPun
{
    [Header("Estado de Varitas")]
    [SerializeField] private bool hasIceWand;
    [SerializeField] private bool hasJumpWand;

    [Header("Iconos de UI (Canvas)")]
    [SerializeField] private GameObject noneWandIcon;
    [SerializeField] private GameObject iceWandIcon;
    [SerializeField] private GameObject jumpWandIcon;

    [Header("Configuración Común")]
    [SerializeField] private float clickMaxDistance = 50f;
    [SerializeField] private Camera cam;

    [Header("Configuración Varita de Salto")]
    [SerializeField] private float jumpBuffDuration = 6f;
    [SerializeField] private float jumpMultiplier = 1.8f;
    [SerializeField] private GameObject jumpBeamPrefab;

    [Header("Configuración Varita de Hielo")]
    [SerializeField] private float freezeDuration = 3f;
    [SerializeField] private GameObject freezeBeamPrefab;

    [SerializeField] private float beamDuration = 1.0f;

    private void Start()
    {
        if (!photonView.IsMine) return;
        if (!cam) cam = Camera.main;
        UpdateWandUI();
    }

    public bool HasIceWand => hasIceWand;
    public bool HasJumpWand => hasJumpWand;

    [PunRPC]
    public void RPC_SetHasIceWand(bool value)
    {
        hasIceWand = value;
        UpdateWandUI();
    }

    [PunRPC]
    public void RPC_SetHasJumpWand(bool value)
    {
        hasJumpWand = value;
        UpdateWandUI();
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        if (!hasIceWand && !hasJumpWand) return;

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
                    if (hasIceWand)
                    {
                        ActivateFreezeEffect(targetPV);
                    }
                    else if (hasJumpWand)
                    {
                        ActivateJumpEffect(targetPV);
                    }
                }
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            if (hasJumpWand)
            {
                ActivateSelfJumpEffect();
            }
        }
    }

    private void ActivateFreezeEffect(PhotonView targetPV)
    {
        var now = PhotonNetwork.Time;

        targetPV.RPC("RPC_StartFreeze", RpcTarget.All, now, freezeDuration);

        photonView.RPC(nameof(RPC_DrawFreezeBeam), RpcTarget.All, photonView.ViewID, targetPV.ViewID);
    }

    private void ActivateJumpEffect(PhotonView targetPV)
    {
        var now = PhotonNetwork.Time;

        targetPV.RPC("RPC_StartJumpBuff", RpcTarget.All, now, jumpBuffDuration, jumpMultiplier + 1);

        photonView.RPC(nameof(RPC_DrawJumpBeam), RpcTarget.All, photonView.ViewID, targetPV.ViewID);
    }
    private void ActivateSelfJumpEffect()
    {
        var now = PhotonNetwork.Time;

        photonView.RPC("RPC_StartJumpBuff", RpcTarget.All, now, jumpBuffDuration, jumpMultiplier);
    }

    [PunRPC]
    private void RPC_DrawFreezeBeam(int casterViewID, int targetViewID)
    {
        if (freezeBeamPrefab == null)
        {
            Debug.LogError("FreezeBeamPrefab no está asignado en el ModularWandInteractor.");
            return;
        }

        PhotonView casterPV = PhotonView.Find(casterViewID);
        PhotonView targetPV = PhotonView.Find(targetViewID);

        if (casterPV != null && targetPV != null)
        {
            GameObject beamObj = Instantiate(freezeBeamPrefab, Vector3.zero, Quaternion.identity);
            TemporaryBeamController beamScript = beamObj.GetComponent<TemporaryBeamController>();

            if (beamScript != null)
            {
                beamScript.targetA = casterPV.transform;
                beamScript.targetB = targetPV.transform;
                beamScript.lifeTime = beamDuration;
            }
            else
            {
                Debug.LogError("El prefab FreezeBeamPrefab no tiene el script TemporaryBeamController");
                Destroy(beamObj);
            }
        }
    }

    [PunRPC]
    private void RPC_DrawJumpBeam(int casterViewID, int targetViewID)
    {
        if (jumpBeamPrefab == null)
        {
            Debug.LogError("JumpBeamPrefab no está asignado en el ModularWandInteractor.");
            return;
        }

        PhotonView casterPV = PhotonView.Find(casterViewID);
        PhotonView targetPV = PhotonView.Find(targetViewID);

        if (casterPV != null && targetPV != null)
        {
            GameObject beamObj = Instantiate(jumpBeamPrefab, Vector3.zero, Quaternion.identity);
            TemporaryBeamController beamScript = beamObj.GetComponent<TemporaryBeamController>();

            if (beamScript != null)
            {
                beamScript.targetA = casterPV.transform;
                beamScript.targetB = targetPV.transform;
                beamScript.lifeTime = beamDuration;
            }
            else
            {
                Debug.LogError("El prefab JumpBeamPrefab no tiene el script TemporaryBeamController");
                Destroy(beamObj);
            }
        }
    }

    private void UpdateWandUI()
    {
        if (!photonView.IsMine) return;

        if (noneWandIcon == null || iceWandIcon == null || jumpWandIcon == null)
        {
            Debug.LogWarning("Los iconos de UI de la varita no están asignados en el ModularWandInteractor.");
            return;
        }

        if (hasIceWand)
        {
            noneWandIcon.SetActive(false);
            iceWandIcon.SetActive(true);
            jumpWandIcon.SetActive(false);
        }
        else if (hasJumpWand)
        {
            noneWandIcon.SetActive(false);
            iceWandIcon.SetActive(false);
            jumpWandIcon.SetActive(true);
        }
        else
        {
            noneWandIcon.SetActive(true);
            iceWandIcon.SetActive(false);
            jumpWandIcon.SetActive(false);
        }
    }
}