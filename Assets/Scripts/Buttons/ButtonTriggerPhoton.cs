using UnityEngine;
using Photon.Pun;


[RequireComponent(typeof(Collider))]
public class ButtonTriggerPhoton : MonoBehaviourPun
{
    [Header("Target a eliminar (asignar en Inspector)")]
    [Tooltip("Objeto a destruir. Ideal que tenga PhotonView (Scene Object o Instantiated).")]
    public GameObject targetToDestroy;

    [Header("Condición de activación")]
    [Tooltip("Tag requerido para el objeto que presiona (ej: 'Caja'). Déjalo vacío para ignorar tag.")]
    public string triggerTag = "Caja";
    [Tooltip("Exigir que el objeto tenga Rigidbody para activarse.")]
    public bool requireRigidbody = true;
    [Tooltip("Capas válidas para presionar (0 = todas).")]
    public LayerMask validLayers = ~0;

    [Header("Comportamiento")]
    [Tooltip("Si true, se puede activar solo una vez.")]
    public bool oneShot = true;
    [Tooltip("Evitar re-disparos muy seguidos.")]
    public float cooldown = 0.25f;

    [Header("Feedback visual (opcional)")]
    [Tooltip("Mueve el botón hacia abajo al activarse.")]
    public float pressOffset = 0.05f;
    [Tooltip("Tiempo de animación de hundimiento.")]
    public float pressAnimTime = 0.08f;
    [Tooltip("Queda hundido si es oneShot; si no, vuelve a su posición.")]
    public bool stayPressedIfOneShot = true;

    private bool _pressed;
    private float _nextAllowedTime;
    private Vector3 _startPos;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true; 

        _startPos = transform.localPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (!PhotonNetwork.IsMasterClient) return;

        if (Time.time < _nextAllowedTime) return;
        if (oneShot && _pressed) return;

       
        if (requireRigidbody && other.attachedRigidbody == null) return;
        if (validLayers != 0 && (validLayers.value & (1 << other.gameObject.layer)) == 0) return;
        if (!string.IsNullOrEmpty(triggerTag) && !other.CompareTag(triggerTag)) return;

        _nextAllowedTime = Time.time + cooldown;

       
        photonView.RPC(nameof(RPC_ActivateButton), RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void RPC_ActivateButton()
    {
        if (oneShot && _pressed) return;
        _pressed = true;

        
        StopAllCoroutines();
        if (oneShot && stayPressedIfOneShot)
            StartCoroutine(AnimatePress(_startPos, _startPos + Vector3.down * pressOffset, pressAnimTime, stayDown: true));
        else
            StartCoroutine(AnimatePress(_startPos, _startPos + Vector3.down * pressOffset, pressAnimTime, stayDown: false));

        
        if (PhotonNetwork.IsMasterClient)
            TryNetworkDestroyTarget();
    }

    private System.Collections.IEnumerator AnimatePress(Vector3 from, Vector3 to, float t, bool stayDown)
    {
        float elapsed = 0f;
        while (elapsed < t)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Clamp01(elapsed / t);
            transform.localPosition = Vector3.Lerp(from, to, a);
            yield return null;
        }

        if (!stayDown && !oneShot)
        {
           
            elapsed = 0f;
            while (elapsed < t)
            {
                elapsed += Time.deltaTime;
                float a = Mathf.Clamp01(elapsed / t);
                transform.localPosition = Vector3.Lerp(to, _startPos, a);
                yield return null;
            }
            transform.localPosition = _startPos;
            _pressed = false; 
        }
    }

    private void TryNetworkDestroyTarget()
    {
        if (targetToDestroy == null) return;

        // Si el target tiene PhotonView -> destrucción de red correcta
        PhotonView targetPv = targetToDestroy.GetComponent<PhotonView>();
        if (targetPv != null)
        {
            // Si es Scene Object, también funciona con PhotonNetwork.Destroy.
            PhotonNetwork.Destroy(targetPv);
        }
        else
        {
            // Sin PhotonView: destruir localmente en todos (RPC).
            // Nota: esto puede des-sincronizar estados futuros; lo ideal es poner un PhotonView.
            photonView.RPC(nameof(RPC_LocalDestroyTargetByPath), RpcTarget.AllBuffered, targetToDestroy.name);
        }
    }

    // Método de respaldo si el objeto no tiene PhotonView (no ideal para objetos críticos).
    [PunRPC]
    private void RPC_LocalDestroyTargetByPath(string objName)
    {
        if (targetToDestroy != null)
        {
            Destroy(targetToDestroy);
            targetToDestroy = null;
            return;
        }

        // fallback: intenta encontrar por nombre en escena (no 100% seguro)
        var obj = GameObject.Find(objName);
        if (obj != null) Destroy(obj);
    }
}
