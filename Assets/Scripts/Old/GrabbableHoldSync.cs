using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;


[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(Rigidbody))]
public class GrabbableHoldSync : MonoBehaviourPun
{
    private Rigidbody rb;
    private bool isHeld;
    private int holderViewId = -1;

   
    private static readonly Dictionary<int, Collider[]> playerColsCache = new Dictionary<int, Collider[]>();

    private float originalDrag;
    private bool originalUseGravity;
    private RigidbodyInterpolation originalInterp;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        originalDrag = rb.drag;
        originalUseGravity = rb.useGravity;
        originalInterp = rb.interpolation;
    }

    
    public void NetGrab(int playerViewId, float heldDrag = 8f, bool disableGravity = true)
    {
        if (!photonView.AmOwner) photonView.RequestOwnership();
        photonView.RPC(nameof(RPC_SetHeld), RpcTarget.AllBuffered, true, playerViewId, heldDrag, disableGravity);
    }

    
    public void NetRelease(bool applyThrow, Vector3 throwVelocity)
    {
        if (!photonView.AmOwner) photonView.RequestOwnership();
        photonView.RPC(nameof(RPC_SetReleased), RpcTarget.AllBuffered, applyThrow, throwVelocity);
    }

    [PunRPC]
    private void RPC_SetHeld(bool held, int playerViewId, float heldDrag, bool disableGravity)
    {
        isHeld = held;
        holderViewId = held ? playerViewId : -1;

        if (isHeld)
        {
            rb.drag = heldDrag;
            rb.useGravity = !disableGravity;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            ToggleIgnoreWithHolder(true);
        }
        else
        {
            rb.drag = originalDrag;
            rb.useGravity = originalUseGravity;
            rb.interpolation = originalInterp;
            ToggleIgnoreWithHolder(false);
        }
    }

    [PunRPC]
    private void RPC_SetReleased(bool applyThrow, Vector3 throwVel)
    {
        isHeld = false;
        holderViewId = -1;

        rb.drag = originalDrag;
        rb.useGravity = originalUseGravity;
        rb.interpolation = originalInterp;
        ToggleIgnoreWithHolder(false);

        if (applyThrow)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(throwVel, ForceMode.VelocityChange);
        }
    }

    private void ToggleIgnoreWithHolder(bool ignore)
    {
        if (holderViewId < 0) return;

        if (!playerColsCache.TryGetValue(holderViewId, out var holderCols) || holderCols == null)
        {
            var holderPv = PhotonView.Find(holderViewId);
            if (holderPv != null)
            {
                holderCols = holderPv.GetComponentsInChildren<Collider>(true);
                playerColsCache[holderViewId] = holderCols;
            }
        }

        var myCols = GetComponentsInChildren<Collider>(true);
        if (holderCols == null || myCols == null) return;

        foreach (var hc in holderCols)
        {
            if (hc == null) continue;
            foreach (var mc in myCols)
            {
                if (mc == null) continue;
                Physics.IgnoreCollision(hc, mc, ignore);
            }
        }
    }
}
