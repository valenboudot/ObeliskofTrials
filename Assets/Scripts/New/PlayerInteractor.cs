using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class PlayerInteractor : MonoBehaviour
{
    [Header("Configuración")]
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;

    private PhotonView pv;
    private Transform cameraTransform;

    void Start()
    {
        pv = GetComponent<PhotonView>();

        if (!pv.IsMine)
        {
            enabled = false;
            return;
        }

        // Asumimos que la cámara está etiquetada como "MainCamera" y es hija
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            CheckForInteractable();
        }
    }

    private void CheckForInteractable()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            // Busca el script del botón en el objeto golpeado
            PuzzleButton button = hit.collider.GetComponent<PuzzleButton>();

            if (button != null)
            {
                // ¡Encontrado! Llama a la función de interacción del botón
                button.OnPressedByPlayer();
            }
        }
    }
}