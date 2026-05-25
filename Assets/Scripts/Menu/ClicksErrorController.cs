using UnityEngine;
using System.Collections;

public class ClicksErrorController : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip soundClick;
    public AudioClip soundError;

    public void PlaySounds()
    {
        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        audioSource.PlayOneShot(soundClick);

        yield return new WaitForSeconds(soundClick.length);

        audioSource.PlayOneShot(soundError);
    }
}
