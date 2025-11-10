using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Collider))]
public class FinishZone : MonoBehaviour
{
    [SerializeField] string leaderboardKey = "global_highscore";
    [SerializeField] bool submitToGlobal = true;
    bool consumed;

    void OnTriggerEnter(Collider other)
    {
        if (consumed) return;
        var view = other.GetComponentInParent<Photon.Pun.PhotonView>();
        if (view == null || !view.IsMine) return;

        consumed = true;

        double elapsed = LocalRunTimer.Instance?.StopRunAndGetElapsed() ?? 0;

        FinishBroadcaster.BroadcastFinish(elapsed);

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