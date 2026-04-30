using UnityEngine;
using System.Collections;

public class IntroSequence : MonoBehaviour
{
    public GameObject pantalla;
    public GameObject logo;
    public GameObject autores;
    public GameObject subLogo;
    public FollowUI followUI;

    public AudioSource audioSource;
    public AudioClip musicaIntro;
    public AudioClip musicaFinal;

    void Start()
    {
        logo.SetActive(false);
        autores.SetActive(false);
        subLogo.SetActive(false);
        pantalla.SetActive(false);

        followUI.enabled = false;

        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        // Música intro
        audioSource.clip = musicaIntro;
        audioSource.Play();

        // Pantalla Inicial
        yield return new WaitForSeconds(3f);

        pantalla.SetActive(true);

        yield return new WaitForSeconds(3f);

        // Logo
        logo.SetActive(true);

        yield return new WaitForSeconds(3f);

        // Autores
        logo.SetActive(false);
        autores.SetActive(true);

        yield return new WaitForSeconds(3f);

        // Sub logo
        autores.SetActive(false);
        subLogo.SetActive(true);

        yield return new WaitForSeconds(3f);

        // Final intro
        subLogo.SetActive(false);

        yield return new WaitForSeconds(3f);

        pantalla.SetActive(false);

        // Cambiar música
        audioSource.Stop();
        audioSource.clip = musicaFinal;
        audioSource.Play();

        followUI.enabled = true;
    }
}