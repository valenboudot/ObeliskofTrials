using System.Collections;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class JumpBuff : MonoBehaviourPun
{
    [Header("Refs")]
    [SerializeField] private PlayerController playerController;

    [Header("Debug")]
    [SerializeField] private bool showLogs = true;

    private float _baseJumpHeight;
    private Coroutine _buffCoro;

    private void Awake()
    {
        if (!playerController) playerController = GetComponent<PlayerController>();
        _baseJumpHeight = playerController != null ? playerController.jumpHeight : 1.5f;
    }

    [PunRPC]
    public void RPC_StartJumpBuff(double startTime, float duration, float multiplier)
    {
        if (playerController == null) return;

        
        double elapsed = PhotonNetwork.Time - startTime;
        float remaining = Mathf.Max(0f, duration - (float)elapsed);
        if (remaining <= 0f) return;

        if (_buffCoro != null) StopCoroutine(_buffCoro);
        _buffCoro = StartCoroutine(BuffRoutine(remaining, multiplier));
    }

    private IEnumerator BuffRoutine(float duration, float mult)
    {
        
        playerController.jumpHeight = _baseJumpHeight * mult;
        if (showLogs) Debug.Log($"[JumpBuff] ↑ Buff ON ({mult}x) por {duration:0.0}s");

        
        yield return new WaitForSeconds(duration);

        
        playerController.jumpHeight = _baseJumpHeight;
        if (showLogs) Debug.Log("[JumpBuff] ↓ Buff OFF");

        _buffCoro = null;
    }
}
