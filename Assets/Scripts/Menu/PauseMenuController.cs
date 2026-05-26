using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenuController : MonoBehaviour
{
    public AudioSource clip;

    [Header("Transición oscura")]
    public Image darkScreen;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip doorSound;

    [Header("Script de Movimiento")]
    public MonoBehaviour firstPersonLocomotor;

    [Header("Colision del personaje")]
    public Rigidbody playerRigidbody;

    [Header("Menu de pausa")]
    public GameObject pauseMenu;

    void Start()
    {
        if (darkScreen != null) darkScreen.color = new Color(0, 0, 0, 0);
    }

    public void PlayClick()
    {
        clip.Play();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;

        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = false;
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }

        if (firstPersonLocomotor != null)
            firstPersonLocomotor.enabled = true;

        pauseMenu.SetActive(false);
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
            tiempo += Time.unscaledDeltaTime;

            if (darkScreen != null)
                darkScreen.color = new Color(0, 0, 0, Mathf.Clamp01(tiempo / duracionFade));

            yield return null;
        }

        audioSource.Stop();

        // Música de transición final antes de cambiar escena
        audioSource.clip = doorSound;
        audioSource.volume = 1f;
        audioSource.Play();

        yield return new WaitForSecondsRealtime(doorSound.length);

        Application.Quit();
    }
}