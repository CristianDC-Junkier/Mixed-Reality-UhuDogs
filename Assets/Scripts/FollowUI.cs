using UnityEngine;

public class FollowUI : MonoBehaviour
{
    public Transform camaraJugador;
    public float suavidad = 2.0f;
    public float distancia = 0.5f;

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One) || OVRInput.GetDown(OVRInput.RawButton.A))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Cristian");
        }
    }

    void LateUpdate()
    {
        if (camaraJugador == null) return;

        // 1. Posición deseada: frente a la cámara
        Vector3 targetPos = camaraJugador.position + (camaraJugador.forward * distancia);
        // 2. Movimiento suave
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * suavidad);

        // 3. Rotación: que mire al jugador pero manteniéndose vertical (opcional)
        // Usamos una rotación que solo gire en el eje Y para que no se incline raro
        Vector3 direccionMirada = camaraJugador.position - transform.position;
        direccionMirada.y = 0; // Esto evita que el texto se incline hacia arriba/abajo
        if (direccionMirada != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direccionMirada);
        }
    }
}