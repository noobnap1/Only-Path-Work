using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public InputActionReference moveActionRef;  // Assign Player/Move here
    public InputActionReference jumpActionRef;  // Assign Player/Jump here

    public float speed = 5f;
    public float jumpPower = 2f;

    private CharacterController controller;
    private float verticalVelocity;

    private void OnEnable()
    {
        moveActionRef?.action.Enable();
        jumpActionRef?.action.Enable();
    }

    private void OnDisable()
    {
        moveActionRef?.action.Disable();
        jumpActionRef?.action.Disable();
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        bool isGrounded = controller.isGrounded;

        if (isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        Vector2 input = moveActionRef != null ? moveActionRef.action.ReadValue<Vector2>() : Vector2.zero;
        Vector3 move = transform.right * input.x + transform.forward * input.y;
        controller.Move(move * speed * Time.deltaTime);

        if (jumpActionRef != null && jumpActionRef.action.triggered && isGrounded)
            verticalVelocity = jumpPower;

        verticalVelocity += Physics.gravity.y * Time.deltaTime;
        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }
}
