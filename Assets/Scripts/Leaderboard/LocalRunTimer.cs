using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;

// Cuenta el tiempo de la corrida local (solo del jugador dueño).
public class LocalRunTimer : MonoBehaviour
{
    public static LocalRunTimer Instance { get; private set; }

    [Header("UI (opcional)")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Auto-binding")]
    [SerializeField] private string timerTextName = "LeaderboardText"; // nombre esperado
    [SerializeField] private string timerTextTag = "TimerText";       // tag opcional

    private bool running;
    private double startNetworkTime;
    private double pausedAt;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Reenganchar al cargar escenas nuevas
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Intento inicial por si ya estoy en una escena con el TMP
        TryResolveText();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Cuando cambia la escena, el viejo TMP ya no existe: resolvelo de nuevo
        timerText = null;
        TryResolveText();
    }

    private void TryResolveText()
    {
        if (timerText != null) return;

        // 1) Por Tag (rápido y confiable si lo configurás)
        if (!string.IsNullOrEmpty(timerTextTag))
        {
            var goByTag = SafeFindByTag(timerTextTag);
            if (goByTag && goByTag.TryGetComponent(out TextMeshProUGUI tmp1))
            {
                timerText = tmp1;
                return;
            }
        }

        // 2) Por Nombre exacto
        if (!string.IsNullOrEmpty(timerTextName))
        {
            var goByName = GameObject.Find(timerTextName);
            if (goByName && goByName.TryGetComponent(out TextMeshProUGUI tmp2))
            {
                timerText = tmp2;
                return;
            }
        }

        // 3) Heurística: el primer TMP visible con "Timer" o "Leader" en el nombre
        var allTmps = Object.FindObjectsByType<TextMeshProUGUI>(
                 FindObjectsInactive.Include,   // incluye inactivos
                 FindObjectsSortMode.None); // incluye inactivos
        foreach (var t in allTmps)
        {
            string n = t.gameObject.name.ToLower();
            if (n.Contains("timer") || n.Contains("leader"))
            {
                timerText = t;
                return;
            }
        }

        // 4) Último recurso: cualquier TMP de la escena
        if (allTmps.Length > 0)
            timerText = allTmps[0];
    }

    private GameObject SafeFindByTag(string tag)
    {
        try { return GameObject.FindGameObjectWithTag(tag); }
        catch { return null; } // por si el tag no existe en el proyecto
    }

    public void SetTimerText(TextMeshProUGUI tmp) => timerText = tmp;

    void Update()
    {
        if (!running) return;
        if (timerText == null) TryResolveText();

        double elapsed = PhotonNetwork.Time - startNetworkTime;
        if (timerText) timerText.text = FormatTime(elapsed);
    }

    public void StartRun()
    {
        running = true;
        startNetworkTime = PhotonNetwork.Time;
        pausedAt = 0;
        // opcional: refrescar UI al empezar
        if (timerText) timerText.text = FormatTime(0);
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

