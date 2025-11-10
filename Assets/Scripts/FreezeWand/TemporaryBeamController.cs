using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TemporaryBeamController : MonoBehaviour
{
    public Transform targetA;
    public Transform targetB;
    public float lifeTime = 1.0f;

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 2;

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (targetA == null || targetB == null)
        {
            Destroy(gameObject);
            return;
        }

        lineRenderer.SetPosition(0, targetA.position);
        lineRenderer.SetPosition(1, targetB.position);
    }
}