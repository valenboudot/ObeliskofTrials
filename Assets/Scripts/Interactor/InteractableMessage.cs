using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(SphereCollider))]
public class InteractableMessage : MonoBehaviourPun
{
    [Header("UI / Feedback Visual")]
    public GameObject interactPrompt;

    // Estado interno
    private bool isLocalPlayerInZone = false;
    private bool isBeingLookedAt = false; // Nuevo estado

    private void Awake()
    {
        GetComponent<SphereCollider>().isTrigger = true;
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    // --- 1. LÓGICA DE VISUALIZACIÓN ---

    // Esta función combina las dos condiciones para mostrar/ocultar el mensaje
    private void UpdatePromptState()
    {
        if (interactPrompt == null) return;

        // SOLO mostramos si estamos cerca Y mirando
        bool shouldShow = isLocalPlayerInZone && isBeingLookedAt;

        if (interactPrompt.activeSelf != shouldShow)
        {
            interactPrompt.SetActive(shouldShow);
        }
    }

    // Llamado por PlayerRaycaster cuando la cámara apunta/deja de apuntar
    public void SetHover(bool state)
    {
        isBeingLookedAt = state;
        UpdatePromptState();
    }

    // --- 2. DETECCIÓN DE ZONA (Trigger) ---

    private void OnTriggerEnter(Collider other)
    {
        if (IsMyPlayer(other))
        {
            isLocalPlayerInZone = true;
            UpdatePromptState(); // Revisamos si hay que mostrar
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsMyPlayer(other))
        {
            isLocalPlayerInZone = false;
            UpdatePromptState(); // Ocultamos inmediatamente
        }
    }

    private bool IsMyPlayer(Collider other)
    {
        if (!other.CompareTag("Player")) return false;
        PhotonView pView = other.GetComponent<PhotonView>();
        return pView != null && pView.IsMine;
    }

    // --- 3. INTERACCIÓN (Lógica de Red) ---

    public void TryInteract()
    {
        // Solo permitimos interactuar si se cumplen ambas condiciones
        if (isLocalPlayerInZone && isBeingLookedAt)
        {
            photonView.RPC(nameof(RPC_DisableObject), RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void RPC_DisableObject()
    {
        if (interactPrompt != null) interactPrompt.SetActive(false);
        gameObject.SetActive(false);
    }
}