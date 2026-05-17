using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DogMenuController : MonoBehaviour
{
    public TMP_InputField nameField;
    public TMP_Text nEnergy;
    public TMP_Text nHunger;
    public TMP_Text nThirst;
    public TMP_Text nSleep;
    public Image barEnergy;
    public Image barHunger;
    public Image barThirst;
    public Image barSleep;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        // Dog name
        nameField.text = PlayerPrefs.GetString("dogName", "Tobby");

        // Percentages
        int energy = PlayerPrefs.GetInt("energy", 100);
        int hunger = PlayerPrefs.GetInt("hunger", 0);
        int thirst = PlayerPrefs.GetInt("thirst", 0);
        int sleep = PlayerPrefs.GetInt("sleep", 0);

        nEnergy.text = energy + "%";
        nHunger.text = hunger + "%";
        nThirst.text = thirst + "%";
        nSleep.text = sleep + "%";

        barEnergy.fillAmount = energy / 100f;
        barHunger.fillAmount = hunger / 100f;
        barThirst.fillAmount = thirst / 100f;
        barSleep.fillAmount = sleep / 100f;
    }

    public void SaveName()
    {
        PlayerPrefs.SetString("dogName", nameField.text);
        PlayerPrefs.Save();
    }
}
