using Photon.Pun;
using UnityEngine;

// [RequireComponent(typeof(PhotonView))]
public class IceWandInteractor : MonoBehaviourPun
{
    [Header("Estado")]
    [SerializeField] private bool hasWand;

    [Header("Freeze Settings")]
    [SerializeField] private float freezeDuration = 3f;
    [SerializeField] private float clickMaxDistance = 50f;

    [Header("Raycast")]
    [SerializeField] private Camera cam;

    [Header("Uso")]
    [SerializeField] private bool consumeOnUse = false;

    [Header("Visuals")]
    [SerializeField] private GameObject freezeBeamPrefab;
    [SerializeField] private float beamDuration = 1.0f;

    private void Start()
    {
        if (!photonView.IsMine) return;
        if (!cam) cam = Camera.main;
    }

    public bool HasWand => hasWand;

    [PunRPC]
    public void RPC_SetHasWand(bool value)
    {
        hasWand = value;
    }

    private void Update()
    {
        // Solo el jugador local puede disparar la varita
        if (!photonView.IsMine) return;
        if (!hasWand) return;

        if (Input.GetMouseButtonDown(0)) // Clic izquierdo
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return; // Salir si no hay cámara

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, clickMaxDistance))
            {
                // Comprobar si golpeamos a un objeto con PhotonView (otro jugador)
                PhotonView targetPV = hit.collider.GetComponentInParent<PhotonView>();

                // Asegurarse de que el objetivo es válido y no somos nosotros mismos
                if (targetPV != null && targetPV != photonView)
                {
                    // Obtener la hora de la red para sincronizar el inicio del efecto
                    var now = PhotonNetwork.Time;

                    targetPV.RPC("RPC_StartFreeze", RpcTarget.All, now, freezeDuration);

                    photonView.RPC("RPC_DrawFreezeBeam", RpcTarget.All, photonView.ViewID, targetPV.ViewID);

                    // Consumir la varita si está configurado
                    if (consumeOnUse)
                        photonView.RPC(nameof(RPC_SetHasWand), RpcTarget.All, false);
                }
            }
        }
    }

    [PunRPC]
    private void RPC_DrawFreezeBeam(int casterViewID, int targetViewID)
    {
        // Este código se ejecuta en la máquina de TODOS los jugadores

        // Primero, validamos que el prefab del efecto exista
        if (freezeBeamPrefab == null)
        {
            Debug.LogError("FreezeBeamPrefab no está asignado en el IceWandInteractor.");
            return;
        }

        // Encontrar los transforms del lanzador y del objetivo usando sus ViewIDs
        PhotonView casterPV = PhotonView.Find(casterViewID);
        PhotonView targetPV = PhotonView.Find(targetViewID);

        // Si ambos existen...
        if (casterPV != null && targetPV != null)
        {
            // ...Instanciar el prefab del rayo (es un objeto local)
            GameObject beamObj = Instantiate(freezeBeamPrefab, Vector3.zero, Quaternion.identity);

            // Obtener su script
            TemporaryBeamController beamScript = beamObj.GetComponent<TemporaryBeamController>();

            // Y configurarlo para que siga a los objetivos
            beamScript.targetA = casterPV.transform;
            beamScript.targetB = targetPV.transform;
            beamScript.lifeTime = beamDuration; // Usamos la duración definida
        }
    }
}
