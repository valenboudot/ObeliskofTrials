using LootLocker.Requests;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardService : MonoBehaviour
{
    public static void SubmitScore(int score, string leaderboardKey, System.Action<bool> onDone = null)
    {
        LootLockerSDKManager.SubmitScore("", score, leaderboardKey, response =>
        {
            if (!response.success)
            {
                Debug.LogError($"[LootLocker] SubmitScore FAILED. status={response.statusCode} key={leaderboardKey}");
                onDone?.Invoke(false);

                return;
            }
            Debug.Log("[LootLocker] SubmitScore OK");
            onDone?.Invoke(true);
        });
    }
}
