using UnityEngine;
using System;
using System.Collections;
using Photon.Pun;

public class ButtonSyncTrigger : MonoBehaviourPun
{
    public bool moveOnX = true;
    public float moveDistance = 2f;
    public float returnDelay = 1f;
    public KeyCode interactKey = KeyCode.E;

    private Vector3 originalPosition;
    private bool playerInRange = false;
    private bool isMoving = false;

    public event Action<PhotonMessageInfo> OnInteracted;

    private void Start()
    {
        originalPosition = transform.position;
        BoxCollider box = GetComponent<BoxCollider>();
        box.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView playerView = other.GetComponent<PhotonView>();
            if (playerView != null && playerView.IsMine)
            {
                playerInRange = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView playerView = other.GetComponent<PhotonView>();
            if (playerView != null && playerView.IsMine)
            {
                playerInRange = false;
            }
        }
    }

    private void Update()
    {
        if (playerInRange && !isMoving && Input.GetKeyDown(interactKey))
        {
            photonView.RPC(nameof(Rpc_RequestPressButton), RpcTarget.MasterClient);

            StartCoroutine(AnimateButtonCoroutine());
        }
    }

    [PunRPC]
    private void Rpc_RequestPressButton(PhotonMessageInfo info)
    {
        if (isMoving)
        {
            return;
        }

        OnInteracted?.Invoke(info);

        photonView.RPC(nameof(Rpc_AnimateButton), RpcTarget.Others);
    }

    [PunRPC]
    private void Rpc_AnimateButton()
    {
        if (isMoving) return;
        StartCoroutine(AnimateButtonCoroutine());
    }

    private IEnumerator AnimateButtonCoroutine()
    {
        isMoving = true;

        Vector3 targetPosition = originalPosition;
        if (moveOnX)
            targetPosition += Vector3.right * moveDistance;
        else
            targetPosition += Vector3.forward * moveDistance;

        transform.position = targetPosition;

        yield return new WaitForSeconds(returnDelay);
        transform.position = originalPosition;
        isMoving = false;
    }
}