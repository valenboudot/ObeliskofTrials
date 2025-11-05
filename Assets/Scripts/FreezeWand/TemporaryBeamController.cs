using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TemporaryBeamController : MonoBehaviour
{
    // Estos serán asignados por el RPC
    public Transform targetA;
    public Transform targetB;
    public float lifeTime = 1.0f; // Cuánto tiempo vivirá el rayo

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // Configurar el LineRenderer para tener solo 2 puntos
        lineRenderer.positionCount = 2;

        // Destruir este objeto (el rayo) después de 'lifeTime' segundos
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Si por alguna razón los objetivos desaparecen (desconexión, etc.)
        if (targetA == null || targetB == null)
        {
            Destroy(gameObject); // Destruirse inmediatamente
            return;
        }

        // Actualizar las posiciones del LineRenderer para que siga a los jugadores
        lineRenderer.SetPosition(0, targetA.position);
        lineRenderer.SetPosition(1, targetB.position);
    }
}