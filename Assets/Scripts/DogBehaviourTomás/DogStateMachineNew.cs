using UnityEngine;

public class DogStateMachineNew : MonoBehaviour
{
    [Header("References")]
    public DogStats dogStats;

    [Header("Needs Limits")]
    public float lowEnergyThreshold = 15f;
    public float highHungerThreshold = 80f;
    public float highThirstThreshold = 80f;
    public float poopThreshold = 100f;

    [Header("Scene")]
    public bool isInPark;

    // =====================================================
    // STATES
    // =====================================================
    public enum DogState
    {
        Idle,
        Wander,

        FollowPlayer,
        FetchBall,

        NeedFood,
        NeedWater,

        Poop,

        Sit
    }

    [Header("Current State")]
    public DogState currentState = DogState.Idle;

    // =====================================================
    // REQUESTS / ORDERS
    // =====================================================
    private bool playerCalledDog;
    private bool playerRequestedBall;

    // =====================================================
    // UPDATE
    // =====================================================
    private void Start()
    {
        if (dogStats == null)
        {
            dogStats = FindFirstObjectByType<DogStats>();
        }

        dogStats.isInPark = isInPark;
    }

    private void Update()
    {
        EvaluateState();
    }

    // =====================================================
    // STATE EVALUATION
    // =====================================================
    void EvaluateState()
    {
        // =================================================
        // 1. LOW ENERGY
        // Highest priority
        // =================================================
        if (dogStats.energy <= lowEnergyThreshold)
        {
            ChangeState(DogState.Sit);
            return;
        }

        // =================================================
        // 2. POOP
        // =================================================

        if (dogStats.bath >= poopThreshold)
        {
            ChangeState(DogState.Poop);
            return;
        }

        // =================================================
        // 3. HUNGER
        // =================================================

        if (dogStats.hunger >= highHungerThreshold)
        {
            ChangeState(DogState.NeedFood);
            return;
        }

        // =================================================
        // 4. THIRST
        // =================================================

        if (dogStats.thirst >= highThirstThreshold)
        {
            ChangeState(DogState.NeedWater);
            return;
        }

        // =================================================
        // 5. FETCH BALL
        // =================================================

        if (playerRequestedBall)
        {
            ChangeState(DogState.FetchBall);
            return;
        }

        // =================================================
        // 6. FOLLOW PLAYER
        // =================================================

        if (playerCalledDog)
        {
            ChangeState(DogState.FollowPlayer);
            return;
        }

        // =================================================
        // 7. DEFAULT BEHAVIOUR
        // =================================================
        ChangeState(DogState.Wander);
    }

    // =====================================================
    // CHANGE STATE
    // =====================================================
    void ChangeState(DogState newState)
    {
        if (currentState == newState) return;

        ExitState(currentState);
        currentState = newState;
        EnterState(newState);
    }

    // =====================================================
    // ENTER STATE
    // =====================================================
    void EnterState(DogState state)
    {
        switch (state)
        {
            case DogState.Idle:
                dogStats.currentEnergyState = DogStats.EnergyState.Idle;
                break;

            case DogState.Wander:
                dogStats.currentEnergyState = DogStats.EnergyState.Wander;
                break;

            case DogState.FollowPlayer:
                dogStats.currentEnergyState = DogStats.EnergyState.Follow;
                break;

            case DogState.FetchBall:
                dogStats.currentEnergyState = DogStats.EnergyState.Fetch;
                break;

            case DogState.NeedFood:
                dogStats.currentEnergyState = DogStats.EnergyState.Follow;
                break;

            case DogState.NeedWater:
                dogStats.currentEnergyState = DogStats.EnergyState.Follow;
                break;

            case DogState.Poop:
                dogStats.currentEnergyState = DogStats.EnergyState.Idle;
                break;

            case DogState.Sit:
                dogStats.currentEnergyState = DogStats.EnergyState.Sit;
                break;
        }
    }

    // =====================================================
    // EXIT STATE
    // =====================================================
    void ExitState(DogState state)
    {
        switch (state)
        {
            case DogState.FollowPlayer:
                playerCalledDog = false;
                break;

            case DogState.FetchBall:
                playerRequestedBall = false;
                break;
        }
    }

    // =====================================================
    // PUBLIC REQUESTS
    // =====================================================
    public void CallDog()
    {
        // Ignore if dog has urgent needs
        if (dogStats.energy <= lowEnergyThreshold)
            return;

        if (dogStats.hunger >= highHungerThreshold)
            return;

        if (dogStats.thirst >= highThirstThreshold)
            return;

        if (dogStats.bath >= poopThreshold)
            return;

        playerCalledDog = true;
    }

    public void RequestBall()
    {
        // Ignore if dog has urgent needs
        if (dogStats.energy <= lowEnergyThreshold)
            return;

        if (dogStats.hunger >= highHungerThreshold)
            return;

        if (dogStats.thirst >= highThirstThreshold)
            return;

        if (dogStats.bath >= poopThreshold)
            return;

        playerRequestedBall = true;
    }

    // =====================================================
    // COMPLETION METHODS
    // Called by controller later
    // =====================================================
    public void FinishFollowPlayer()
    {
        playerCalledDog = false;
    }

    public void FinishFetchBall()
    {
        playerRequestedBall = false;
    }
}
