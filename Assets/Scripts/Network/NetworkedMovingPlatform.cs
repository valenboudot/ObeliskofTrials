using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(PhotonTransformView))]
public class NetworkedMovingPlatform : MonoBehaviourPun
{
    [Header("Configuración de Movimiento")]
    public Transform pointA;
    public Transform pointB;
    public float speed = 2.0f;

    private Transform currentTarget;
    private Vector3 lastPosition;

    public Vector3 MovementDelta { get; private set; }

    void Start()
    {
        currentTarget = pointB;
        lastPosition = transform.position;
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            currentTarget.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, currentTarget.position) < 0.1f)
        {
            currentTarget = (currentTarget == pointA) ? pointB : pointA;
        }
    }

    void LateUpdate()
    {
        MovementDelta = transform.position - lastPosition;

        lastPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView playerPV = other.GetComponent<PhotonView>();
            if (playerPV != null && playerPV.IsMine)
            {
                var playerScript = other.GetComponent<PlayerController>();
                if (playerScript != null)
                {
                    playerScript.SetCurrentPlatform(this);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView playerPV = other.GetComponent<PhotonView>();
            if (playerPV != null && playerPV.IsMine)
            {
                var playerScript = other.GetComponent<PlayerController>();
                if (playerScript != null)
                {
                    playerScript.ClearCurrentPlatform(this);
                }
            }
        }
    }
}