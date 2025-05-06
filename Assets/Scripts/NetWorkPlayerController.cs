using UnityEngine;
using Unity.Netcode;

public class NetworkPlayerController : NetworkBehaviour
{
    private Rigidbody rb;

    [Header("Camera Settings")]
    public Camera playerCamera;
    public float fov = 60f;
    public bool invertCamera = false;
    public bool cameraCanMove = true;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 50f;

    [Header("Movement Settings")]
    public bool playerCanMove = true;
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public float maxVelocityChange = 10f;

    [Header("Jump Settings")]
    public bool enableJump = true;
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpPower = 5f;

    private float yaw;
    private float pitch;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            if (playerCamera != null)
            {
                playerCamera.enabled = false;
                if (playerCamera.TryGetComponent(out AudioListener listener))
                    listener.enabled = false;
            }
            return;
        }

        // ✅ Owner일 경우 Rigidbody 재할당 보장
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        // ✅ 회전 기준 초기화
        yaw = transform.eulerAngles.y;

        Cursor.lockState = CursorLockMode.Locked;
        Debug.Log("[OnNetworkSpawn] Player controller ready");
    }

    private void Update()
    {
        if (!IsOwner) return;

        HandleCamera();

        if (enableJump && Input.GetKeyDown(jumpKey) && isGrounded)
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner || !playerCanMove) return;
        if (rb == null)
        {
            Debug.LogWarning("[FixedUpdate] Rigidbody is null");
            return;
        }

        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        Vector3 targetVelocity = transform.TransformDirection(input.normalized) *
                                 (Input.GetKey(sprintKey) ? sprintSpeed : walkSpeed);

        Vector3 velocityChange = targetVelocity - rb.velocity;
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = 0;

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    private void HandleCamera()
    {
        if (!IsOwner || !cameraCanMove || playerCamera == null) return;

        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch += (invertCamera ? 1 : -1) * mouseSensitivity * Input.GetAxis("Mouse Y");
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

        rb.MoveRotation(Quaternion.Euler(0, yaw, 0));
        playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
    }

    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        isGrounded = false;
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f)
            {
                isGrounded = true;
                break;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}
