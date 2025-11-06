using UnityEngine;
using System.Collections;
using Photon.Pun;
using UnityEngine.UI;

public class FreezeController : MonoBehaviourPun, IPunObservable
{
    public bool ItsFreeze = false;
    public GameObject FreezeEffect;

    public Image freezeUIOverlay;

    private PlayerController playerController;

    private float originalMoveSpeed;
    private float originalGravity;
    private bool valuesStored = false;

    private Coroutine _freezeCoro;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();

        if (playerController == null)
        {
            Debug.LogError("FreezeController no se encontro un PlayerController en este GameObject.");
        }

        if (photonView.IsMine && freezeUIOverlay != null)
        {
            freezeUIOverlay.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        ApplyFreezeState();
    }

    void ApplyFreezeState()
    {
        if (playerController == null) return;

        if (ItsFreeze)
        {
            if (FreezeEffect != null)
            {
                FreezeEffect.SetActive(true);
            }
            playerController.ItsFrozen = true;

            if (photonView.IsMine && freezeUIOverlay != null)
            {
                freezeUIOverlay.gameObject.SetActive(ItsFreeze);
            }

            if (photonView.IsMine)
            {
                if (!valuesStored)
                {
                    originalMoveSpeed = playerController.moveSpeed;
                    originalGravity = playerController.gravity;
                    valuesStored = true;
                }

                playerController.moveSpeed = 0f;
                playerController.gravity = 0f;
            }
        }
        else
        {
            if (FreezeEffect != null)
            {
                FreezeEffect.SetActive(false);
            }
            playerController.ItsFrozen = false;

            if (photonView.IsMine && freezeUIOverlay != null)
            {
                freezeUIOverlay.gameObject.SetActive(ItsFreeze);
            }

            if (photonView.IsMine)
            {
                if (valuesStored)
                {
                    playerController.moveSpeed = originalMoveSpeed;
                    playerController.gravity = originalGravity;
                    valuesStored = false;
                }
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(this.ItsFreeze);
        }
        else
        {
            this.ItsFreeze = (bool)stream.ReceiveNext();
        }
    }

    [PunRPC]
    public void RPC_StartFreeze(double startTime, float duration)
    {
        double elapsed = PhotonNetwork.Time - startTime;
        float remaining = Mathf.Max(0f, duration - (float)elapsed);
        if (remaining <= 0f) return;

        if (_freezeCoro != null) StopCoroutine(_freezeCoro);

        _freezeCoro = StartCoroutine(FreezeRoutine(remaining));
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        ItsFreeze = true;

        yield return new WaitForSeconds(duration);

        ItsFreeze = false;

        _freezeCoro = null;
    }
}