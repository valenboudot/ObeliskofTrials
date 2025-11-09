using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LeaderboardUI))]
public class RankingView : MonoBehaviour
{
    LeaderboardUI ui;

    void Awake() => ui = GetComponent<LeaderboardUI>();

    void OnEnable()
    {
        LeaderboardEvents.OnScoreSubmitted += HandleScoreSubmitted;
        StartCoroutine(RefreshWhenReady());
    }

    void OnDisable()
    {
        LeaderboardEvents.OnScoreSubmitted -= HandleScoreSubmitted;
    }

    void HandleScoreSubmitted()
    {
        if (!LootLockerBootstrap.SessionStarted) return;
        ui.Refresh();
    }

    System.Collections.IEnumerator RefreshWhenReady()
    {
        while (!LootLockerBootstrap.SessionStarted) yield return null;
        ui.Refresh(); // al abrir la pantalla
    }
}

