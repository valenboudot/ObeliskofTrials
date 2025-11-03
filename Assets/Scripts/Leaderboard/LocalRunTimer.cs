using UnityEngine;
using TMPro;
using Photon.Pun;

// Cuenta el tiempo de la corrida local (solo del jugador dueño).
public class LocalRunTimer : MonoBehaviour
{
    public static LocalRunTimer Instance { get; private set; }

    [Header("UI (opcional)")]
    [SerializeField] private TextMeshProUGUI timerText;

    private bool running;
    private double startNetworkTime;   // anclamos al reloj de Photon para consistencia
    private double pausedAt;           // por si luego querés pausar/reanudar

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (!running) return;

        double elapsed = PhotonNetwork.Time - startNetworkTime;
        if (timerText) timerText.text = FormatTime(elapsed);
    }

    public void StartRun()
    {
        running = true;
        startNetworkTime = PhotonNetwork.Time;
        pausedAt = 0;
    }

    public double StopRunAndGetElapsed()
    {
        if (!running) return 0;
        running = false;
        return PhotonNetwork.Time - startNetworkTime;
    }

    public static string FormatTime(double t)
    {
        int minutes = (int)(t / 60.0);
        double seconds = t % 60.0;
        return $"{minutes:00}:{seconds:00.000}";
    }
}
