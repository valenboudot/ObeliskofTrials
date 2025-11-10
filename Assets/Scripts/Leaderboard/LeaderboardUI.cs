using LootLocker.Requests;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class LeaderboardUI : MonoBehaviour
{
    [SerializeField] string leaderboardKey = "global_highscore";
    [SerializeField] int count = 10;
    [SerializeField] TMPro.TextMeshProUGUI tableText;

    public void Refresh()
    {
        if (!LootLockerBootstrap.SessionStarted)
        {
            tableText.text = "Logueando...";
            return;
        }

        LootLockerSDKManager.GetScoreList(leaderboardKey, count, 0, response =>
        {
            if (!response.success)
            {
                tableText.text = "Error...";
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Rank Name              Score");
            sb.AppendLine("-----------------------------");

            var items = response.items;

            if (items == null || items.Length == 0)
            {
                sb.AppendLine("No se registro nada todavia");
            }
            else
            {
                foreach (var item in items)
                {

                    int msNeg = item.score;  
                    int ms = Mathf.Abs(msNeg);
                    double seconds = ms / 1000.0;
                    string timeStr = LocalRunTimer.FormatTime(seconds);

                    string name = string.IsNullOrEmpty(item.player.name)
                        ? "Player " + item.player.id
                        : item.player.name;

                    sb.AppendLine($"{item.rank,4}  {name,-16} {timeStr,9}");
                }
            }

            tableText.text = sb.ToString();
        });
    }

    public void OnSubmitScoreTMP(TMPro.TMP_InputField scoreInput)
    {
        if (int.TryParse(scoreInput.text, out var score))
        {
            LeaderboardService.SubmitScore(score, leaderboardKey, _ => Refresh());
        }
    }

    public void OnSetNameTMP(TMPro.TMP_InputField nameInput)
    {
        PlayerNameHelper.SetPlayerName(nameInput.text);
    }

    void OnEnable() { StartCoroutine(AutoRefreshWhenReady()); }

    System.Collections.IEnumerator AutoRefreshWhenReady()
    {
        while (!LootLockerBootstrap.SessionStarted) yield return null;
        Refresh();
    }
}
