using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("References")]
    public Transform playerBody;

    [Header("Mouse Settings")]
    public float mouseSensitivity = 200f;

    [Header("Look Limits")]
    private float xRotation = 0f;

    [Header("Sway Settings")]
    public float swayAmount = 2f;
    public float swaySmooth = 6f;
    public float swayClamp = 5f;

    private Vector2 currentMouseDelta;
    private Vector2 smoothedMouseDelta;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        if (playerBody == null && transform.parent != null)
            playerBody = transform.parent;
    }

    void Update()
    {
        // --- Get raw mouse delta ---
        currentMouseDelta.x = Input.GetAxisRaw("Mouse X");
        currentMouseDelta.y = Input.GetAxisRaw("Mouse Y");

        // --- Smooth mouse movement ---
        smoothedMouseDelta = Vector2.Lerp(smoothedMouseDelta, currentMouseDelta, Time.deltaTime * 12f);

        // --- Apply sensitivity and deltaTime ---
        float mouseX = smoothedMouseDelta.x * mouseSensitivity * Time.deltaTime;
        float mouseY = smoothedMouseDelta.y * mouseSensitivity * Time.deltaTime;

        // --- Rotate player (Yaw) ---
        playerBody.Rotate(Vector3.up * mouseX);

        // --- Rotate camera (Pitch) ---
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // --- Camera sway (tilt left/right only, no vertical drifting) ---
        float swayZ = Mathf.Clamp(-smoothedMouseDelta.x * swayAmount, -swayClamp, swayClamp);
        float swayX = Mathf.Clamp(-smoothedMouseDelta.y * swayAmount, -swayClamp, swayClamp);

        Quaternion targetRotation = Quaternion.Euler(xRotation + swayX, 0f, swayZ);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * swaySmooth);
    }
}
