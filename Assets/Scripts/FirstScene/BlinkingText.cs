using TMPro;
using UnityEngine;

public class BlinkingText : MonoBehaviour
{
    public TMP_Text PressButtonText;
    public float speedBlink = 2f;
    public float speedPulse = 2f;
    public float intensityPulse = 0.05f;

    Vector3 initialScale;

    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        // Parpadeo (alpha)
        float alpha = Mathf.PingPong(Time.time * speedBlink, 1f);
        Color c = PressButtonText.color;
        c.a = alpha;
        PressButtonText.color = c;

        // Pulso (escala)
        float escala = 1 + Mathf.Sin(Time.time * speedPulse) * intensityPulse;
        transform.localScale = initialScale * escala;
    }
}