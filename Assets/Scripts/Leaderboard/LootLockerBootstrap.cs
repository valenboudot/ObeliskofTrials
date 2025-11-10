using LootLocker;
using LootLocker.Requests;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootLockerBootstrap : MonoBehaviour
{
    public static bool SessionStarted { get; private set; }

    [SerializeField] string playerIdentifier = "";

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        LootLockerConfig.current.allowTokenRefresh = false;
        StartGuest();
    }

    void StartGuest()
    {
        LootLockerSDKManager.StartGuestSession(playerIdentifier, response =>
        {
            if (!response.success)
            {
                Debug.LogError($"[LootLocker] Guest login FAILED. status={response.statusCode}");
                return;
            }
            SessionStarted = true;
            Debug.Log($"[LootLocker] Guest login OK! PlayerID={response.player_id}");
        });
    }

    void OnApplicationQuit()
    {
        if (LootLockerBootstrap.SessionStarted)
            LootLocker.Requests.LootLockerSDKManager.EndSession(_ => { });
    }
}

