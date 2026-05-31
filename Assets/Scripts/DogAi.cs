using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class DogAI : MonoBehaviour
{
    [Header("Referencias")]
    public Transform bowlTarget;
    public Transform bedTarget;

    [Header("Comida")]
    public GameObject foodObjectToHide;
    public int food;

    [Header("Bebida")]
    public GameObject waterObjectToHide;
    public int water;

    [Header("Posición exacta comida")]
    public Vector3 eatWorldPosition;
    public float eatRotationY = 0f;

    [Header("Posición exacta bebida")]
    public Vector3 waterWorldPosition;
    public float waterRotationY = 0f;

    [Header("Posición exacta dormir")]
    public Vector3 sleepWorldPosition;
    public float sleepRotationY = 0f;

    [Header("Hambre")]
    [Range(1, 100)] public int hunger = 100;
    public int hungryThreshold = 20;
    public int fullAfterEating = 100;
    public float eatAnimationDuration = 3.5f;
    public float hungerTickAbove50 = 5f;
    public float hungerTickBelow50 = 10f;

    [Header("Sed")]
    [Range(1, 100)] public int thirst = 100;
    public int thirstThreshold = 20;
    public int fullAfterDrink = 100;
    public float drinkAnimationDuration = 3.5f;
    public float drinkTickAbove50 = 5f;
    public float drinkTickBelow50 = 10f;

    [Header("Energía")]
    [Range(1, 100)] public int energy = 100;
    public int sleepThreshold = 20;
    public int fullAfterSleep = 100;
    public float sleepAnimationDuration = 20f;
    public float energyTickAbove50 = 5f;
    public float energyTickBelow50 = 10f;

    [Header("Baño")]
    [Range(1, 100)] public int bath = 100;
    public int bathThreshold = 30;
    public float bathTickAbove50 = 5f;
    public float bathTickBelow50 = 10f;

    [Header("Seguir Jugador")]
    public Transform playerTarget;
    public float walkSpeed = 3.5f;
    public float runSpeed = 6f;
    public float playerStopDistance = 4f;
    public float barkRepeatTime = 2f;

    [Header("Movimiento")]
    public float wanderRadius = 4f;
    public float minWalkTime = 6f;
    public float maxWalkTime = 14f;
    public float pauseBetweenWalks = 0.3f;

    [Header("Ladrido")]
    public AudioSource audioSource;
    public AudioClip barkSound;
    public float minBarkTime = 35f;
    public float maxBarkTime = 90f;
    public float barkDuration = 1.5f;

    [Header("Voces")]
    public AudioClip bathMan;
    public AudioClip bathWomen;
    public AudioClip HungryMan;
    public AudioClip HungryWomen;
    public AudioClip ThirstyMan;
    public AudioClip ThirstyWomen;

    private Animator animator;
    private NavMeshAgent agent;

    private bool isEating;
    private bool isDrinking;
    private bool isSleeping;
    private bool isFollowBark;
    private bool isBarking;

    private float nextBarkTime;

    private int gender;

    private AudioClip GetUrgentNeedAudio()
    {
        if (hunger < hungryThreshold && food == 0)
        {
            return (gender == 0) ? HungryMan : HungryWomen;
        }
        else if (thirst < thirstThreshold && water == 0)
        {
            return (gender == 0) ? ThirstyMan : ThirstyWomen;
        }
        else if (bath < bathThreshold)
        {
            return (gender == 0) ? bathMan : bathWomen;
        }

        return null; 
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        if (agent != null) walkSpeed = agent.speed;

        hunger = PlayerPrefs.GetInt("hunger", hunger);
        thirst = PlayerPrefs.GetInt("thirst", thirst);
        energy = PlayerPrefs.GetInt("energy", energy);
        bath = PlayerPrefs.GetInt("bath", bath);
        food = PlayerPrefs.GetInt("food", 1);
        water = PlayerPrefs.GetInt("water", 1);

        nextBarkTime = Time.time + Random.Range(minBarkTime, maxBarkTime);

        StartCoroutine(HungerRoutine());
        StartCoroutine(ThirstRoutine());
        StartCoroutine(EnergyRoutine());
        StartCoroutine(BathRoutine());
        StartCoroutine(MainRoutine());
        StartCoroutine(AutoSaveRoutine());
    }

    void SaveStats()
    {
        PlayerPrefs.SetInt("hunger", hunger);
        PlayerPrefs.SetInt("thirst", thirst);
        PlayerPrefs.SetInt("energy", energy);
        PlayerPrefs.SetInt("bath", bath);
        PlayerPrefs.SetInt("food", food);
        PlayerPrefs.SetInt("water", water);
    }

    IEnumerator AutoSaveRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(30f);
            PlayerPrefs.Save();
        }
    }

    IEnumerator HungerRoutine()
    {
        while (true)
        {
            float waitTime = hunger > 50 ? hungerTickAbove50 : hungerTickBelow50;
            yield return new WaitForSeconds(waitTime);

            if (!isEating)
            {
                hunger = Mathf.Clamp(hunger - 1, 1, 100);
                SaveStats();
            }
        }
    }

    IEnumerator ThirstRoutine()
    {
        while (true)
        {
            float waitTime = thirst > 50 ? drinkTickAbove50 : drinkTickBelow50;
            yield return new WaitForSeconds(waitTime);

            if (!isDrinking)
            {
                thirst = Mathf.Clamp(thirst - 1, 1, 100);
                SaveStats();
            }
        }
    }

    IEnumerator EnergyRoutine()
    {
        while (true)
        {
            float waitTime = energy > 50 ? energyTickAbove50 : energyTickBelow50;
            yield return new WaitForSeconds(waitTime);

            if (!isSleeping)
            {
                energy = Mathf.Clamp(energy - 1, 1, 100);
                SaveStats();
            }
        }
    }

    IEnumerator BathRoutine()
    {
        while (true)
        {
            float waitTime = bath > 50 ? bathTickAbove50 : bathTickBelow50;
            yield return new WaitForSeconds(waitTime);

            bath = Mathf.Clamp(bath - 1, 1, 100);
            SaveStats();
        }
    }

    IEnumerator MainRoutine()
    {
        while (true)
        {
            food = PlayerPrefs.GetInt("food", 0);
            water = PlayerPrefs.GetInt("water", 0);

            if (!isEating && hunger < hungryThreshold)
            {
                if (food > 0) yield return GoEat();
                else if (!isFollowBark) yield return GoFollowBark();
                else yield return null;
            }
            else if (!isDrinking && thirst < thirstThreshold)
            {
                if (water > 0) yield return GoDrink();
                else if (!isFollowBark) yield return GoFollowBark();
                else yield return null;
            }
            else if (!isSleeping && energy < sleepThreshold) yield return GoSleep();
            else if (!isFollowBark && bath < bathThreshold) yield return GoFollowBark();
            else yield return WanderForAWhile();

            if (!isEating && !isDrinking && !isSleeping && !isFollowBark && !isBarking && Time.time >= nextBarkTime)
            {
                yield return BarkRoutine();
            }

            yield return new WaitForSeconds(pauseBetweenWalks);
        }
    }

    IEnumerator WanderForAWhile()
    {
        if (isEating || isDrinking || isSleeping || isFollowBark || isBarking) yield break;

        agent.speed = walkSpeed;
        Vector3 destination = GetRandomNavMeshPoint(transform.position, wanderRadius);

        agent.isStopped = false;
        agent.SetDestination(destination);
        animator.SetBool("walking", true);

        float walkTime = Random.Range(minWalkTime, maxWalkTime);
        float timer = 0f;

        while (!isEating && !isDrinking && !isSleeping && !isFollowBark && !isBarking && timer < walkTime &&
              (agent.pathPending || agent.remainingDistance > agent.stoppingDistance + 0.1f || agent.velocity.magnitude > 0.05f))
        {
            timer += Time.deltaTime;
            yield return null;
        }

        animator.SetBool("walking", false);
    }

    IEnumerator GoEat()
    {
        if (bowlTarget == null || isEating) yield break;
        isEating = true;

        agent.speed = walkSpeed;
        agent.isStopped = false;
        agent.SetDestination(bowlTarget.position);
        animator.SetBool("walking", true);

        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance + 0.1f) yield return null;

        ResetAgentToPosition(eatWorldPosition, eatRotationY);

        animator.SetTrigger("eating");
        yield return new WaitForSeconds(eatAnimationDuration);

        if (foodObjectToHide != null) foodObjectToHide.SetActive(false);

        hunger = Mathf.Clamp(fullAfterEating, 1, 100);
        food = 0;
        SaveStats();

        yield return new WaitForSeconds(0.2f);
        agent.isStopped = false;
        isEating = false;
    }

    IEnumerator GoDrink()
    {
        if (bowlTarget == null || isDrinking) yield break;
        isDrinking = true;

        agent.speed = walkSpeed;
        agent.isStopped = false;
        agent.SetDestination(bowlTarget.position);
        animator.SetBool("walking", true);

        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance + 0.1f) yield return null;

        ResetAgentToPosition(waterWorldPosition, waterRotationY);

        animator.SetTrigger("drinking");
        yield return new WaitForSeconds(drinkAnimationDuration);

        if (waterObjectToHide != null) waterObjectToHide.SetActive(false);

        thirst = Mathf.Clamp(fullAfterDrink, 1, 100);
        water = 0;
        SaveStats();

        yield return new WaitForSeconds(0.2f);
        agent.isStopped = false;
        isDrinking = false;
    }

    IEnumerator GoSleep()
    {
        if (bedTarget == null || isSleeping) yield break;
        isSleeping = true;

        agent.speed = walkSpeed;
        agent.isStopped = false;
        agent.SetDestination(bedTarget.position);
        animator.SetBool("walking", true);

        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance + 0.1f) yield return null;

        ResetAgentToPosition(sleepWorldPosition, sleepRotationY);

        animator.SetTrigger("sit");

        yield return new WaitForSeconds(1.2f);

        animator.speed = 0f;
        yield return new WaitForSeconds(sleepAnimationDuration);

        energy = Mathf.Clamp(fullAfterSleep, 1, 100);
        SaveStats();

        animator.speed = 1f;

        yield return new WaitForSeconds(1f);

        agent.isStopped = false;
        isSleeping = false;
    }

    IEnumerator GoFollowBark()
    {
        if (playerTarget == null) yield break;
        isFollowBark = true;

        gender = PlayerPrefs.GetInt("gender", 0);

        agent.isStopped = false;
        agent.speed = runSpeed;
        agent.stoppingDistance = playerStopDistance;

        int barkCount = 0;
        float barkTimer = 0f;
        float pathUpdateInterval = 0.3f;
        float nextPathUpdateTime = 0f;

        animator.SetBool("walking", false);
        animator.SetBool("run", true);
        yield return new WaitForSeconds(1.75f);

        while (bath < bathThreshold ||
              (hunger < hungryThreshold && food == 0) ||
              (thirst < thirstThreshold && water == 0))
        {

            if (Time.time >= nextPathUpdateTime)
            {
                nextPathUpdateTime = Time.time + pathUpdateInterval;
                agent.SetDestination(playerTarget.position);

                food = PlayerPrefs.GetInt("food", 0);
                water = PlayerPrefs.GetInt("water", 0);
            }

            if (agent.remainingDistance > agent.stoppingDistance + 0.1f && agent.velocity.magnitude > 0.1f)
            {
                animator.SetBool("run", true);
            }
            else
            {
                animator.SetBool("run", false);
            }

            barkTimer += Time.deltaTime;

            if (barkTimer >= barkRepeatTime)
            {
                if (agent.remainingDistance <= agent.stoppingDistance + 0.3f)
                {
                    barkCount++;

                    if (barkCount >= 5)
                    {
                        barkCount = 0; 

                        AudioClip clipToPlay = GetUrgentNeedAudio();

                        if (audioSource != null && clipToPlay != null)
                        {
                            audioSource.PlayOneShot(clipToPlay);
                        }
                    }
                    else
                    {
                        animator.SetTrigger("bark");

                        if (audioSource != null && barkSound != null) audioSource.PlayOneShot(barkSound);
                    }

                    barkTimer = 0f;
                }
            }

            yield return null;
        }

        animator.SetBool("run", false);
        agent.stoppingDistance = 0f;
        agent.speed = walkSpeed;
        isFollowBark = false;
    }

    IEnumerator BarkRoutine()
    {
        if (isEating || isDrinking || isSleeping || isFollowBark) yield break;
        isBarking = true;

        agent.ResetPath();
        agent.isStopped = true;
        animator.SetBool("walking", false);
        animator.SetTrigger("bark");

        if (audioSource != null && barkSound != null)
        {
            audioSource.PlayOneShot(barkSound);
            yield return new WaitForSeconds(barkSound.length);
        }

        nextBarkTime = Time.time + Random.Range(minBarkTime, maxBarkTime);
        agent.isStopped = false;
        isBarking = false;
    }

    void ResetAgentToPosition(Vector3 targetPos, float rotationY)
    {
        agent.ResetPath();
        agent.isStopped = true;
        animator.SetBool("walking", false);

        transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
        agent.Warp(targetPos);
    }

    Vector3 GetRandomNavMeshPoint(Vector3 center, float radius)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * radius;
            randomPoint.y = center.y;

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, radius, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        return center;
    }
}