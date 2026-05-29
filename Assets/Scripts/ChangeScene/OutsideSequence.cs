using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class OutsideSequence : MonoBehaviour
{
    [Header("Transición oscura")]
    public Image darkScreen;

    [Header("Elemento de seguimiento")]
    public GameObject followUI;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip doorSound;

    [Header("Escenas")]
    public Object leftScene;
    public Object rightScene;

    void Start()
    {
        if (darkScreen != null) darkScreen.color = new Color(0, 0, 0, 0);
        StartCoroutine(WaitTransition());
    }

    IEnumerator WaitTransition()
    {
        while (true)
        {
            // ===== MANDO DERECHO (Secondary) =====
            if (OVRInput.GetDown(OVRInput.Button.One) ||
                OVRInput.GetDown(OVRInput.Button.Two) ||
                OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.8f ||
                OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) > 0.8f)
            {
                yield return StartCoroutine(ChangeTransition(rightScene.name));
                yield break;
            }

            // ===== MANDO IZQUIERDO (Primary) =====
            if (OVRInput.GetDown(OVRInput.Button.Three) ||
                OVRInput.GetDown(OVRInput.Button.Four) ||
                OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) > 0.8f ||
                OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) > 0.8f)
            {
                yield return StartCoroutine(ChangeTransition(leftScene.name));
                yield break;
            }

            yield return null;
        }
    }

    IEnumerator ChangeTransition(string sceneName)
    {
        Debug.Log("Iniciando transición hacia: " + sceneName);

        float tiempo = 0;
        float duracionFade = 1.5f;

        while (tiempo < duracionFade)
        {
            tiempo += Time.deltaTime;
            if (darkScreen != null)
                darkScreen.color = new Color(0, 0, 0, Mathf.Clamp01(tiempo / duracionFade));
            yield return null;
        }

        if (followUI != null)
        {
            foreach (Transform hijo in followUI.transform)
                hijo.gameObject.SetActive(false);
        }

        if (audioSource != null && doorSound != null)
        {
            audioSource.Stop();
            audioSource.clip = doorSound;
            audioSource.Play();
            yield return new WaitForSeconds(doorSound.length);
        }

        int walks = PlayerPrefs.GetInt("nWalks", 0) + 1;
        PlayerPrefs.SetInt("nWalks", walks);
        PlayerPrefs.Save();

        SceneManager.LoadScene(sceneName);
    }
}