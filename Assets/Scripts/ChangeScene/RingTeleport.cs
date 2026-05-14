using UnityEngine;

public class RingTeleport : MonoBehaviour
{
    [Header("Rotation")]
    public float velocidadRotacion = 50f;

    [Header("Blink")]
    public float velocidadBlink = 2f;

    [Header("TeleportActivate")]
    public GameObject teleportSequence;

    private Renderer rend;
    private Material mat;

    private Transform playerInside;

    void Start()
    {
        rend = GetComponent<Renderer>();
        mat = rend.material;
    }

    void Update()
    {
        // Rotación
        transform.Rotate(0f, velocidadRotacion * Time.deltaTime, 0f);

        // Blink
        float t = (Mathf.Sin(Time.time * velocidadBlink) + 1f) * 0.5f;

        mat.color = Color.Lerp(Color.black, Color.white, t);

        // Activación si el jugador está dentro
        if (playerInside != null)
        {
            teleportSequence.SetActive(true);
        }
        else
        {
            teleportSequence.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = null;
        }
    }
}