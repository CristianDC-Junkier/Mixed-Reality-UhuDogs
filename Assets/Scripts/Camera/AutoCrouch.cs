using UnityEngine;
using Oculus.Interaction.Locomotion;

public class AutoCrouch : MonoBehaviour
{
    [Header("Camara Jugador")]
    public Transform centerEyeAnchor;

    [Header("Script de Movimiento")]
    public FirstPersonLocomotor locomotor;

    [Header("Angulo de agache")]
    public float crouchAngle = 45f;

    private bool autoCrouch;

    void Update()
    {
        if (centerEyeAnchor == null || locomotor == null)
            return;

        float pitch = centerEyeAnchor.localEulerAngles.x;

        if (pitch > 180f)
            pitch -= 360f;

        bool shouldCrouch = pitch > crouchAngle;

        if (shouldCrouch && !autoCrouch)
        {
            locomotor.Crouch(true);
            autoCrouch = true;
        }
        else if (!shouldCrouch && autoCrouch)
        {
            locomotor.Crouch(false);
            autoCrouch = false;
        }
    }
}