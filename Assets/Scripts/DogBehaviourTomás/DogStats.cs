using UnityEngine;

public class DogStats : MonoBehaviour
{
    [Header("Current Stats")]
    [Range(0, 100)] public float energy = 100f;
    [Range(0, 100)] public float hunger = 0f;
    [Range(0, 100)] public float thirst = 0f;
    [Range(0, 100)] public float bath = 0f;

    [Header("Stats Limits")]
    public float maxStat = 100f;
    public float minStat = 0f;

    [Header("Hunger")]
    public float hungerIncreaseAmount = 1f;
    public float hungerIncreaseInterval = 30f;

    [Header("Thirst")]
    public float thirstIncreaseAmount = 1f;
    public float thirstIncreaseInterval = 25f;

    [Header("Bath / Poop")] //hacer el aumento o tiempo aleatorio para mas realismo?
    public float bathIncreaseAmount = 1f;
    public float bathIncreaseInterval = 40f;

    [Header("Energy Recovery")]
    public float idleEnergyRecovery = 1f;
    public float idleRecoveryInterval = 5f;

    public float sitEnergyRecovery = 2f;
    public float sitRecoveryInterval = 4f;

    public float sleepEnergyRecovery = 5f;
    public float sleepRecoveryInterval = 2f;

    [Header("Energy Consumption")]
    public float wanderEnergyConsumption = 1f;
    public float wanderConsumptionInterval = 8f;

    public float followEnergyConsumption = 1f;
    public float followConsumptionInterval = 6f;

    public float fetchEnergyConsumption = 2f;
    public float fetchConsumptionInterval = 3f;

    [Header("Park Multipliers")]
    public float parkHungerMultiplier = 1.25f;
    public float parkThirstMultiplier = 1.5f;

    [Header("Statistics")]
    public int nFeeds;
    public int nBalls;
    public int nWalks;
    public int nPoops;

    public float distance;

    public string dogName = "Tobby";

    // Timers
    private float hungerTimer;
    private float thirstTimer;
    private float bathTimer;
    private float energyTimer;

    // Runtime
    private Vector3 lastPosition;
    [HideInInspector] public bool isInPark;

    // Energy states
    public enum EnergyState
    {
        Idle,
        Wander,
        Follow,
        Fetch,
        Sit,
        Sleep
    }

    [HideInInspector]
    public EnergyState currentEnergyState = EnergyState.Idle;

    private void Awake()
    {
        // Singleton simple
        DogStats[] managers = FindObjectsByType<DogStats>(FindObjectsSortMode.None);

        if (managers.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        LoadStats();
    }

    private void Start()
    {
        lastPosition = transform.position;
    }

    private void Update()
    {
        UpdateNeeds();
        UpdateEnergy();
        UpdateDistance();
    }

    void UpdateNeeds()
    {
        // HUNGER
        hungerTimer += Time.deltaTime;

        if (hungerTimer >= hungerIncreaseInterval)
        {
            hungerTimer = 0f;
            float amount = hungerIncreaseAmount;

            if (isInPark)
            {
                amount *= parkHungerMultiplier;
            }

            hunger += amount;
            hunger = Mathf.Clamp(hunger, minStat, maxStat);
        }

        // THIRST
        thirstTimer += Time.deltaTime;

        if (thirstTimer >= thirstIncreaseInterval)
        {
            thirstTimer = 0f;
            float amount = thirstIncreaseAmount;

            if (isInPark)
            {
                amount *= parkThirstMultiplier;
            }

            thirst += amount;
            thirst = Mathf.Clamp(thirst, minStat, maxStat);
        }

        // BATH / POOP
        bathTimer += Time.deltaTime;

        if (bathTimer >= bathIncreaseInterval)
        {
            bathTimer = 0f;

            // Aleatorio para hacerlo menos exacto
            float randomAmount = Random.Range(bathIncreaseAmount * 0.5f, bathIncreaseAmount * 1.5f);
            bath += randomAmount;
            bath = Mathf.Clamp(bath, minStat, maxStat);
        }
    }

    void UpdateEnergy()
    {
        float interval = 0f;
        float amount = 0f;
        bool recovering = false;

        switch (currentEnergyState)
        {
            case EnergyState.Idle:
                interval = idleRecoveryInterval;
                amount = idleEnergyRecovery;
                recovering = true;
                break;

            case EnergyState.Wander:
                interval = wanderConsumptionInterval;
                amount = wanderEnergyConsumption;
                break;

            case EnergyState.Follow:
                interval = followConsumptionInterval;
                amount = followEnergyConsumption;
                break;

            case EnergyState.Fetch:
                interval = fetchConsumptionInterval;
                amount = fetchEnergyConsumption;
                break;

            case EnergyState.Sit:
                interval = sitRecoveryInterval;
                amount = sitEnergyRecovery;
                recovering = true;
                break;

            case EnergyState.Sleep:
                interval = sleepRecoveryInterval;
                amount = sleepEnergyRecovery;
                recovering = true;
                break;
        }

        energyTimer += Time.deltaTime;

        if (energyTimer >= interval)
        {
            energyTimer = 0f;

            if (recovering)
            {
                energy += amount;
            }
            else
            {
                energy -= amount;
            }

            energy = Mathf.Clamp(energy, minStat, maxStat);
        }
    }

    void UpdateDistance()
    {
        if (!isInPark) return;

        float movedDistance = Vector3.Distance(transform.position, lastPosition);
        distance += movedDistance;
        lastPosition = transform.position;
    }

    // =====================================================
    // PUBLIC METHODS
    // =====================================================
    public void FeedDog(float amount)
    {
        hunger -= amount;
        hunger = Mathf.Clamp(hunger, minStat, maxStat);
        nFeeds++;
        SaveStats();
    }

    public void Drink(float amount)
    {
        thirst -= amount;
        thirst = Mathf.Clamp(thirst, minStat, maxStat);
        SaveStats();
    }

    public void RegisterBallThrow()
    {
        nBalls++;
        SaveStats();
    }

    public void RegisterWalk()
    {
        nWalks++;
        SaveStats();
    }

    public void RegisterPoop()
    {
        nPoops++;
        bath = 0f;
        SaveStats();
    }

    public void SetDogName(string newName)
    {
        dogName = newName;
        SaveStats();
    }

    // =====================================================
    // PLAYER PREFS
    // =====================================================
    public void SaveStats()
    {
        PlayerPrefs.SetFloat("energy", energy);
        PlayerPrefs.SetFloat("hunger", hunger);
        PlayerPrefs.SetFloat("thirst", thirst);
        PlayerPrefs.SetFloat("bath", bath);

        PlayerPrefs.SetInt("nFeeds", nFeeds);
        PlayerPrefs.SetInt("nBalls", nBalls);
        PlayerPrefs.SetInt("nWalks", nWalks);
        PlayerPrefs.SetInt("nPoops", nPoops);

        PlayerPrefs.SetFloat("distance", distance);

        PlayerPrefs.SetString("dogName", dogName);

        PlayerPrefs.Save();
    }

    public void LoadStats()
    {
        energy = PlayerPrefs.GetFloat("energy", 100f);
        hunger = PlayerPrefs.GetFloat("hunger", 0f);
        thirst = PlayerPrefs.GetFloat("thirst", 0f);
        bath = PlayerPrefs.GetFloat("bath", 0f);

        nFeeds = PlayerPrefs.GetInt("nFeeds", 0);
        nBalls = PlayerPrefs.GetInt("nBalls", 0);
        nWalks = PlayerPrefs.GetInt("nWalks", 0);
        nPoops = PlayerPrefs.GetInt("nPoops", 0);

        distance = PlayerPrefs.GetFloat("distance", 0f);

        dogName = PlayerPrefs.GetString("dogName", "Tobby");
    }

    private void OnApplicationQuit()
    {
        SaveStats();
    }
} 
