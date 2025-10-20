using UnityEngine;
using TMPro;      // Necesario para manipular el componente TextMeshPro
using Photon.Pun; // Necesario para obtener la información del jugador de Photon

public class PlayerNameDisplay : MonoBehaviour
{
    [Header("Referencia UI")]
    [Tooltip("El componente TextMeshProUGUI que mostrará el nombre.")]
    public TextMeshProUGUI nameText;

    // Referencia al PhotonView, lo obtiene del objeto padre (PlayerPrefab)
    private PhotonView pv;

    void Start()
    {
        pv = GetComponent<PhotonView>();

        if (pv == null)
        {
            Debug.LogError("PlayerNameDisplay requiere un PhotonView.");
            enabled = false;
            return;
        }

        if (nameText == null)
        {
            Debug.LogError("Falta asignar el componente NameText en el Inspector.");
            enabled = false;
            return;
        }

        // Obtener y mostrar el nombre del jugador
        DisplayPlayerName();
    }

    private void DisplayPlayerName()
    {
        // El 'Owner' del PhotonView es el jugador que lo instanció.
        // El 'NickName' se estableció antes de entrar al Lobby.
        if (pv.Owner != null)
        {
            nameText.text = pv.Owner.NickName;
        }
        else
        {
            // Caso de fallback o si el jugador aún no se ha sincronizado completamente
            nameText.text = "Jugador Desconocido";
            Debug.LogWarning("No se pudo obtener el propietario del PhotonView.");
        }
    }

    void LateUpdate()
    {
        // Opcional: Rotar el Canvas del nombre para que siempre mire a la cámara.
        // Esto previene que el texto se vea plano si la cámara rota.
        if (Camera.main != null)
        {
            // Apuntar la rotación del canvas hacia la cámara principal
            nameText.transform.parent.rotation = Camera.main.transform.rotation;
        }
    }
}