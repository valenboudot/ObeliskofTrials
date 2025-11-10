using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LeaderboardUI))]
public class RankingAutoRefresher : MonoBehaviour
{
    [SerializeField] float firstDelay = 0.2f; 
    [SerializeField] float refreshEvery = 5f;  
    [SerializeField] float maxMinutes = 5f;

    LeaderboardUI ui;
    Coroutine loop;

    void Awake() => ui = GetComponent<LeaderboardUI>();

    void OnEnable() => loop = StartCoroutine(Loop());
    void OnDisable() { if (loop != null) StopCoroutine(loop); }

    IEnumerator Loop()
    {
        while (!LootLockerBootstrap.SessionStarted) yield return null;

        yield return new WaitForSeconds(firstDelay);
        ui.Refresh();

        float endAt = Time.time + maxMinutes * 60f;
        while (isActiveAndEnabled && Time.time < endAt)
        {
            yield return new WaitForSeconds(refreshEvery);
            if (!LootLockerBootstrap.SessionStarted) continue;
            ui.Refresh();
        }
    }
}
