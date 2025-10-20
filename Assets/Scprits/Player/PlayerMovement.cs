using UnityEngine;
using Photon.Pun;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 10f;

    public float maxSpeed = 8f;
    public float accelerationDrag = 0.5f;
    public float decelerationDrag = 15f;

    [Header("Configuración de Salto")]
    public float jumpForce = 5f;
    public LayerMask groundLayer;
    public Transform groundCheck; 
    public float groundDistance = 0.4f;

    private bool isGrounded;

    private Rigidbody rb;
    private PhotonView pv;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pv = GetComponent<PhotonView>();

        if (rb == null || pv == null)
        {
            Debug.LogError("Faltan componentes Rigidbody o PhotonView.");
            enabled = false;
        }

        rb.drag = accelerationDrag;
    }

    void Update()
    {
        if (pv.IsMine)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }
    }

    void FixedUpdate()
    {
        if (!pv.IsMine)
        {
            return;
        }

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 localMoveDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;
        Vector3 worldMoveDirection = transform.TransformDirection(localMoveDirection);


        if (localMoveDirection.magnitude > 0.1f)
        {
            Vector3 targetVelocity = worldMoveDirection * moveSpeed;

            rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);
        }
        else
        {
            if (isGrounded)
            {
                rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            }
        }

        Vector3 flatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatVelocity.magnitude > maxSpeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * maxSpeed;
            rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
        }
    }
}