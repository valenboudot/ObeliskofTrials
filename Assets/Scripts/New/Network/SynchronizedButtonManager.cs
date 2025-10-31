using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections;

public class SynchronizedButtonManager : MonoBehaviourPunCallbacks
{
    [Header("Configuración del Puzzle")]
    public float timeWindow = 1.0f;

    private const string ACTION_TIMESTAMP_KEY = "PuzzleButtonTime";

    //[Header("Feedback Visual (Opcional)")]
    //public GameObject button1_Indicator_Pressed;
    //public GameObject button2_Indicator_Pressed;

    private Coroutine resetCoroutine;

    public void OnActionButtonPressed(int buttonID)
    {
        double networkTime = PhotonNetwork.Time;

        var roomProps = PhotonNetwork.CurrentRoom.CustomProperties;
        double storedTime = 0.0;

        if (roomProps.ContainsKey(ACTION_TIMESTAMP_KEY))
        {
            storedTime = (double)roomProps[ACTION_TIMESTAMP_KEY];
        }

        if (storedTime == 0.0)
        {
            SetRoomProperty(ACTION_TIMESTAMP_KEY, networkTime);
            resetCoroutine = StartCoroutine(ResetActionDelay(networkTime));
        }
        else
        {
            double timeDifference = networkTime - storedTime;

            if (timeDifference > 0 && timeDifference <= timeWindow)
            {
                if (resetCoroutine != null)
                {
                    StopCoroutine(resetCoroutine);
                    resetCoroutine = null;
                }

                TriggerSuccessEvent();
                SetRoomProperty(ACTION_TIMESTAMP_KEY, 0.0);
            }
            else if (timeDifference > timeWindow)
            {
                SetRoomProperty(ACTION_TIMESTAMP_KEY, networkTime);
                resetCoroutine = StartCoroutine(ResetActionDelay(networkTime));
            }
        }
    }

    private IEnumerator ResetActionDelay(double originalPressTime)
    {
        yield return new WaitForSeconds(timeWindow + 0.1f);

        if (PhotonNetwork.IsMasterClient)
        {
            var roomProps = PhotonNetwork.CurrentRoom.CustomProperties;
            if (roomProps.ContainsKey(ACTION_TIMESTAMP_KEY) && (double)roomProps[ACTION_TIMESTAMP_KEY] == originalPressTime)
            {
                SetRoomProperty(ACTION_TIMESTAMP_KEY, 0.0);
            }
        }
        resetCoroutine = null;
    }

    private void TriggerSuccessEvent()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Debug.Log("ABRIENDO LA PUERTA (RPC)");
        // photonView.RPC("Rpc_OpenGate", RpcTarget.All);
    }

    private void SetRoomProperty(string key, object value)
    {
        ExitGames.Client.Photon.Hashtable prop = new ExitGames.Client.Photon.Hashtable { { key, value } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(prop);
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(ACTION_TIMESTAMP_KEY))
        {
            double newTime = (double)propertiesThatChanged[ACTION_TIMESTAMP_KEY];
            bool isPressed = (newTime > 0.0);

            //if (button1_Indicator_Pressed != null)
            //    button1_Indicator_Pressed.SetActive(isPressed);

            //if (button2_Indicator_Pressed != null)
            //    button2_Indicator_Pressed.SetActive(isPressed);
        }
    }
}