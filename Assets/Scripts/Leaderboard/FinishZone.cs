using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Collider))]
public class FinishZone : MonoBehaviour
{
    [SerializeField] string leaderboardKey = "global_highscore";
    [SerializeField] bool submitToGlobal = true; // <-- deja en false para no subir
    bool consumed;

    void OnTriggerEnter(Collider other)
    {
        if (consumed) return;
        var view = other.GetComponentInParent<Photon.Pun.PhotonView>();
        if (view == null || !view.IsMine) return;

        consumed = true;

        double elapsed = LocalRunTimer.Instance?.StopRunAndGetElapsed() ?? 0;

        // 1) Broadcast en vivo (lo que sí querés ver)
        FinishBroadcaster.BroadcastFinish(elapsed);

        // 2) (Opcional) Subir a LootLocker, pero sin refrescar nada de UI
        if (submitToGlobal && LootLockerBootstrap.SessionStarted)
        {
            int ms = Mathf.RoundToInt((float)(elapsed * 1000.0));
            int score = -ms;
            LeaderboardService.SubmitScore(score, leaderboardKey, ok =>
            {
                if (ok)
                {
                    Debug.Log("[Leaderboard] Tiempo subido (silencioso).");
                    LeaderboardEvents.OnScoreSubmitted?.Invoke();
                }
                
            });
        }
    }
}
//using UnityEngine;
 //using Photon.Pun;

//[RequireComponent(typeof(Collider))]
//public class FinishZone : MonoBehaviour
//{
//    [SerializeField] private string leaderboardKey = "global_besttime";

//    private void OnTriggerEnter(Collider other)
//    {
//        var view = other.GetComponentInParent<PhotonView>();
//        if (view == null || !view.IsMine) return; // solo el dueño del player dispara

//        // 1) Parar timer
//        double elapsed = LocalRunTimer.Instance?.StopRunAndGetElapsed() ?? 0;

//        // 2) Convertir a "score" (negativo en milisegundos)
//        int ms = Mathf.RoundToInt((float)(elapsed * 1000.0));
//        int score = -ms; // menor tiempo -> score más alto

//        // 3) Subir a LootLocker
//        LeaderboardService.SubmitScore(score, leaderboardKey, ok =>
//        {
//            if (ok)
//                Debug.Log($"[Leaderboard] Tiempo subido: {LocalRunTimer.FormatTime(elapsed)} ({score})");
//            else
//                Debug.LogError("[Leaderboard] Error al subir el tiempo");
//        });
//    }
//}