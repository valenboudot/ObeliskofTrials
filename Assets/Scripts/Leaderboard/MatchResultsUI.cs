using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class MatchResultsUI : MonoBehaviourPunCallbacks
{
    [SerializeField] TextMeshProUGUI resultsText;  
    readonly Dictionary<int, double> results = new();

    void Start()
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.CustomProperties != null)
            LoadExisting();
        Redraw();
    }

    void LoadExisting()
    {
        foreach (var kv in PhotonNetwork.CurrentRoom.CustomProperties)
        {
            if (kv.Key is string k && k.StartsWith("finish_") && kv.Value is double elapsed)
            {
                if (int.TryParse(k.Substring("finish_".Length), out var actor))
                    results[actor] = elapsed;
            }
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        foreach (var key in propertiesThatChanged.Keys)
        {
            if (key is string k && k.StartsWith("finish_"))
            {
                if (propertiesThatChanged[key] is double elapsed &&
                    int.TryParse(k.Substring("finish_".Length), out var actor))
                {
                    results[actor] = elapsed;
                }
            }
        }
        Redraw();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        results.Remove(otherPlayer.ActorNumber);
        Redraw();
    }

    void Redraw()
    {
        if (!resultsText) return;

        if (results.Count == 0)
        {
            resultsText.text = "Resultados de la partida:\n—";
            return;
        }

        var lines = results
            .OrderBy(kv => kv.Value)
            .Select((kv, idx) =>
            {
                var p = PhotonNetwork.CurrentRoom?.GetPlayer(kv.Key);
                string name = p != null && !string.IsNullOrEmpty(p.NickName)
                              ? p.NickName
                              : $"Player {kv.Key}";
                string t = LocalRunTimer.FormatTime(kv.Value);
                return $"{idx + 1,2}. {name,-16} {t,9}";
            });

        resultsText.text = "Resultados de la partida:\n" + string.Join("\n", lines);
    }
}
