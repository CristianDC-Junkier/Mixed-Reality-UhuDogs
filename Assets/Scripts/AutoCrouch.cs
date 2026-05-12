using UnityEngine;
using Oculus.Interaction.Locomotion;

public class AutoCrouch : MonoBehaviour
{
    public Transform centerEyeAnchor;
    public FirstPersonLocomotor locomotor;

    public float crouchAngle = 45f;

    void Update()
    {
        if (centerEyeAnchor == null || locomotor == null)
            return;

        float pitch = centerEyeAnchor.localEulerAngles.x;

        if (pitch > 180f)
            pitch -= 360f;

        bool shouldCrouch = pitch > crouchAngle;

        locomotor.Crouch(shouldCrouch);
    }
}