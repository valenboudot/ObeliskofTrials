using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class ButtonTrigger : MonoBehaviourPun
{
    [Header("Objetivo a Mover (con PhotonView)")]
    [Tooltip("El objeto que se moverá (puerta, plataforma, etc.). DEBE tener PhotonView y PhotonTransformView.")]
    public GameObject targetToMove;

    [Header("Configuración de Movimiento")]
    [Tooltip("La dirección en la que se moverá el objeto (ej: (0, 1, 0) para arriba)")]
    public Vector3 moveDirection = Vector3.up;
    [Tooltip("La distancia total que se moverá el objeto en unidades de Unity")]
    public float moveDistance = 2f;
    [Tooltip("La velocidad a la que el objeto se mueve y regresa")]
    public float moveSpeed = 1.5f;

    [Header("Condición de Activación (Placa de Presión)")]
    [Tooltip("Tags requeridos para el objeto que presiona (ej: 'Caja', 'Player'). Si la lista está vacía, CUALQUIER tag es válido.")]
    public List<string> triggerTags = new List<string>();
    [Tooltip("Exigir que el objeto tenga Rigidbody para activarse.")]
    public bool requireRigidbody = true;
    [Tooltip("Evitar re-disparos muy seguidos.")]
    public float cooldown = 0.1f;

    [Header("Feedback Visual del Botón")]
    [Tooltip("Mueve el botón hacia abajo al activarse.")]
    public float pressOffset = 0.05f;
    [Tooltip("Tiempo de animación de hundimiento.")]
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
            Debug.LogError("¡'targetToMove' no está asignado en " + gameObject.name + "!", this);
            return;
        }

        _targetStartPosition = targetToMove.transform.position;
        _targetEndPosition = _targetStartPosition + (moveDirection.normalized * moveDistance);

        if (targetToMove.GetComponent<PhotonView>() == null ||
            (targetToMove.GetComponent<PhotonTransformView>() == null && targetToMove.GetComponent<PhotonTransformViewClassic>() == null))
        {
            Debug.LogError("¡El 'targetToMove' (" + targetToMove.name + ") DEBE tener un PhotonView y un PhotonTransformView (o Classic) para funcionar!", targetToMove);
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