using TMPro;
using UnityEngine;

public class BlinkingText : MonoBehaviour
{
    public TMP_Text PressButtonText;
    public float velocidadParpadeo = 2f;
    public float velocidadPulso = 2f;
    public float intensidadPulso = 0.05f;

    Vector3 escalaInicial;

    void Start()
    {
        escalaInicial = transform.localScale;
    }

    void Update()
    {
        // Parpadeo (alpha)
        float alpha = Mathf.PingPong(Time.time * velocidadParpadeo, 1f);
        Color c = PressButtonText.color;
        c.a = alpha;
        PressButtonText.color = c;

        // Pulso (escala)
        float escala = 1 + Mathf.Sin(Time.time * velocidadPulso) * intensidadPulso;
        transform.localScale = escalaInicial * escala;
    }
}