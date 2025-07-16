using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    public Transform playerBody;
    public float mouseSensitivity = 100f;

    private float xRotation = 0f;

    public float swayAmount = 2f;
    public float swaySmooth = 6f;
    public float swayClamp = 5f;

    public InputActionReference lookActionRef; // Assign Player/Look here

    private Vector2 currentMouseDelta;
    private Vector2 smoothedMouseDelta;

    private void OnEnable()
    {
        lookActionRef?.action.Enable();
    }

    private void OnDisable()
    {
        lookActionRef?.action.Disable();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        if (playerBody == null && transform.parent != null)
            playerBody = transform.parent;
    }

    void Update()
    {
        if (lookActionRef == null) return;

        currentMouseDelta = lookActionRef.action.ReadValue<Vector2>();
        smoothedMouseDelta = Vector2.Lerp(smoothedMouseDelta, currentMouseDelta, Time.deltaTime * 12f);

        float mouseX = smoothedMouseDelta.x * mouseSensitivity * Time.deltaTime;
        float mouseY = smoothedMouseDelta.y * mouseSensitivity * Time.deltaTime;

        playerBody.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        float swayZ = Mathf.Clamp(-smoothedMouseDelta.x * swayAmount, -swayClamp, swayClamp);
        float swayX = Mathf.Clamp(-smoothedMouseDelta.y * swayAmount, -swayClamp, swayClamp);

        Quaternion targetRotation = Quaternion.Euler(xRotation + swayX, 0f, swayZ);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * swaySmooth);
    }
}
