using UnityEngine;
using System;
using System.Collections;
using Photon.Pun;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider))]
public class ButtonSyncTrigger : MonoBehaviourPun
{
    [Header("Configuración de Interacción")]
    public KeyCode interactKey = KeyCode.E;

    [Header("Configuración de Animación")]
    public string pressAnimationName = "ButtonPress";
    public string returnAnimationName = "ButtonReturn";
    public float pressedHoldTime = 0.5f;

    private Animator animator;

    private bool playerInRange = false;
    private bool isMoving = false;
    public event Action<PhotonMessageInfo> OnInteracted;

    private void Start()
    {
        animator = GetComponent<Animator>();
        GetComponent<BoxCollider>().isTrigger = true;
    }

    #region Triggers de Proximidad
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
    #endregion

    #region Lógica de Red
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
        photonView.RPC(nameof(Rpc_AnimateButton), RpcTarget.All);
    }

    [PunRPC]
    private void Rpc_AnimateButton()
    {
        if (isMoving) return;
        StartCoroutine(AnimateButtonCoroutine());
    }
    #endregion

    private IEnumerator AnimateButtonCoroutine()
    {
        isMoving = true;

        if (animator != null)
        {
            animator.Play(pressAnimationName, 0, 0.0f);
        }
        else
        {
            Debug.LogError("¡Animator no encontrado en el botón!");
        }

        yield return new WaitForSeconds(pressedHoldTime);

        if (animator != null)
        {
            animator.Play(returnAnimationName, 0, 0.0f);
        }
        else
        {
            Debug.LogError("¡Animator no encontrado en el botón!");
        }

        yield return new WaitForSeconds(pressedHoldTime);

        isMoving = false;
    }
}