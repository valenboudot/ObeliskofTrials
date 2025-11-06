using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

[RequireComponent(typeof(PhotonView))]
public class PlayerGrabberPhoton : MonoBehaviourPun, IPunOwnershipCallbacks
{
    [Header("DEBUG")] public bool debugLogs = true;

    [Header("Referencias")] public Transform cameraTransform;

    [Header("Selección")]
    public LayerMask grabbableLayers;
    public float maxGrabDistance = 4f;
    public float grabRadius = 0.25f;

    [Header("Sujeción")]
    public float holdDistance = 2f;
    public Vector2 holdDistanceLimits = new Vector2(1f, 5f);
    public float scrollSpeed = 2f;

    [Header("Control físico")]
    public float positionFollowGain = 12f;
    public float maxFollowSpeed = 12f;
    public float rotationFollowGain = 15f;
    public float heldDrag = 8f;
    public bool disableGravityWhileHeld = true;

    [Header("Lanzamiento")] public float throwImpulse = 8f;

    [Header("Controles")]
    public KeyCode grabKey = KeyCode.E;
    public KeyCode throwKey = KeyCode.Mouse1;

    [Header("Net")]
    public bool requireOwnershipToMove = true;
    public float ownershipTimeout = 1.0f;

    
    public System.Action<GameObject> OnLocalGrab;
    public System.Action<GameObject> OnLocalRelease;

    private Rigidbody heldRb;
    private PhotonView heldPv;
    private Collider[] heldColliders;
    private float originalDrag;
    private bool originalUseGravity;
    private Collider[] playerColliders;
    private Coroutine ownershipLoop;

    private void OnEnable() { PhotonNetwork.AddCallbackTarget(this); }
    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
        if (heldRb != null) Release(false);
    }

    private void Awake()
    {
        if (!photonView.IsMine) { enabled = false; return; }

        if (cameraTransform == null)
        {
            var cam = GetComponentInChildren<Camera>();
            if (cam == null && Camera.main != null) cam = Camera.main;
            if (cam != null) cameraTransform = cam.transform;
        }

        playerColliders = GetComponentsInChildren<Collider>(true);
    }

    private void Update()
    {
        if (!photonView.IsMine || cameraTransform == null) return;

        if (heldRb != null)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.0001f)
            {
                holdDistance = Mathf.Clamp(
                    holdDistance + scroll * scrollSpeed,
                    holdDistanceLimits.x, holdDistanceLimits.y
                );
            }
        }

        if (Input.GetKeyDown(grabKey))
        {
            if (heldRb == null) TryGrab();
            else Release(false);
        }

        if (Input.GetKeyDown(throwKey) && heldRb != null) Release(true);

        Debug.DrawRay(cameraTransform.position, cameraTransform.forward * maxGrabDistance, Color.cyan);
    }

    private void FixedUpdate()
    {
        if (heldRb == null) return;

        
        if (requireOwnershipToMove && heldPv != null && !heldPv.IsMine)
        {
            if (debugLogs) Debug.Log("[Grab] Esperando ownership/control (IsMine) para mover la caja…");
            return;
        }

        Vector3 targetPos = cameraTransform.position + cameraTransform.forward * holdDistance;
        Vector3 toTarget = targetPos - heldRb.position;

        Vector3 desiredVel = toTarget * positionFollowGain;
        if (desiredVel.magnitude > maxFollowSpeed)
            desiredVel = desiredVel.normalized * maxFollowSpeed;

        heldRb.velocity = desiredVel;

        Quaternion targetRot = Quaternion.LookRotation(cameraTransform.forward, Vector3.up);
        Quaternion delta = targetRot * Quaternion.Inverse(heldRb.rotation);
        delta.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f) angle -= 360f;
        heldRb.angularVelocity = (Mathf.Abs(angle) > 0.1f)
            ? axis * angle * Mathf.Deg2Rad * rotationFollowGain
            : Vector3.zero;
    }

    private void TryGrab()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        if (!Physics.SphereCast(ray, grabRadius, out RaycastHit hit, maxGrabDistance, grabbableLayers, QueryTriggerInteraction.Ignore))
            return;

        Rigidbody rb = hit.rigidbody;
        if (rb == null || rb.isKinematic) return;

        PhotonView pv = rb.GetComponent<PhotonView>();
        if (pv == null) pv = rb.GetComponentInParent<PhotonView>();

        
        heldRb = rb;
        heldPv = pv;
        heldColliders = heldRb.GetComponentsInChildren<Collider>(true);

        
        originalDrag = heldRb.drag;
        originalUseGravity = heldRb.useGravity;
        heldRb.drag = heldDrag;
        if (disableGravityWhileHeld) heldRb.useGravity = false;
        heldRb.interpolation = RigidbodyInterpolation.Interpolate;
        heldRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        ToggleIgnoreCollision(heldColliders, true);
        holdDistance = Mathf.Clamp(Vector3.Distance(cameraTransform.position, heldRb.position), holdDistanceLimits.x, holdDistanceLimits.y);

       
        if (pv != null)
        {
            if (debugLogs)
                Debug.Log($"[Grab] PV {pv.ViewID} transfer={pv.OwnershipTransfer} isScene={pv.IsSceneView} IsMine={pv.IsMine} ownerActor={pv.OwnerActorNr} master={PhotonNetwork.MasterClient?.ActorNumber}");

            if (pv.IsSceneView && !PhotonNetwork.IsMasterClient)
            {
                photonView.RPC(nameof(RPC_RequestSceneObjectOwnership), RpcTarget.MasterClient, pv.ViewID, PhotonNetwork.LocalPlayer.ActorNumber);
            }
            else if (!pv.IsMine)
            {
                pv.RequestOwnership();
            }

            if (ownershipLoop != null) StopCoroutine(ownershipLoop);
            ownershipLoop = StartCoroutine(WaitOwnershipOrControl(pv, ownershipTimeout));
        }

        
        var sync = heldRb.GetComponent<GrabbableHoldSync>();
        if (sync != null) sync.NetGrab(photonView.ViewID, heldDrag, disableGravityWhileHeld);

       
        OnLocalGrab?.Invoke(heldRb.gameObject);
    }

    private IEnumerator WaitOwnershipOrControl(PhotonView pv, float timeout)
    {
        float t = 0f;
        while (pv != null && !pv.IsMine && t < timeout)
        {
            if (!pv.IsSceneView) pv.RequestOwnership();
            yield return new WaitForSeconds(0.25f);
            t += 0.25f;
        }
        if (debugLogs && pv != null) Debug.Log($"[Grab] Estado final: IsMine={pv.IsMine}");
        ownershipLoop = null;
    }

    private void Release(bool doThrow)
    {
        if (heldRb == null) return;

        
        OnLocalRelease?.Invoke(heldRb.gameObject);

        heldRb.drag = originalDrag;
        heldRb.useGravity = originalUseGravity;
        ToggleIgnoreCollision(heldColliders, false);

        var sync = heldRb.GetComponent<GrabbableHoldSync>();
        if (sync != null)
        {
            Vector3 throwVel = doThrow ? (cameraTransform.forward * throwImpulse) : Vector3.zero;
            sync.NetRelease(doThrow, throwVel);
        }
        else if (doThrow)
        {
            heldRb.AddForce(cameraTransform.forward * throwImpulse, ForceMode.VelocityChange);
        }

        if (ownershipLoop != null) { StopCoroutine(ownershipLoop); ownershipLoop = null; }
        heldRb = null; heldPv = null; heldColliders = null;
    }

    private void ToggleIgnoreCollision(Collider[] targetColliders, bool ignore)
    {
        if (targetColliders == null || playerColliders == null) return;
        foreach (var pc in playerColliders)
        {
            if (pc == null) continue;
            foreach (var tc in targetColliders)
            {
                if (tc == null) continue;
                if (pc == tc) continue;
                Physics.IgnoreCollision(pc, tc, ignore);
            }
        }
    }

    
    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer) { }
    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {
        if (heldPv != null && targetView == heldPv && debugLogs)
            Debug.Log($"[Grab] Ownership transferido. IsMine={heldPv.IsMine} ViewID={heldPv.ViewID}");
    }
    public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
    {
        if (heldPv != null && targetView == heldPv)
            Debug.LogWarning($"[Grab] Ownership FAIL ViewID={heldPv.ViewID}");
    }

    
    [PunRPC]
    private void RPC_RequestSceneObjectOwnership(int viewId, int requesterActorNumber, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        var target = PhotonView.Find(viewId);
        if (target != null) target.TransferOwnership(requesterActorNumber);
    }
}
