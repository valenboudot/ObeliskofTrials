using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(PhotonView))]
public class SceneChange : MonoBehaviourPun
{
    [Header("Destino")]
    public string sceneToLoad = "TowerEntrance";

    [Header("Filtro")]
    public string triggerTag = "Player";

    private bool isLoading = false;

    private void Start()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isLoading) return;

        if (!other.CompareTag(triggerTag)) return;

        PhotonView playerView = other.GetComponent<PhotonView>();
        if (playerView != null && playerView.IsMine)
        {
            Debug.Log("¡Jugador local entró al portal! Pidiendo al Master que cargue la escena: " + sceneToLoad);

            photonView.RPC(nameof(RPC_MasterLoadScene), RpcTarget.MasterClient, sceneToLoad);

            isLoading = true;
        }
    }

    [PunRPC]
    private void RPC_MasterLoadScene(string sceneName)
    {
        if (isLoading) return;

        isLoading = true;

        PhotonNetwork.LoadLevel(sceneName);
    }
}
