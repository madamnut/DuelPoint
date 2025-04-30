using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;

    public Transform headTransform;   // 머리 (회전용)
    public Transform bodyVisual;      // 시각적 바디 (캡슐)

    float xRotation = 0f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMouseLook();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 direction = transform.right * h + transform.forward * v;
        Vector3 velocity = direction.normalized * moveSpeed;

        Vector3 move = new Vector3(velocity.x, rb.velocity.y, velocity.z);
        rb.velocity = move;
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 수직 회전
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        headTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 수평 회전 (플레이어 루트 회전)
        transform.Rotate(Vector3.up * mouseX);
    }
}
