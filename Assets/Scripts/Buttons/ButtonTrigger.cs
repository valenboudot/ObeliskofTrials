using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class ButtonTrigger : MonoBehaviourPun
{
    [Header("Objetivo a Mover (con PhotonView)")]
    public GameObject targetToMove;

    [Header("Configuración de Movimiento")]
    public Vector3 moveDirection = Vector3.up;
    public float moveDistance = 2f;
    public float moveSpeed = 1.5f;

    [Header("Condición de Activación (Placa de Presión)")]
    public List<string> triggerTags = new List<string>();
    public bool requireRigidbody = true;
    public float cooldown = 0.1f;

    [Header("Feedback Visual del Botón")]
    public float pressOffset = 0.05f;
    public float pressAnimTime = 0.1f;

    private int _activatorCount = 0;
    private bool _isPressed = false;
    private float _nextAllowedTime;

    private Vector3 _buttonStartPosition;
    private Vector3 _targetStartPosition;
    private Vector3 _targetEndPosition;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        _buttonStartPosition = transform.localPosition;

        if (targetToMove == null)
        {
            return;
        }

        _targetStartPosition = targetToMove.transform.position;
        _targetEndPosition = _targetStartPosition + (moveDirection.normalized * moveDistance);

        if (targetToMove.GetComponent<PhotonView>() == null ||
            (targetToMove.GetComponent<PhotonTransformView>() == null && targetToMove.GetComponent<PhotonTransformViewClassic>() == null))
        {
            Debug.LogError("¡El " + targetToMove.name + " tiene que tener un PhotonView y un PhotonTransformView para funcionar", targetToMove);
        }
    }

    private bool IsValidActivator(Collider other)
    {
        if (Time.time < _nextAllowedTime) return false;
        if (requireRigidbody && other.attachedRigidbody == null) return false;

        if (!IsValidTag(other.tag)) return false;

        return true;
    }

    private bool IsValidTag(string objectTag)
    {
        if (triggerTags.Count == 0) return true;

        return triggerTags.Contains(objectTag);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!IsValidActivator(other)) return;

        _activatorCount++;

        if (_activatorCount == 1)
        {
            _nextAllowedTime = Time.time + cooldown;
            photonView.RPC(nameof(RPC_SetPressedState), RpcTarget.AllBuffered, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (requireRigidbody && other.attachedRigidbody == null) return;

        if (!IsValidTag(other.tag)) return;

        _activatorCount--;

        if (_activatorCount <= 0)
        {
            _activatorCount = 0;
            _nextAllowedTime = Time.time + cooldown;
            photonView.RPC(nameof(RPC_SetPressedState), RpcTarget.AllBuffered, false);
        }
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (targetToMove == null) return;

        Vector3 targetPosition = _isPressed ? _targetEndPosition : _targetStartPosition;

        targetToMove.transform.position = Vector3.MoveTowards(
            targetToMove.transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }

    [PunRPC]
    private void RPC_SetPressedState(bool pressed)
    {
        _isPressed = pressed;

        StopAllCoroutines();
        if (pressed)
        {
            StartCoroutine(AnimatePress(_buttonStartPosition + Vector3.down * pressOffset));
        }
        else
        {
            StartCoroutine(AnimatePress(_buttonStartPosition));
        }
    }

    private IEnumerator AnimatePress(Vector3 toPosition)
    {
        Vector3 fromPosition = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < pressAnimTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / pressAnimTime);
            transform.localPosition = Vector3.Lerp(fromPosition, toPosition, t);
            yield return null;
        }

        transform.localPosition = toPosition;
    }
}