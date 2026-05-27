using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Estado del Perro")]
    public DogStateMachine dogStateMachine;

    [Header("Configuraciones de Pantalla y Audio")]
    public float valueVolume;
    public float valueBrightness;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadPersistentData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // =========================================================
    // PERSISTENCIA
    // =========================================================

    public void LoadPersistentData()
    {
        // Solo datos persistentes importantes
        if (!PlayerPrefs.HasKey("dogName"))
        {
            PlayerPrefs.SetString("dogName", "Firulais");
        }

        if (!PlayerPrefs.HasKey("nFeeds"))
        {
            PlayerPrefs.SetInt("nFeeds", 0);
        }

        if (!PlayerPrefs.HasKey("nWalks"))
        {
            PlayerPrefs.SetInt("nWalks", 0);
        }

        if (!PlayerPrefs.HasKey("nBalls"))
        {
            PlayerPrefs.SetInt("nBalls", 0);
        }

        if (!PlayerPrefs.HasKey("foodBowlState"))
        {
            PlayerPrefs.SetInt("foodBowlState", 1);
        }

        if (!PlayerPrefs.HasKey("volume"))
        {
            PlayerPrefs.SetFloat("volume", 0.5f);
        }

        if (!PlayerPrefs.HasKey("brightness"))
        {
            PlayerPrefs.SetFloat("volume", 0.45f);
        }

        PlayerPrefs.Save();
    }

    // =========================================================
    // GAMEPLAY GLOBAL
    // =========================================================

    public void FeedDog()
    {
        // Actualizar estadística persistente
        int feeds = PlayerPrefs.GetInt("nFeeds", 0);
        PlayerPrefs.SetInt("nFeeds", feeds + 1);

        // Cambiar estado del cuenco
        PlayerPrefs.SetInt("foodBowlState", 0);

        if (dogStateMachine != null)
        {
            dogStateMachine.RestoreHunger();
        }
    }

    public void RestoreFood()
    {
        // Cambiar estado del cuenco
        PlayerPrefs.SetInt("foodBowlState", 1);
    }

    public void RegisterWalk(float distance)
    {
        int walks = PlayerPrefs.GetInt("nWalks", 0);
        PlayerPrefs.SetInt("nWalks", walks + 1);

        float totalDistance = PlayerPrefs.GetFloat("distance", 0f);
        PlayerPrefs.SetFloat("distance", totalDistance + distance);
    }

    public void RegisterBallPlay()
    {
        int balls = PlayerPrefs.GetInt("nBalls", 0);
        PlayerPrefs.SetInt("nBalls", balls + 1);
    }
}