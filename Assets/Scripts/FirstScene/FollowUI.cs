using UnityEngine;

public class FollowUI : MonoBehaviour
{
    [Header("Configuración de Seguimiento")]
    public Transform playerCamera;
    public float smoothness = 2.0f;
    public float distance = 0.5f;

    void LateUpdate()
    {
        if (playerCamera == null) return;

        // Posición frente a la cámara
        Vector3 targetPos = playerCamera.position + (playerCamera.forward * distance);
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothness);

        // Rotacion orientada al jugador (solo eje Y para VR)
        Vector3 directionGlance = playerCamera.position - transform.position;
        directionGlance.y = 0;

        if (directionGlance != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(-directionGlance); // Invertido para que el UI mire al jugador
        }
    }
}