// CHANGE LOG
// 
// CHANGES || version VERSION
//
// "Enable/Disable Headbob, Changed look rotations - should result in reduced camera jitters" || version 1.0.1

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
    using UnityEditor;
    using System.Net;
#endif

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;

    #region Camera Movement Variables

    public Camera playerCamera;

    public float fov = 60f;
    public bool invertCamera = false;
    public bool cameraCanMove = true;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 50f;

    // Crosshair
    public bool lockCursor = true;
    public bool crosshair = true;
    public Sprite crosshairImage;
    public Color crosshairColor = Color.white;

    // Internal Variables
    private float yaw = 0.0f;
    private float pitch = 0.0f;
    private Image crosshairObject;

    #region Camera Zoom Variables

    public bool enableZoom = true;
    public bool holdToZoom = false;
    public KeyCode zoomKey = KeyCode.Mouse1;
    public float zoomFOV = 30f;
    public float zoomStepTime = 5f;

    // Internal Variables
    private bool isZoomed = false;

    #endregion
    #endregion

    #region Movement Variables

    public bool playerCanMove = true;
    public float walkSpeed = 5f;
    public float maxVelocityChange = 10f;

    // Internal Variables
    private bool isWalking = false;

    #region Sprint

    public bool enableSprint = true;
    public bool unlimitedSprint = false;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public float sprintSpeed = 7f;
    public float sprintDuration = 5f;
    public float sprintCooldown = .5f;
    public float sprintFOV = 80f;
    public float sprintFOVStepTime = 10f;

    // Sprint Bar
    public bool useSprintBar = true;
    public bool hideBarWhenFull = true;
    public Image sprintBarBG;
    public Image sprintBar;
    public float sprintBarWidthPercent = .3f;
    public float sprintBarHeightPercent = .015f;

    // Internal Variables
    private CanvasGroup sprintBarCG;
    private bool isSprinting = false;
    private float sprintRemaining;
    private float sprintBarWidth;
    private float sprintBarHeight;
    private bool isSprintCooldown = false;
    private float sprintCooldownReset;

    #endregion

    #region Jump

    public bool enableJump = true;
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpPower = 5f;

    // Internal Variables
    private bool isGrounded = false;

    #endregion

    #region Crouch

    public bool enableCrouch = true;
    public bool holdToCrouch = true;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public float crouchHeight = .75f;
    public float speedReduction = .5f;

    // Internal Variables
    private bool isCrouched = false;
    private Vector3 originalScale;

    #endregion
    #endregion

    #region Head Bob

    public bool enableHeadBob = true;
    public Transform joint;
    public float bobSpeed = 10f;
    public Vector3 bobAmount = new Vector3(.15f, .05f, 0f);

    // Internal Variables
    private Vector3 jointOriginalPos;
    private float timer = 0;

    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        crosshairObject = GetComponentInChildren<Image>();

        // Set internal variables
        playerCamera.fieldOfView = fov;
        originalScale = transform.localScale;
        jointOriginalPos = joint.localPosition;

        if (!unlimitedSprint)
        {
            sprintRemaining = sprintDuration;
            sprintCooldownReset = sprintCooldown;
        }
    }

    void Start()
    {
        if(lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        if(crosshair)
        {
            crosshairObject.sprite = crosshairImage;
            crosshairObject.color = crosshairColor;
        }
        else
        {
            crosshairObject.gameObject.SetActive(false);
        }

        #region Sprint Bar

        sprintBarCG = GetComponentInChildren<CanvasGroup>();

        if(useSprintBar)
        {
            sprintBarBG.gameObject.SetActive(true);
            sprintBar.gameObject.SetActive(true);

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            sprintBarWidth = screenWidth * sprintBarWidthPercent;
            sprintBarHeight = screenHeight * sprintBarHeightPercent;

            sprintBarBG.rectTransform.sizeDelta = new Vector3(sprintBarWidth, sprintBarHeight, 0f);
            sprintBar.rectTransform.sizeDelta = new Vector3(sprintBarWidth - 2, sprintBarHeight - 2, 0f);

            if(hideBarWhenFull)
            {
                sprintBarCG.alpha = 0;
            }
        }
        else
        {
            sprintBarBG.gameObject.SetActive(false);
            sprintBar.gameObject.SetActive(false);
        }

        #endregion
    }

    float camRotation;

    private void Update()
    {
        #region Camera

        // Control camera movement
        if(cameraCanMove)
        {
            yaw = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivity;

            if (!invertCamera)
            {
                pitch -= mouseSensitivity * Input.GetAxis("Mouse Y");
            }
            else
            {
                // Inverted Y
                pitch += mouseSensitivity * Input.GetAxis("Mouse Y");
            }

            // Clamp pitch between lookAngle
            pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

            transform.localEulerAngles = new Vector3(0, yaw, 0);
            playerCamera.transform.localEulerAngles = new Vector3(pitch, 0, 0);
        }

        #region Camera Zoom

        if (enableZoom)
        {
            // Changes isZoomed when key is pressed
            // Behavior for toogle zoom
            if(Input.GetKeyDown(zoomKey) && !holdToZoom && !isSprinting)
            {
                if (!isZoomed)
                {
                    isZoomed = true;
                }
                else
                {
                    isZoomed = false;
                }
            }

            // Changes isZoomed when key is pressed
            // Behavior for hold to zoom
            if(holdToZoom && !isSprinting)
            {
                if(Input.GetKeyDown(zoomKey))
                {
                    isZoomed = true;
                }
                else if(Input.GetKeyUp(zoomKey))
                {
                    isZoomed = false;
                }
            }

            // Lerps camera.fieldOfView to allow for a smooth transistion
            if(isZoomed)
            {
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, zoomFOV, zoomStepTime * Time.deltaTime);
            }
            else if(!isZoomed && !isSprinting)
            {
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, fov, zoomStepTime * Time.deltaTime);
            }
        }

        #endregion
        #endregion

        #region Sprint

        if(enableSprint)
        {
            if(isSprinting)
            {
                isZoomed = false;
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, sprintFOV, sprintFOVStepTime * Time.deltaTime);

                // Drain sprint remaining while sprinting
                if(!unlimitedSprint)
                {
                    sprintRemaining -= 1 * Time.deltaTime;
                    if (sprintRemaining <= 0)
                    {
                        isSprinting = false;
                        isSprintCooldown = true;
                    }
                }
            }
            else
            {
                // Regain sprint while not sprinting
                sprintRemaining = Mathf.Clamp(sprintRemaining += 1 * Time.deltaTime, 0, sprintDuration);
            }

            // Handles sprint cooldown 
            // When sprint remaining == 0 stops sprint ability until hitting cooldown
            if(isSprintCooldown)
            {
                sprintCooldown -= 1 * Time.deltaTime;
                if (sprintCooldown <= 0)
                {
                    isSprintCooldown = false;
                }
            }
            else
            {
                sprintCooldown = sprintCooldownReset;
            }

            // Handles sprintBar 
            if(useSprintBar && !unlimitedSprint)
            {
                float sprintRemainingPercent = sprintRemaining / sprintDuration;
                sprintBar.transform.localScale = new Vector3(sprintRemainingPercent, 1f, 1f);
            }
        }

        #endregion

        #region Jump

        // Gets input and calls jump method
        if(enableJump && Input.GetKeyDown(jumpKey) && isGrounded)
        {
            Jump();
        }

        #endregion

        #region Crouch

        if (enableCrouch)
        {
            if(Input.GetKeyDown(crouchKey) && !holdToCrouch)
            {
                Crouch();
            }
            
            if(Input.GetKeyDown(crouchKey) && holdToCrouch)
            {
                isCrouched = false;
                Crouch();
            }
            else if(Input.GetKeyUp(crouchKey) && holdToCrouch)
            {
                isCrouched = true;
                Crouch();
            }
        }

        #endregion

        CheckGround();

        if(enableHeadBob)
        {
            HeadBob();
        }
    }

    void FixedUpdate()
    {
        #region Movement

        if (playerCanMove)
        {
            // Calculate how fast we should be moving
            Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            // Checks if player is walking and isGrounded
            // Will allow head bob
            if (targetVelocity.x != 0 || targetVelocity.z != 0 && isGrounded)
            {
                isWalking = true;
            }
            else
            {
                isWalking = false;
            }

            // All movement calculations shile sprint is active
            if (enableSprint && Input.GetKey(sprintKey) && sprintRemaining > 0f && !isSprintCooldown)
            {
                targetVelocity = transform.TransformDirection(targetVelocity) * sprintSpeed;

                // Apply a force that attempts to reach our target velocity
                Vector3 velocity = rb.velocity;
                Vector3 velocityChange = (targetVelocity - velocity);
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                velocityChange.y = 0;

                // Player is only moving when valocity change != 0
                // Makes sure fov change only happens during movement
                if (velocityChange.x != 0 || velocityChange.z != 0)
                {
                    isSprinting = true;

                    if (isCrouched)
                    {
                        Crouch();
                    }

                    if (hideBarWhenFull && !unlimitedSprint)
                    {
                        sprintBarCG.alpha += 5 * Time.deltaTime;
                    }
                }

                rb.AddForce(velocityChange, ForceMode.VelocityChange);
            }
            // All movement calculations while walking
            else
            {
                isSprinting = false;

                if (hideBarWhenFull && sprintRemaining == sprintDuration)
                {
                    sprintBarCG.alpha -= 3 * Time.deltaTime;
                }

                targetVelocity = transform.TransformDirection(targetVelocity) * walkSpeed;

                // Apply a force that attempts to reach our target velocity
                Vector3 velocity = rb.velocity;
                Vector3 velocityChange = (targetVelocity - velocity);
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                velocityChange.y = 0;

                rb.AddForce(velocityChange, ForceMode.VelocityChange);
            }
        }

        #endregion
    }

    // Sets isGrounded based on a raycast sent straigth down from the player object
    private void CheckGround()
    {
        Vector3 origin = new Vector3(transform.position.x, transform.position.y - (transform.localScale.y * .5f), transform.position.z);
        Vector3 direction = transform.TransformDirection(Vector3.down);
        float distance = .75f;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance))
        {
            Debug.DrawRay(origin, direction * distance, Color.red);
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void Jump()
    {
        // Adds force to the player rigidbody to jump
        if (isGrounded)
        {
            rb.AddForce(0f, jumpPower, 0f, ForceMode.Impulse);
            isGrounded = false;
        }

        // When crouched and using toggle system, will uncrouch for a jump
        if(isCrouched && !holdToCrouch)
        {
            Crouch();
        }
    }

    private void Crouch()
    {
        // Stands player up to full height
        // Brings walkSpeed back up to original speed
        if(isCrouched)
        {
            transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
            walkSpeed /= speedReduction;

            isCrouched = false;
        }
        // Crouches player down to set height
        // Reduces walkSpeed
        else
        {
            transform.localScale = new Vector3(originalScale.x, crouchHeight, originalScale.z);
            walkSpeed *= speedReduction;

            isCrouched = true;
        }
    }

    private void HeadBob()
    {
        if(isWalking)
        {
            // Calculates HeadBob speed during sprint
            if(isSprinting)
            {
                timer += Time.deltaTime * (bobSpeed + sprintSpeed);
            }
            // Calculates HeadBob speed during crouched movement
            else if (isCrouched)
            {
                timer += Time.deltaTime * (bobSpeed * speedReduction);
            }
            // Calculates HeadBob speed during walking
            else
            {
                timer += Time.deltaTime * bobSpeed;
            }
            // Applies HeadBob movement
            joint.localPosition = new Vector3(jointOriginalPos.x + Mathf.Sin(timer) * bobAmount.x, jointOriginalPos.y + Mathf.Sin(timer) * bobAmount.y, jointOriginalPos.z + Mathf.Sin(timer) * bobAmount.z);
        }
        else
        {
            // Resets when play stops moving
            timer = 0;
            joint.localPosition = new Vector3(Mathf.Lerp(joint.localPosition.x, jointOriginalPos.x, Time.deltaTime * bobSpeed), Mathf.Lerp(joint.localPosition.y, jointOriginalPos.y, Time.deltaTime * bobSpeed), Mathf.Lerp(joint.localPosition.z, jointOriginalPos.z, Time.deltaTime * bobSpeed));
        }
    }
}



// Custom Editor
#if UNITY_EDITOR
    [CustomEditor(typeof(PlayerController)), InitializeOnLoadAttribute]
    public class PlayerControllerEditor : Editor
    {
    PlayerController fpc;
    SerializedObject SerFPC;

    private void OnEnable()
    {
        fpc = (PlayerController)target;
        SerFPC = new SerializedObject(fpc);
    }

    public override void OnInspectorGUI()
{
    SerFPC.Update();

    EditorGUILayout.Space();
    GUILayout.Label("Modular First Person Controller", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 16 });
    GUILayout.Label("By Jess Case", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Normal, fontSize = 12 });
    GUILayout.Label("version 1.0.1", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Normal, fontSize = 12 });
    EditorGUILayout.Space();

    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
    GUILayout.Label("Camera Setup", EditorStyles.boldLabel);
    EditorGUILayout.Space();

    fpc.playerCamera = (Camera)EditorGUILayout.ObjectField("Camera", fpc.playerCamera, typeof(Camera), true);
    fpc.fov = EditorGUILayout.Slider("Field of View", fpc.fov, fpc.zoomFOV, 179f);
    fpc.cameraCanMove = EditorGUILayout.ToggleLeft("Enable Camera Rotation", fpc.cameraCanMove);

    GUI.enabled = fpc.cameraCanMove;
    fpc.invertCamera = EditorGUILayout.ToggleLeft("Invert Camera Rotation", fpc.invertCamera);
    fpc.mouseSensitivity = EditorGUILayout.Slider("Look Sensitivity", fpc.mouseSensitivity, .1f, 10f);
    fpc.maxLookAngle = EditorGUILayout.Slider("Max Look Angle", fpc.maxLookAngle, 40, 90);
    GUI.enabled = true;

    fpc.lockCursor = EditorGUILayout.ToggleLeft("Lock and Hide Cursor", fpc.lockCursor);
    fpc.crosshair = EditorGUILayout.ToggleLeft("Auto Crosshair", fpc.crosshair);

    if (fpc.crosshair)
    {
        EditorGUI.indentLevel++;
        fpc.crosshairImage = (Sprite)EditorGUILayout.ObjectField("Crosshair Image", fpc.crosshairImage, typeof(Sprite), false);
        fpc.crosshairColor = EditorGUILayout.ColorField("Crosshair Color", fpc.crosshairColor);
        EditorGUI.indentLevel--;
    }

    EditorGUILayout.Space();
    GUILayout.Label("Zoom", EditorStyles.boldLabel);

    fpc.enableZoom = EditorGUILayout.ToggleLeft("Enable Zoom", fpc.enableZoom);
    GUI.enabled = fpc.enableZoom;
    fpc.holdToZoom = EditorGUILayout.ToggleLeft("Hold to Zoom", fpc.holdToZoom);
    fpc.zoomKey = (KeyCode)EditorGUILayout.EnumPopup("Zoom Key", fpc.zoomKey);
    fpc.zoomFOV = EditorGUILayout.Slider("Zoom FOV", fpc.zoomFOV, .1f, fpc.fov);
    fpc.zoomStepTime = EditorGUILayout.Slider("Step Time", fpc.zoomStepTime, .1f, 10f);
    GUI.enabled = true;

    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
    GUILayout.Label("Movement Setup", EditorStyles.boldLabel);
    EditorGUILayout.Space();

    fpc.playerCanMove = EditorGUILayout.ToggleLeft("Enable Player Movement", fpc.playerCanMove);
    GUI.enabled = fpc.playerCanMove;
    fpc.walkSpeed = EditorGUILayout.Slider("Walk Speed", fpc.walkSpeed, .1f, fpc.sprintSpeed);
    GUI.enabled = true;

    EditorGUILayout.Space();
    GUILayout.Label("Sprint", EditorStyles.boldLabel);

    fpc.enableSprint = EditorGUILayout.ToggleLeft("Enable Sprint", fpc.enableSprint);
    GUI.enabled = fpc.enableSprint;
    fpc.unlimitedSprint = EditorGUILayout.ToggleLeft("Unlimited Sprint", fpc.unlimitedSprint);
    fpc.sprintKey = (KeyCode)EditorGUILayout.EnumPopup("Sprint Key", fpc.sprintKey);
    fpc.sprintSpeed = EditorGUILayout.Slider("Sprint Speed", fpc.sprintSpeed, fpc.walkSpeed, 20f);
    fpc.sprintDuration = EditorGUILayout.Slider("Sprint Duration", fpc.sprintDuration, 1f, 20f);
    fpc.sprintCooldown = EditorGUILayout.Slider("Sprint Cooldown", fpc.sprintCooldown, .1f, fpc.sprintDuration);
    fpc.sprintFOV = EditorGUILayout.Slider("Sprint FOV", fpc.sprintFOV, fpc.fov, 179f);
    fpc.sprintFOVStepTime = EditorGUILayout.Slider("Step Time", fpc.sprintFOVStepTime, .1f, 20f);
    fpc.useSprintBar = EditorGUILayout.ToggleLeft("Use Sprint Bar", fpc.useSprintBar);

    if (fpc.useSprintBar)
    {
        EditorGUI.indentLevel++;
        fpc.hideBarWhenFull = EditorGUILayout.ToggleLeft("Hide Full Bar", fpc.hideBarWhenFull);
        fpc.sprintBarBG = (Image)EditorGUILayout.ObjectField("Bar BG", fpc.sprintBarBG, typeof(Image), true);
        fpc.sprintBar = (Image)EditorGUILayout.ObjectField("Bar", fpc.sprintBar, typeof(Image), true);
        fpc.sprintBarWidthPercent = EditorGUILayout.Slider("Bar Width", fpc.sprintBarWidthPercent, .1f, .5f);
        fpc.sprintBarHeightPercent = EditorGUILayout.Slider("Bar Height", fpc.sprintBarHeightPercent, .001f, .025f);
        EditorGUI.indentLevel--;
    }
    GUI.enabled = true;

    EditorGUILayout.Space();
    GUILayout.Label("Jump", EditorStyles.boldLabel);

    fpc.enableJump = EditorGUILayout.ToggleLeft("Enable Jump", fpc.enableJump);
    GUI.enabled = fpc.enableJump;
    fpc.jumpKey = (KeyCode)EditorGUILayout.EnumPopup("Jump Key", fpc.jumpKey);
    fpc.jumpPower = EditorGUILayout.Slider("Jump Power", fpc.jumpPower, .1f, 60f);
    GUI.enabled = true;

    EditorGUILayout.Space();
    GUILayout.Label("Crouch", EditorStyles.boldLabel);

    fpc.enableCrouch = EditorGUILayout.ToggleLeft("Enable Crouch", fpc.enableCrouch);
    GUI.enabled = fpc.enableCrouch;
    fpc.holdToCrouch = EditorGUILayout.ToggleLeft("Hold To Crouch", fpc.holdToCrouch);
    fpc.crouchKey = (KeyCode)EditorGUILayout.EnumPopup("Crouch Key", fpc.crouchKey);
    fpc.crouchHeight = EditorGUILayout.Slider("Crouch Height", fpc.crouchHeight, .1f, 1);
    fpc.speedReduction = EditorGUILayout.Slider("Speed Reduction", fpc.speedReduction, .1f, 1);
    GUI.enabled = true;

    EditorGUILayout.Space();
    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
    GUILayout.Label("Head Bob Setup", EditorStyles.boldLabel);
    EditorGUILayout.Space();

    fpc.enableHeadBob = EditorGUILayout.ToggleLeft("Enable Head Bob", fpc.enableHeadBob);
    GUI.enabled = fpc.enableHeadBob;
    fpc.joint = (Transform)EditorGUILayout.ObjectField("Camera Joint", fpc.joint, typeof(Transform), true);
    fpc.bobSpeed = EditorGUILayout.Slider("Speed", fpc.bobSpeed, 1, 20);
    fpc.bobAmount = EditorGUILayout.Vector3Field("Bob Amount", fpc.bobAmount);
    GUI.enabled = true;

    if (GUI.changed)
    {
        EditorUtility.SetDirty(fpc);
        Undo.RecordObject(fpc, "FPC Change");
        SerFPC.ApplyModifiedProperties();
    }
}

}

#endif