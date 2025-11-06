using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

[DisallowMultipleComponent]
public class CrosshairUI : MonoBehaviourPun
{
    [Header("Apariencia")]
    public Color color = Color.white;
    [Tooltip("Grosor de las líneas (px)")]
    [Range(1f, 8f)] public float thickness = 2f;
    [Tooltip("Largo de cada segmento (px)")]
    [Range(6f, 40f)] public float segmentLength = 12f;
    [Tooltip("Separación en el centro (0 = cruz sólida)")]
    [Range(0f, 20f)] public float gap = 0f;

    [Header("Orden")]
    [Tooltip("Orden de dibujo en la capa de UI (mayor = encima)")]
    public int sortingOrder = 9999;

    [Header("Escala")]
    public Vector2 referenceResolution = new Vector2(1920, 1080);

    
    private static GameObject s_canvasInstance;

    private void Start()
    {
        
        if (!photonView.IsMine) return;

        if (s_canvasInstance == null)
        {
            s_canvasInstance = CreateCanvas();
            DontDestroyOnLoad(s_canvasInstance);
        }

      
        CreateCrosshair(s_canvasInstance.transform);
    }

    private GameObject CreateCanvas()
    {
        var canvasGO = new GameObject("HUD_Canvas_Local");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>(); 
        return canvasGO;
    }

    private void CreateCrosshair(Transform parent)
    {
          
        var root = new GameObject("Crosshair");
        var rt = root.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;

        if (gap <= 0.01f)
        {
            
            CreateImage(rt, new Vector2(segmentLength, thickness), Vector2.zero);
            CreateImage(rt, new Vector2(thickness, segmentLength), Vector2.zero);
        }
        else
        {
            
            float half = (gap * 0.5f) + (segmentLength * 0.5f);

           
            CreateImage(rt, new Vector2(segmentLength, thickness), new Vector2(-half, 0f)); 
            CreateImage(rt, new Vector2(segmentLength, thickness), new Vector2(half, 0f)); 

           
            CreateImage(rt, new Vector2(thickness, segmentLength), new Vector2(0f, half)); 
            CreateImage(rt, new Vector2(thickness, segmentLength), new Vector2(0f, -half)); 
        }
    }

    private RectTransform CreateImage(Transform parent, Vector2 size, Vector2 pos)
    {
        var go = new GameObject("img");
        var rt = go.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        var img = go.AddComponent<Image>();
        img.raycastTarget = false;
        img.color = color;
        img.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");

        return rt;
    }
}
