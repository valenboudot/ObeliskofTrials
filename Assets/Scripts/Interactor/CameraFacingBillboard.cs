using UnityEngine;

public class CameraFacingBillboard : MonoBehaviour
{
    private Camera mainCamera;
    private Vector3 startLocalPos;
    private float randomOffset;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        transform.rotation = mainCamera.transform.rotation;
    }
}