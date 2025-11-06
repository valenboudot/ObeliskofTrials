using UnityEngine;
using System.Collections;
using Photon.Pun;

public class FreezeController : MonoBehaviourPun, IPunObservable
{
    public bool ItsFreeze = false;
    public GameObject FreezeEffect;

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
        // Calcular el tiempo restante (exactamente como JumpBuff)
        double elapsed = PhotonNetwork.Time - startTime;
        float remaining = Mathf.Max(0f, duration - (float)elapsed);
        if (remaining <= 0f) return;

        // Detener cualquier congelación anterior para reiniciar el temporizador
        if (_freezeCoro != null) StopCoroutine(_freezeCoro);

        // Iniciar la nueva corrutina de congelación
        _freezeCoro = StartCoroutine(FreezeRoutine(remaining));
    }

    // --- ¡NUEVA CORRUTINA AÑADIDA! ---
    // Esta corrutina aplica el estado y lo revierte después de 'duration'
    private IEnumerator FreezeRoutine(float duration)
    {
        // 1. Aplicar el estado de congelación
        //    Simplemente seteamos la variable. El Update() y OnPhotonSerializeView()
        //    se encargarán de aplicar la lógica y sincronizarla.
        ItsFreeze = true;

        // 2. Esperar la duración del efecto
        yield return new WaitForSeconds(duration);

        // 3. Revertir el estado
        ItsFreeze = false;

        _freezeCoro = null;
    }
}