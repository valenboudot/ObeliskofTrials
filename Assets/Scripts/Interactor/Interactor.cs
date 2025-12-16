using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

interface IInteractable
{
    public void Interact();
}

public class Interactor : MonoBehaviourPun
{
    public Transform interactorSource;
    public float interactRange;
    public KeyCode interactKey = KeyCode.E;

    void Start()
    {

    }

    void Update()
    {
        if (!photonView.IsMine) return;

        if (Input.GetKeyDown(interactKey))
        {
            Ray r = new Ray(interactorSource.position, interactorSource.forward);
            if (Physics.Raycast(r, out RaycastHit hitInfo, interactRange))
            {
                if (hitInfo.collider.gameObject.TryGetComponent(out IInteractable interactObj))
                {
                    interactObj.Interact();
                }
            }
        }
    }
}