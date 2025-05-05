using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private Animator animator;

    [Header("Camera Settings")]
    public Camera playerCamera;
    public float fov = 60f;
    public bool invertCamera = false;
    public bool cameraCanMove = true;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 50f;
    private float yaw, pitch;

    [Header("Movement Settings")]
    public bool playerCanMove = true;
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public float maxVelocityChange = 10f;
    private bool isSprinting;

    [Header("Jump Settings")]
    public bool enableJump = true;
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpPower = 5f;
    private bool isGrounded;

    [Header("Crouch Settings")]
    public bool enableCrouch = true;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public float crouchHeight = 0.75f;
    public float speedReduction = 0.5f;
    private bool isCrouched = false;
    private Vector3 originalScale;

    [Header("Crosshair Settings")]
    public bool lockCursor = true;
    public bool crosshair = true;
    public Sprite crosshairImage;
    public Color crosshairColor = Color.white;
    private Image crosshairObject;

    [Header("Gun Settings")]
    public GameObject bulletPrefab;
    public Transform gunMuzzle;
    public float bulletForce = 30f;
    public float bulletLifetime = 2f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        originalScale = transform.localScale;
        crosshairObject = GetComponentInChildren<Image>();
    }

    private void Start()
    {
        if (lockCursor)
            Cursor.lockState = CursorLockMode.Locked;

        if (crosshair && crosshairObject != null)
        {
            crosshairObject.sprite = crosshairImage;
            crosshairObject.color = crosshairColor;
        }
        else if (crosshairObject != null)
        {
            crosshairObject.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        HandleCamera();

        if (enableJump && Input.GetKeyDown(jumpKey) && isGrounded)
            Jump();

        if (enableCrouch && Input.GetKeyDown(crouchKey))
            ToggleCrouch();

        if (Input.GetMouseButtonDown(0))
            Shoot();

        CheckGround();
    }

    private void FixedUpdate()
    {
        if (!playerCanMove) return;

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
        if (!cameraCanMove || playerCamera == null) return;

        yaw = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch += (invertCamera ? 1 : -1) * mouseSensitivity * Input.GetAxis("Mouse Y");
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

        transform.localEulerAngles = new Vector3(0, yaw, 0);
        playerCamera.transform.localEulerAngles = new Vector3(pitch, 0, 0);
    }

    private void CheckGround()
    {
        Vector3 origin = transform.position + Vector3.down * (transform.localScale.y * 0.5f);
        isGrounded = Physics.Raycast(origin, Vector3.down, 0.75f);
    }

    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        isGrounded = false;
    }

    private void ToggleCrouch()
    {
        if (isCrouched)
        {
            transform.localScale = originalScale;
            walkSpeed /= speedReduction;
            isCrouched = false;
        }
        else
        {
            transform.localScale = new Vector3(originalScale.x, crouchHeight, originalScale.z);
            walkSpeed *= speedReduction;
            isCrouched = true;
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null || gunMuzzle == null || playerCamera == null) return;

        GameObject bullet = Instantiate(bulletPrefab, gunMuzzle.position, Quaternion.identity);
        if (bullet.TryGetComponent(out Rigidbody bulletRb))
        {
            Vector3 shootDir;
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit))
                shootDir = (hit.point - gunMuzzle.position).normalized;
            else
                shootDir = playerCamera.transform.forward;

            bulletRb.AddForce(shootDir * bulletForce, ForceMode.Impulse);

            // 자기 자신과 충돌 무시
            if (bullet.TryGetComponent(out Collider bulletCol) && TryGetComponent(out Collider selfCol))
                Physics.IgnoreCollision(selfCol, bulletCol);
        }

        Destroy(bullet, bulletLifetime);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(PlayerController)), InitializeOnLoad]
public class PlayerControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.HelpBox("Modular First Person Controller\nCustomized by ChatGPT", MessageType.Info);
    }
}
#endif
