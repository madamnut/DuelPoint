using UnityEngine;
using Unity.Netcode;

public class NetworkPlayerController : NetworkBehaviour
{
    private Rigidbody rb;

    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float maxVelocityChange = 10f;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Jump Settings")]
    public float jumpPower = 5f;

    [Header("Camera Settings")]
    public Camera playerCamera;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 50f;
    public bool invertCamera = false;

    private float yaw;
    private float pitch;
    private bool isGrounded;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();

        if (!IsOwner)
        {
            if (playerCamera != null)
            {
                playerCamera.enabled = false;
                if (playerCamera.TryGetComponent(out AudioListener listener))
                    listener.enabled = false;
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        // 입력 감지 후 서버에 전송
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        bool isSprinting = Input.GetKey(sprintKey);
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space);
        Vector2 mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        SendInputServerRpc(moveInput, isSprinting, jumpPressed, mouseInput);
    }

    [ServerRpc]
    private void SendInputServerRpc(Vector2 moveInput, bool isSprinting, bool jumpPressed, Vector2 mouseInput)
    {
        if (rb == null) return;

        // 회전 처리
        yaw += mouseInput.x * mouseSensitivity;
        pitch += (invertCamera ? 1 : -1) * mouseSensitivity * mouseInput.y;
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // 이동 처리
        Vector3 inputDir = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        Vector3 targetVelocity = transform.TransformDirection(inputDir) * (isSprinting ? sprintSpeed : walkSpeed);
        Vector3 velocityChange = targetVelocity - rb.velocity;
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = 0;

        rb.AddForce(velocityChange, ForceMode.VelocityChange);

        // 점프 처리
        if (jumpPressed && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            isGrounded = false;
        }
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
