using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;  // Drag your Camera Transform here

    private CharacterController controller;

    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float jumpPower = 2f;

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

        // Look (raw values from Input System, sensitivity handled in action asset)
        float mouseX = lookInput.x;
        float mouseY = lookInput.y;

        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void FixedUpdate()
    {
        bool isGrounded = controller.isGrounded;
        if (isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        if (jumpActionRef != null && jumpActionRef.action.triggered && isGrounded)
            verticalVelocity = jumpPower;

        verticalVelocity += Physics.gravity.y * Time.fixedDeltaTime;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        move *= walkSpeed;
        move.y = verticalVelocity;

        controller.Move(move * Time.fixedDeltaTime);
    }
}
