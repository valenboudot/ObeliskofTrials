using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;

public class LocalRunTimer : MonoBehaviour
{
    public static LocalRunTimer Instance { get; private set; }

    [Header("UI (opcional)")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Auto-binding")]
    [SerializeField] private string timerTextName = "LeaderboardText"; 
    [SerializeField] private string timerTextTag = "TimerText"; 

    private bool running;
    private double startNetworkTime;
    private double pausedAt;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        TryResolveText();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        timerText = null;
        TryResolveText();
    }

    private void TryResolveText()
    {
        if (timerText != null) return;

        if (!string.IsNullOrEmpty(timerTextTag))
        {
            var goByTag = SafeFindByTag(timerTextTag);
            if (goByTag && goByTag.TryGetComponent(out TextMeshProUGUI tmp1))
            {
                timerText = tmp1;
                return;
            }
        }

        if (!string.IsNullOrEmpty(timerTextName))
        {
            var goByName = GameObject.Find(timerTextName);
            if (goByName && goByName.TryGetComponent(out TextMeshProUGUI tmp2))
            {
                timerText = tmp2;
                return;
            }
        }

        var allTmps = Object.FindObjectsByType<TextMeshProUGUI>(
                 FindObjectsInactive.Include, 
                 FindObjectsSortMode.None);
        foreach (var t in allTmps)
        {
            string n = t.gameObject.name.ToLower();
            if (n.Contains("timer") || n.Contains("leader"))
            {
                timerText = t;
                return;
            }
        }

        if (allTmps.Length > 0)
            timerText = allTmps[0];
    }

    private GameObject SafeFindByTag(string tag)
    {
        try { return GameObject.FindGameObjectWithTag(tag); }
        catch { return null; } 
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

