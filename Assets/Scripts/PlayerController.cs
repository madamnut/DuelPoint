using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("이동 관련")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float gravity = -20f;

    [Header("마우스 회전")]
    public float mouseSensitivity = 2f;
    public Transform headTransform;    // 상하 회전 대상
    public Transform cameraTransform;  // (옵션) 카메라

    [Header("지면 체크")]
    public float groundCheckDistance = 1.1f;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundMask;

    private float yVelocity = 0f;
    private bool isGrounded = false;
    private float verticalLook = 0f;

    private void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleJump();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 좌우 회전 (Player 루트 회전)
        transform.Rotate(Vector3.up * mouseX);

        // 상하 회전 (Head 오브젝트 회전)
        verticalLook -= mouseY;
        verticalLook = Mathf.Clamp(verticalLook, -90f, 90f);
        headTransform.localRotation = Quaternion.Euler(verticalLook, 0f, 0f);
    }

    private void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 inputDir = (transform.right * h + transform.forward * v).normalized;
        Vector3 move = inputDir * moveSpeed * Time.deltaTime;

        // 중력 계산
        yVelocity += gravity * Time.deltaTime;
        move.y = yVelocity * Time.deltaTime;

        transform.position += move;
    }

    private void HandleJump()
    {
        // SphereCast로 경사 포함 바닥 체크
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        isGrounded = Physics.SphereCast(origin, groundCheckRadius, Vector3.down, out RaycastHit hit, groundCheckDistance, groundMask);

        if (isGrounded)
        {
            if (yVelocity < 0f)
                yVelocity = 0f;

            if (Input.GetKeyDown(KeyCode.Space))
                yVelocity = jumpForce;
        }
    }
}
