using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject MainMenuPanel;
    public GameObject OptionsPanel;
    public GameObject MaxPlayersPopup;
    public GameObject LobbyPanel;
    public GameObject CreatePanel;
    public GameObject JoinPanel;
    public GameObject RankingPanel;

    public void PlayBotton()
    {
        LobbyPanel.SetActive(true);
        MainMenuPanel.SetActive(false);
    }

    public void OptionsBotton()
    {
        OptionsPanel.SetActive(true);
        MainMenuPanel.SetActive(false);
    }

    public void RankingBotton()
    {
        RankingPanel.SetActive(true);
        MainMenuPanel.SetActive(false);
    }

    public void CreateRoomBotton()
    {
        CreatePanel.SetActive(true);
        LobbyPanel.SetActive(false);
    }

    public void JoinRoomBotton()
    {
        JoinPanel.SetActive(true);
        LobbyPanel.SetActive(false);
    }

    public void OpenCreateRoomPopup()
    {
        MaxPlayersPopup.SetActive(true);
    }

    public void CloseCreateRoomPopup()
    {
        MaxPlayersPopup.SetActive(false);
    }

    public void BackBotton(int Option)
    {
        if (Option == 0)
        {
            LobbyPanel.SetActive(false);
            MainMenuPanel.SetActive(true);
        }
        else if (Option == 1)
        {
            CreatePanel.SetActive(false);
            LobbyPanel.SetActive(true);
        }
        else if (Option == 2)
        {
            JoinPanel.SetActive(false);
            LobbyPanel.SetActive(true);
        }
        else if(Option == 3)
        {
            OptionsPanel.SetActive(false);
            MainMenuPanel.SetActive(true);
        }
        else if (Option == 4)
        {
            RankingPanel.SetActive(false);
            MainMenuPanel.SetActive(true);
        }
    }

    public void QuitBotton()
    {
        Application.Quit();
    }
}
