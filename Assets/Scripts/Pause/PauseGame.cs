using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class PauseGame : MonoBehaviour
{
    [Header("Script de Movimiento")]
    public MonoBehaviour firstPersonLocomotor;

    [Header("Colision del personaje")]
    public Rigidbody playerRigidbody;

    [Header("Menu de pausa")]
    public GameObject pauseMenu;

    void Start()
    {
        pauseMenu.SetActive(false);
    }

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Start))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {

        if (Time.timeScale != 0f)
            Pause();
        else
            Resume();
    }

    void Pause()
    {
        Time.timeScale = 0f;

        if (firstPersonLocomotor != null)
            firstPersonLocomotor.enabled = false;

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
            playerRigidbody.isKinematic = true;
        }

        pauseMenu.SetActive(true);
    }

    void Resume()
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

        ResetPanels();
        pauseMenu.SetActive(false);
    }

    void ResetPanels()
    {
        if (pauseMenu != null)
        {
            Transform main = pauseMenu.transform.Find("CanvasPause/MainPanel");
            Transform options = pauseMenu.transform.Find("CanvasPause/OptionsPanel");

            if (main != null) main.gameObject.SetActive(true);
            if (options != null) options.gameObject.SetActive(false);
        }
    }
}