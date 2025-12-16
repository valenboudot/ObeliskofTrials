using UnityEngine;
using Photon.Pun;   
using Photon.Realtime;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PhotonView))]
public class PlayerController : MonoBehaviourPunCallbacks
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.5f;
    public float acceleration = 5f;

    [Header("Salto / Gravedad")]
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;
    public float groundedStickForce = -2f;

    [Header("Mouse Look")]
    public Transform cameraHolder;
    public float mouseSensitivity = 2f;
    public float verticalLookLimit = 80f;
    public bool lockCursorOnStart = true;

    [Header("Componentes opcionales (se desactivan en remotos)")]
    public Camera playerCamera;          
    public AudioListener audioListener;  

    [Header("Colisiones con cubos (push)")]
    public LayerMask pushableLayers;
    public float pushImpulse = 3.5f;
    public AnimationCurve speedToForce = AnimationCurve.Linear(0, 0.5f, 1, 1.2f);
    public bool requestOwnershipBeforePush = true;
    [Range(0f, 1f)] public float maxVerticalNormalForPush = 0.5f;

    private CharacterController controller;
    private Animator animator;
    private Vector3 velocity;     
    private float pitch;

    private NetworkedMovingPlatform currentPlatform;

    public bool ItsFrozen = false;

    public float actualSpeed;
    private float currentVelocityMagnitude = 0f;

    public bool isMovingForward;
    public bool isMovingBackward;
    public bool isMovingRight;
    public bool isMovingLeft;

    private bool IsGrounded => controller.isGrounded;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        if (playerCamera == null && cameraHolder != null)
            playerCamera = cameraHolder.GetComponentInChildren<Camera>();
        if (audioListener == null && playerCamera != null)
            audioListener = playerCamera.GetComponent<AudioListener>();

        if (!photonView.IsMine)
        {
            if (playerCamera) playerCamera.enabled = false;
            if (audioListener) audioListener.enabled = false;
        }
    }

    private void Start()
    {
        if (!photonView.IsMine) return;

        SetCursorLock(lockCursorOnStart);

        if (cameraHolder != null)
        {
            pitch = cameraHolder.localEulerAngles.x;
            if (pitch > 180f) pitch -= 360f;
            pitch = Mathf.Clamp(pitch, -verticalLookLimit, verticalLookLimit);
            ApplyCameraPitch();
        }
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        if (!ItsFrozen)
        {
            if (animator.speed == 0) animator.speed = 1;

            HandleMouseLook();
            HandleMovement();
            HandleJump();
        }
        else
        {
            velocity = Vector3.zero;
            animator.speed = 0;
        }

        UpdateAnimator();
    }

    private void LateUpdate()
    {
        if (currentPlatform != null)
        {
            controller.Move(currentPlatform.MovementDelta);
        }
    }

    private void HandleMouseLook()
    {
        if (cameraHolder == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -verticalLookLimit, verticalLookLimit);
        ApplyCameraPitch();

        if (Input.GetKeyDown(KeyCode.Escape))
            SetCursorLock(Cursor.lockState != CursorLockMode.Locked);
    }

    private void ApplyCameraPitch()
    {
        if (!cameraHolder) return;
        Vector3 e = cameraHolder.localEulerAngles;
        e.x = pitch; e.y = 0f; e.z = 0f;
        cameraHolder.localEulerAngles = e;
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        isMovingForward = moveZ > 0.1f;
        isMovingBackward = moveZ < -0.1f;
        isMovingRight = moveX > 0.1f;
        isMovingLeft = moveX < -0.1f;

        Vector3 input = new Vector3(moveX, 0f, moveZ);
        input = Vector3.ClampMagnitude(input, 1f);

        Vector3 moveDirection = transform.TransformDirection(input);

        float targetSpeed = 0f;
        if (input.magnitude > 0.1f)
        {
            targetSpeed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f);
        }

        currentVelocityMagnitude = Mathf.Lerp(currentVelocityMagnitude, targetSpeed, acceleration * Time.deltaTime);

        actualSpeed = currentVelocityMagnitude;

        Vector3 horizontalMove = new Vector3(moveDirection.x, 0, moveDirection.z).normalized * currentVelocityMagnitude;

        if (IsGrounded && velocity.y < 0f) velocity.y = groundedStickForce;
        else velocity.y += gravity * Time.deltaTime;

        Vector3 finalMove = horizontalMove;
        finalMove.y = velocity.y;

        controller.Move(finalMove * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (IsGrounded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                animator.SetBool("Jumping", true);
                velocity.y = Mathf.Sqrt(-2f * gravity * jumpHeight);
            }
            animator.SetBool("InFloor", true);
        }
        else
        {
            Falling();
        }
    }

    private void SetCursorLock(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!photonView.IsMine) return;

        if (((1 << hit.gameObject.layer) & pushableLayers) == 0)
            return;

        if (Mathf.Abs(hit.normal.y) > maxVerticalNormalForPush)
            return;

        Rigidbody rb = hit.collider.attachedRigidbody;
        if (rb == null || rb.isKinematic) return;

        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);
        if (pushDir.sqrMagnitude < 0.0001f) return;
        pushDir.Normalize();

        float horizSpeed = new Vector3(controller.velocity.x, 0f, controller.velocity.z).magnitude;
        float speed01 = Mathf.Clamp01(horizSpeed / (moveSpeed * sprintMultiplier));
        float force = pushImpulse * speedToForce.Evaluate(speed01);

        PhotonView targetPv = rb.GetComponent<PhotonView>();
        if (targetPv != null && requestOwnershipBeforePush && !targetPv.AmOwner)
        {
            
            targetPv.RequestOwnership();
        }

        rb.AddForce(pushDir * force, ForceMode.Impulse);
    }
    
    private void OnCollisionStay(Collision collision) { }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.1f, transform.forward * 1.5f);
    }

    public void SetCurrentPlatform(NetworkedMovingPlatform platform)
    {
        currentPlatform = platform;
    }

    public void ClearCurrentPlatform(NetworkedMovingPlatform platform)
    {
        if (currentPlatform == platform)
        {
            currentPlatform = null;
        }
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;
        if (ItsFrozen) return;

        float speedY;
        if(!isMovingForward && !isMovingBackward)
        {
            speedY = 0;
        }
        else if (isMovingForward)
        {
            speedY = actualSpeed;
        }
        else
        {
            speedY = -actualSpeed;
        }

        float speedX;
        if (!isMovingRight && !isMovingLeft)
        {
            speedX = 0;
        }
        else if (isMovingRight)
        {
            speedX = actualSpeed;
        }
        else
        {
            speedX = -actualSpeed;
        }

        animator.SetFloat("VelX", speedX);
        animator.SetFloat("VelY", speedY);
    }

    private void Falling()
    {
        animator.SetBool("Jumping", false);
        animator.SetBool("InFloor", false);
    }
}
