using UnityEngine;
using System.Collections;

public class ArrowAnim : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [Tooltip("Distancia en píxeles que se moverá en el eje Y.")]
    public float moveDistance = 20f;

    [Tooltip("Tiempo que tarda en ir de un punto a otro (velocidad).")]
    public float moveDuration = 0.5f;

    [Header("Configuración de Tiempos")]
    [Tooltip("Tiempo de espera antes de iniciar la animación por primera vez.")]
    public float startDelay = 1.0f;

    [Tooltip("Tiempo de espera al llegar al destino y antes de volver a empezar.")]
    public float pauseDelay = 0.5f;

    private RectTransform rectTransform;
    private Vector2 startPos;
    private Vector2 targetPos;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("ArrowAnim: Este script necesita estar en un objeto con RectTransform (UI Image).");
            enabled = false;
            return;
        }

        startPos = rectTransform.anchoredPosition;
        targetPos = startPos + new Vector2(0, moveDistance);
    }

    private void OnEnable()
    {
        // Reseteamos posición al activar por si acaso
        rectTransform.anchoredPosition = startPos;
        StartCoroutine(AnimateRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator AnimateRoutine()
    {
        // 1. Espera inicial
        yield return new WaitForSeconds(startDelay);

        while (true)
        {
            // --- FASE A: MOVER HACIA EL OBJETIVO (SUAVE) ---
            yield return MoveToPosition(startPos, targetPos);

            // --- FASE B: ESPERAR EN EL DESTINO ---
            yield return new WaitForSeconds(pauseDelay);

            // --- FASE C: TELETRANSPORTAR AL ORIGEN (INSTANTÁNEO) ---
            // En lugar de usar la corrutina MoveToPosition, asignamos directamente.
            rectTransform.anchoredPosition = startPos;

            // (Opcional) Si quieres una pequeña pausa abajo antes de arrancar de nuevo,
            // descomenta la siguiente línea. Si no, arrancará inmediatamente.
            // yield return new WaitForSeconds(pauseDelay); 
        }
    }

    // Corrutina auxiliar para mover suavemente (Solo se usa para la ida)
    private IEnumerator MoveToPosition(Vector2 from, Vector2 to)
    {
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;

            // Suavizado al inicio y final
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            rectTransform.anchoredPosition = Vector2.Lerp(from, to, smoothT);
            yield return null;
        }

        rectTransform.anchoredPosition = to;
    }
}