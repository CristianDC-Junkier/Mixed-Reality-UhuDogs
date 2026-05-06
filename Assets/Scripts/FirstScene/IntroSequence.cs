using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class IntroSequence : MonoBehaviour
{
    [Header("Elementos de la pantalla del PC")]
    public GameObject pcScreen;
    public GameObject logoUHU;
    public GameObject logoETSI;
    public GameObject authors;

    [Header("Transición oscura")]
    public Image darkScreen;

    [Header("Elemento de seguimiento")]
    public GameObject followUI;
    public FollowUI followUIScript;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip officeSounds;
    public AudioClip introMusic;
    public AudioClip doorSound;

    [Header("Escena a cambiar")]
    public Object nextScene;

    void Start()
    {
        // Estado inicial
        pcScreen.SetActive(false);
        logoUHU.SetActive(false);
        logoETSI.SetActive(false);
        authors.SetActive(false);
        followUIScript.enabled = false;

        if (darkScreen != null) darkScreen.color = new Color(0, 0, 0, 0);

        StartCoroutine(IntroTransition());
    }

    IEnumerator IntroTransition()
    {
        // 1. Fase de Intro
        audioSource.clip = officeSounds;
        audioSource.Play();

        yield return new WaitForSeconds(3f);
        pcScreen.SetActive(true);
        yield return new WaitForSeconds(3f);
        logoUHU.SetActive(true);
        yield return new WaitForSeconds(3f);
        logoUHU.SetActive(false);
        logoETSI.SetActive(true);
        yield return new WaitForSeconds(3f);
        logoETSI.SetActive(false);
        authors.SetActive(true);
        yield return new WaitForSeconds(3f);
        authors.SetActive(false);
        pcScreen.SetActive(false);

        // 2. Cambio de música y activación de seguimiento
        audioSource.Stop();
        audioSource.clip = introMusic;
        audioSource.volume = 0.75f;
        audioSource.Play();
        followUIScript.enabled = true;

        // 3. Esperar a la entrada del jugador 
        yield return new WaitUntil(() => 
            // Teclado
            (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) || 
            // Mandos VR
            (OVRInput.GetDown(OVRInput.Button.Any))
        );

       
        followUIScript.enabled = false;

        yield return StartCoroutine(FinalTransition());
    }

    IEnumerator FinalTransition()
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
        audioSource.clip = doorSound;
        audioSource.volume = 1f;
        audioSource.Play();
        yield return new WaitForSeconds(doorSound.length);

        SceneManager.LoadScene(nextScene.name);
    }
}