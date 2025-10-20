using UnityEngine;
using Photon.Pun;

public class FirstPersonCamera : MonoBehaviour
{
    [Header("Configuración de Sensibilidad")]
    public float mouseSensitivity = 100f;
    public float maxVerticalAngle = 90f;

    [Header("Referencias")]
    public Transform cameraRig;

    private const float DEADZONE_THRESHOLD = 0.01f;
    private float xRotation = 0f;
    private PhotonView pv;
    private Rigidbody rb;

    void Start()
    {
        pv = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();

        if (pv.IsMine)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            enabled = false;
        }

        if (rb == null)
        {
            Debug.LogError("FirstPersonCamera requiere un Rigidbody en el objeto padre.");
            enabled = false;
        }
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        float mouseInputMagnitude = Mathf.Abs(mouseX) + Mathf.Abs(mouseY);

        if (mouseInputMagnitude > DEADZONE_THRESHOLD)
        {

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -maxVerticalAngle, maxVerticalAngle);

            cameraRig.localRotation = Quaternion.AngleAxis(xRotation, Vector3.right);

            if (rb != null)
            {
                Quaternion deltaRotation = Quaternion.Euler(Vector3.up * mouseX);

                rb.rotation *= deltaRotation;

                Vector3 eulerAngles = rb.rotation.eulerAngles;
                rb.rotation = Quaternion.Euler(0f, eulerAngles.y, 0f);
            }
        }
    }
}