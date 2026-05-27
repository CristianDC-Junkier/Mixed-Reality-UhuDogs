using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class CameraController : MonoBehaviour
{
    public Transform cameraTransform;

    [Header("Movimiento")]
    public float speed = 5f;
    public float mouseSensitivity = 150f;

    [Header("Altura")]
    public float eyeHeight = 1.65f;

    [Header("Salto visual")]
    public float jumpAmount = 0.5f;
    public float jumpSpeed = 6f;

    float yaw;
    float pitch;
    float jumpOffset;
    bool jumping;

    CharacterController controller;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        Keyboard k = Keyboard.current;
        Mouse m = Mouse.current;

        if (k == null || m == null || cameraTransform == null) return;

        // ===== Mirar =====
        Vector2 delta = m.delta.ReadValue();

        yaw += delta.x * mouseSensitivity * Time.deltaTime;
        pitch -= delta.y * mouseSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // ===== Movimiento =====
        Vector3 move = Vector3.zero;

        if (k.wKey.isPressed) move += transform.forward;
        if (k.sKey.isPressed) move -= transform.forward;
        if (k.aKey.isPressed) move -= transform.right;
        if (k.dKey.isPressed) move += transform.right;

        move.y = 0f;

        if (move != Vector3.zero)
            move.Normalize();

        controller.Move(move * speed * Time.deltaTime);

        // ===== Salto visual =====
        if (k.spaceKey.wasPressedThisFrame && !jumping && jumpOffset <= 0.01f)
            jumping = true;

        if (jumping)
        {
            jumpOffset += jumpSpeed * Time.deltaTime;

            if (jumpOffset >= jumpAmount)
                jumping = false;
        }
        else if (jumpOffset > 0)
        {
            jumpOffset -= jumpSpeed * Time.deltaTime;
        }

        jumpOffset = Mathf.Clamp(jumpOffset, 0f, jumpAmount);

        // Mantener altura correcta
        Vector3 camPos = cameraTransform.localPosition;
        camPos.y = eyeHeight + jumpOffset;
        cameraTransform.localPosition = camPos;
    }
}