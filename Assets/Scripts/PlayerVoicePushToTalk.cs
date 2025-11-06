using Photon.Pun;
using Photon.Voice.Unity;     
using Photon.Voice.PUN;      
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class PlayerVoicePushToTalk : MonoBehaviourPun
{
    public KeyCode pushToTalkKey = KeyCode.V;
    public KeyCode toggleMuteKey = KeyCode.M;
    public bool force2DWhileTesting = true;
    public bool enableDebugEcho = true;

    Recorder recorder;
    Speaker speaker;
    AudioSource audioOut;
    PhotonVoiceView voiceView;
    bool hardMuted;

    void Awake()
    {
        voiceView = GetComponent<PhotonVoiceView>() ?? gameObject.AddComponent<PhotonVoiceView>();
        recorder = GetComponent<Recorder>() ?? gameObject.AddComponent<Recorder>();
        speaker = GetComponent<Speaker>() ?? gameObject.AddComponent<Speaker>();
        audioOut = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        audioOut.playOnAwake = false;
        audioOut.spatialBlend = force2DWhileTesting ? 0f : 1f; 
        audioOut.rolloffMode = AudioRolloffMode.Logarithmic;
        audioOut.minDistance = 1.5f;
        audioOut.maxDistance = 20f;

        recorder.SourceType = Recorder.InputSourceType.Microphone;
        recorder.MicrophoneType = Recorder.MicType.Unity;
        recorder.VoiceDetection = false;     
        recorder.TransmitEnabled = false;     
    }

    void OnEnable() { ApplyOwnerConfig(); }
    void Start() { ApplyOwnerConfig(); } 

    void ApplyOwnerConfig()
    {
        if (!recorder) return;
       
        recorder.RecordingEnabled = photonView.IsMine;
        recorder.DebugEchoMode = photonView.IsMine && enableDebugEcho;
    }

    void Update()
    {
        if (!photonView.IsMine || recorder == null) return;

        if (Input.GetKeyDown(toggleMuteKey))
            hardMuted = !hardMuted;

        recorder.TransmitEnabled = !hardMuted && Input.GetKey(pushToTalkKey);
    }

    void OnGUI()
    {
        if (!photonView.IsMine || recorder == null) return;
        GUILayout.BeginArea(new Rect(10, 10, 380, 110), GUI.skin.box);
        GUILayout.Label($"[Voice] RecordingEnabled: {recorder.RecordingEnabled}");
        GUILayout.Label($"[Voice] TransmitEnabled: {recorder.TransmitEnabled}");
        GUILayout.Label($"[Voice] IsCurrentlyTransmitting: {recorder.IsCurrentlyTransmitting}");
        GUILayout.Label($"[Voice] HardMuted: {hardMuted}");
        GUILayout.Label($"[Voice] DebugEcho: {recorder.DebugEchoMode}");
        GUILayout.EndArea();
    }
}
