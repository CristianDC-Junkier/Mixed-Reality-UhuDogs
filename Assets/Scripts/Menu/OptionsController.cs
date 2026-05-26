using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OptionsController : MonoBehaviour
{
    public Slider sliderVolume;
    public Slider sliderBrightness;
    public Toggle toggleMute;
    public Image panelBrightness;

    public float valueVolume;
    public float valueBrightness;
    public float defaultVolume = 0.5f;
    public float defaultBrightness = 0.45f;
    private float lastVolume;

    public AudioSource clip;
    public AudioClip clickSound;

    // Start is called before the first frame update
    void Start()
    {
        // Volume
        valueVolume = PlayerPrefs.GetFloat("volume", defaultVolume);
        AudioListener.volume = valueVolume;
        sliderVolume.value = valueVolume;
        toggleMute.isOn = (valueVolume == 0);

        //Brightness
        valueBrightness = PlayerPrefs.GetFloat("brightness", defaultBrightness);
        sliderBrightness.value = valueBrightness;
        panelBrightness.color = new Color(panelBrightness.color.r, panelBrightness.color.g, panelBrightness.color.b, valueBrightness);
    }

    public void ChangeVolume(float value)
    {
        AudioListener.volume = value;
        toggleMute.isOn = (value == 0);
        valueVolume = value;
        PlayerPrefs.SetFloat("volume", value);
    }

    public void ChangeBrightness(float value)
    {
        panelBrightness.color = new Color(panelBrightness.color.r, panelBrightness.color.g, panelBrightness.color.b, value);
        valueBrightness = value;
        PlayerPrefs.SetFloat("brightness", value);
    }

    public void OnToggleSound(bool isMuted)
    {
        StartCoroutine(ToggleCoroutine(isMuted));
    }

    private IEnumerator ToggleCoroutine(bool isMuted)
    {
        if (!isMuted)
        {
            valueVolume = lastVolume;
            AudioListener.volume = valueVolume;
        }

        clip.Play();
        yield return new WaitForSeconds(clip.clip.length);

        if (isMuted)
        {
            lastVolume = valueVolume;
            valueVolume = 0f;
        }

        AudioListener.volume = valueVolume;
        sliderVolume.value = valueVolume;
        PlayerPrefs.SetFloat("volume", valueVolume);
    }

    public void ResetOptions()
    {
        //Volume
        AudioListener.volume = defaultVolume;
        sliderVolume.value = defaultVolume;
        toggleMute.isOn = (defaultVolume == 0);
        valueVolume = defaultVolume;
        lastVolume = defaultVolume;
        PlayerPrefs.SetFloat("volume", defaultVolume);

        //Brightness
        panelBrightness.color = new Color(panelBrightness.color.r, panelBrightness.color.g, panelBrightness.color.b, defaultBrightness);
        sliderBrightness.value = defaultBrightness;
        valueBrightness = defaultBrightness;
        PlayerPrefs.SetFloat("brightness", defaultBrightness);
    }

    public void PlayClick()
    {
        clip.Play();
    }
}