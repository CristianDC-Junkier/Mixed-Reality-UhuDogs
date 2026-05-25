using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Audio;

public class DogMenuController : MonoBehaviour
{
    public TMP_InputField nameField;

    public TMP_Text savedText;
    public TMP_Text nEnergy;
    public TMP_Text nHunger;
    public TMP_Text nThirst;
    public TMP_Text nSleep;

    public Image barEnergy;
    public Image barHunger;
    public Image barThirst;
    public Image barSleep;

    public AudioSource clip;

    private void OnEnable()
    {
        // Dog name & reset saved text
        nameField.text = PlayerPrefs.GetString("dogName", "Tobby");
        savedText.text = "";

        // Percentages
        int energy = PlayerPrefs.GetInt("energy", 100);
        int hunger = PlayerPrefs.GetInt("hunger", 0);
        int thirst = PlayerPrefs.GetInt("thirst", 0);
        int sleep = PlayerPrefs.GetInt("sleep", 0);

        // Update stats
        UpdateStat(energy, nEnergy, barEnergy);
        UpdateStat(hunger, nHunger, barHunger);
        UpdateStat(thirst, nThirst, barThirst);
        UpdateStat(sleep, nSleep, barSleep);
    }

    private void UpdateStat(int value, TMP_Text text, Image bar)
    {
        text.text = value + "%";
        bar.fillAmount = value / 100f;

        if (value > 55)
            text.color = Color.white;
        else
            text.color = Color.black;
    }

    public void SaveName()
    {
        PlayerPrefs.SetString("dogName", nameField.text);
        PlayerPrefs.Save();
        savedText.text = "Guardado";
    }

    public void PlayClick()
    {
        clip.Play();
    }
}
