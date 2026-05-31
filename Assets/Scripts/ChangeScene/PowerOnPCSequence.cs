using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PowerOnPCSequence : MonoBehaviour
{
    [Header("Transición oscura")]
    public Image darkScreen;

    [Header("Elemento de seguimiento")]
    public GameObject followUI;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip windowsOn;

    [Header("Escena a cambiar")]
    [Tooltip("Escribe el nombre exacto de la escena como aparece en tus assets")]
    public string nextSceneName;

    void Start()
    {
        if (darkScreen != null) darkScreen.color = new Color(0, 0, 0, 0);
        StartCoroutine(WaitTransition());
    }

    IEnumerator WaitTransition()
    {
        while (true)
        {

            if (
                OVRInput.GetDown(OVRInput.Button.One) ||
                OVRInput.GetDown(OVRInput.Button.Two) ||
                OVRInput.GetDown(OVRInput.Button.Three) ||
                OVRInput.GetDown(OVRInput.Button.Four) ||
                OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) > 0.8f ||
                OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.8f ||
                OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) > 0.8f ||
                OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) > 0.8f
            )
            {
                yield return StartCoroutine(ChangeTransition());
                yield break;
            }
            yield return null;
        }
    }

    IEnumerator ChangeTransition()
    {
        float tiempo = 0;
        float duracionFade = 1.5f;

        // Fundido a negro
        while (tiempo < duracionFade)
        {
            tiempo += Time.deltaTime;
            if (darkScreen != null)
                darkScreen.color = new Color(0, 0, 0, Mathf.Clamp01(tiempo / duracionFade));
            yield return null;
        }

        foreach (Transform hijo in followUI.transform)
        {
            hijo.gameObject.SetActive(false);
        }
        audioSource.Stop();

        // Música de transición final antes de cambiar escena
        audioSource.clip = windowsOn;
        audioSource.volume = 1f;
        audioSource.Play();
        yield return new WaitForSeconds(windowsOn.length);

        SceneManager.LoadScene(nextSceneName);
    }
}