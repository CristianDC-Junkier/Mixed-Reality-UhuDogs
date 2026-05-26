using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    public AudioSource clip;

    /*[Header("Transición oscura")]
    public Image darkScreen;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip windowsOff;
    public AudioClip doorSound;

    [Header("Escena a cambiar")]
    public Object nextScene; // Esto puede fallar al compilar el juego entero, sería mejor usar un string con el nombre de la escena
    
    void Start()
    {
        if (darkScreen != null) darkScreen.color = new Color(0, 0, 0, 0);
    }*/

    public void PlayClick()
    {
        clip.Play();
    }
    /*
    public void ContinueGame()
    {
        StartCoroutine(ChangeTransition()); // Arrancamos la transición
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

        audioSource.Stop();

        // Música de transición final antes de cambiar escena
        audioSource.clip = windowsOff;
        audioSource.volume = 1f;
        audioSource.Play();
        yield return new WaitForSeconds(windowsOff.length);

        SceneManager.LoadScene(nextScene.name);
    }

    public void QuitGame()
    {
        StartCoroutine(Quitransition()); // Arrancamos la transición
    }

    IEnumerator Quitransition()
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

        audioSource.Stop();

        // Música de transición final antes de cambiar escena
        audioSource.clip = doorSound;
        audioSource.volume = 1f;
        audioSource.Play();
        yield return new WaitForSeconds(doorSound.length);

        Application.Quit();
    }*/
}