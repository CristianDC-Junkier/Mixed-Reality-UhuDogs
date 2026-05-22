using UnityEngine;

public class SoundsController : MonoBehaviour
{
    public AudioSource clip;

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayGame()
    {
        //Cambiar escena o desactivar menu desde la interfaz
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ButtonSound()
    {
        clip.Play();
    }
}
