using UnityEngine;

public class WandAnimation : MonoBehaviour
{
    public float rotationSpeed = 50f;

    public float bobSpeed = 2f;
    public float bobHeight = 0.25f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;

        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
}