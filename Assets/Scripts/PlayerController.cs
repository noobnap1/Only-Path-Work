using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;  // Drag your Camera Transform here

    private CharacterController controller;

    [Header("Movement Settings")]
    public float walkSpeed = 5f; // UNUSED NOW (replaced by maxSpeed)
    public float jumpPower = 2f;

    [Header("Momentum / Acceleration")]
    public float maxSpeed = 7f;        // Maximum running speed
    public float acceleration = 10f;   // How fast you speed up
    public float deceleration = 8f;    // How fast you slow down when letting go

    private Vector3 momentumVelocity;  // Horizontal momentum

    [Header("Look Settings")]
    [Range(1, 200)]
    public float mouseSensitivity = 100f;

    [Header("Input Actions")]
    public InputActionReference moveActionRef;   // Vector2
    public InputActionReference lookActionRef;   // Vector2
    public InputActionReference jumpActionRef;   // Button

    private Vector2 moveInput;
    private Vector2 lookInput;

    private float verticalVelocity;
    private float xRotation = 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (!controller) Debug.LogError("CharacterController missing!");
        if (!cameraTransform) Debug.LogError("Camera Transform not assigned!");
    }

    void OnEnable()
    {
        moveActionRef?.action.Enable();
        lookActionRef?.action.Enable();
        jumpActionRef?.action.Enable();
    }

    void OnDisable()
    {
        moveActionRef?.action.Disable();
        lookActionRef?.action.Disable();
        jumpActionRef?.action.Disable();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Input sampling
        moveInput = moveActionRef?.action.ReadValue<Vector2>() ?? Vector2.zero;
        lookInput = lookActionRef?.action.ReadValue<Vector2>() ?? Vector2.zero;

        // Apply mouse sensitivity scaling
        float sensitivityMultiplier = mouseSensitivity / 100f;
        float mouseX = lookInput.x * sensitivityMultiplier;
        float mouseY = lookInput.y * sensitivityMultiplier;

        // Rotate player horizontally
        transform.Rotate(Vector3.up * mouseX);

        // Rotate camera vertically
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void FixedUpdate()
    {
        bool isGrounded = controller.isGrounded;

        // Gravity handling
        if (isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        if (jumpActionRef != null && jumpActionRef.action.triggered && isGrounded)
            verticalVelocity = jumpPower;

        verticalVelocity += Physics.gravity.y * Time.fixedDeltaTime;

        // --- MOMENTUM SYSTEM ---

        // 1. Desired input direction
        Vector3 inputDir = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;

        // 2. Accelerate when moving
        if (inputDir.magnitude > 0.1f)
        {
            momentumVelocity = Vector3.MoveTowards(
                momentumVelocity,
                inputDir * maxSpeed,
                acceleration * Time.fixedDeltaTime
            );
        }
        else
        {
            // 3. Decelerate to zero when no input
            momentumVelocity = Vector3.MoveTowards(
                momentumVelocity,
                Vector3.zero,
                deceleration * Time.fixedDeltaTime
            );
        }

        // 4. Apply vertical movement
        Vector3 finalMove = momentumVelocity;
        finalMove.y = verticalVelocity;

        // 5. Move controller
        controller.Move(finalMove * Time.fixedDeltaTime);
    }
}
