using UnityEngine;
using Photon.Pun;

public class PlayerRaycaster : MonoBehaviourPun
{
    [Header("Configuración del Rayo")]
    public float rayDistance = 5f;
    public LayerMask interactableLayer = ~0;

    [Header("Input")]
    public KeyCode interactKey = KeyCode.E; // Tecla para interactuar

    // Guardamos el objeto que estamos mirando actualmente
    private InteractableMessage currentTarget;

    private void Update()
    {
        if (!photonView.IsMine) return;

        HandleRaycast();
        HandleInput();
    }

    private void HandleRaycast()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // 1. Lanzamos Rayo
        if (Physics.Raycast(ray, out hit, rayDistance, interactableLayer))
        {
            // Intentamos obtener el script
            InteractableMessage hitInteractable = hit.collider.GetComponent<InteractableMessage>();

            // CASO A: Estamos mirando un objeto interactuable nuevo
            if (hitInteractable != null)
            {
                if (currentTarget != hitInteractable)
                {
                    // Si mirábamos otro antes, le decimos "adiós"
                    if (currentTarget != null) currentTarget.SetHover(false);

                    // Al nuevo le decimos "hola"
                    currentTarget = hitInteractable;
                    currentTarget.SetHover(true);
                }
            }
            // CASO B: Miramos algo, pero no es interactuable (ej. una pared)
            else
            {
                ClearCurrentTarget();
            }
        }
        // CASO C: No miramos nada (aire)
        else
        {
            ClearCurrentTarget();
        }
    }

    private void HandleInput()
    {
        // Si tenemos un objetivo y presionamos la tecla...
        if (currentTarget != null && Input.GetKeyDown(interactKey))
        {
            currentTarget.TryInteract();
        }
    }

    // Helper para limpiar el objetivo actual
    private void ClearCurrentTarget()
    {
        if (currentTarget != null)
        {
            currentTarget.SetHover(false); // Apagar el aviso del anterior
            currentTarget = null;
        }
    }
}