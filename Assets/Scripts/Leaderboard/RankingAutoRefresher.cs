using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LeaderboardUI))]
public class RankingAutoRefresher : MonoBehaviour
{
    [SerializeField] float firstDelay = 0.2f;  // pequeño respiro al habilitar
    [SerializeField] float refreshEvery = 5f;  // intervalo de actualización
    [SerializeField] float maxMinutes = 5f;    // por si lo dejan abierto

    LeaderboardUI ui;
    Coroutine loop;

    void Awake() => ui = GetComponent<LeaderboardUI>();

    void OnEnable() => loop = StartCoroutine(Loop());
    void OnDisable() { if (loop != null) StopCoroutine(loop); }

    IEnumerator Loop()
    {
        // esperar login
        while (!LootLockerBootstrap.SessionStarted) yield return null;

        // refresco inicial
        yield return new WaitForSeconds(firstDelay);
        ui.Refresh();

        // refrescar cada X segundos mientras el panel esté activo
        float endAt = Time.time + maxMinutes * 60f;
        while (isActiveAndEnabled && Time.time < endAt)
        {
            yield return new WaitForSeconds(refreshEvery);
            if (!LootLockerBootstrap.SessionStarted) continue;
            ui.Refresh();
        }
    }
}
