using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class DogParkAI : MonoBehaviour
{
    [Header("Referencias")]
    public Transform playerTarget;
    public Transform ball;
    public Transform mouthPoint;

    [Header("Pelota")]
    public float minBallDistanceFromPlayer = 3f;
    public float ballStopDistance = 2f;
    public float bringBallStopDistance = 4f;

    [Header("Hambre")]
    [Range(1, 100)] public int hunger = 100;
    public int hungryThreshold = 20;
    public float hungerTickAbove50 = 5f;
    public float hungerTickBelow50 = 10f;

    [Header("Sed")]
    [Range(1, 100)] public int thirst = 100;
    public int thirstThreshold = 20;
    public float drinkTickAbove50 = 5f;
    public float drinkTickBelow50 = 10f;

    [Header("Energía")]
    [Range(1, 100)] public int energy = 100;
    public int sleepThreshold = 20;
    public float energyTickAbove50 = 5f;
    public float energyTickBelow50 = 10f;

    [Header("Baño")]
    [Range(1, 100)] public int bath = 100;
    public int bathThreshold = 30;
    public int fullAfterPoop = 100;
    public float poopAnimationDuration = 3.5f;
    public float bathTickAbove50 = 5f;
    public float bathTickBelow50 = 10f;
    public GameObject poopPrefab;
    public int maxPoops = 5;
    public float popDeleteTime = 120f;

    [Header("Movimiento")]
    public float wanderRadius = 8f;
    public float minWalkTime = 6f;
    public float maxWalkTime = 14f;
    public float pauseBetweenWalks = 0.3f;

    [Header("Seguir jugador / Llamada")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 6f;
    public float playerStopDistance = 4f;
    public float barkRepeatTime = 2f;

    [Header("Ladrido")]
    public AudioSource audioSource;
    public AudioClip barkSound;
    public float minBarkTime = 35f;
    public float maxBarkTime = 90f;
    public float barkDuration = 1.5f;

    [Header("Voces")]
    public AudioClip energyMan;
    public AudioClip energyWomen;
    public AudioClip HungryMan;
    public AudioClip HungryWomen;
    public AudioClip ThirstyMan;
    public AudioClip ThirstyWomen;

    private Animator animator;
    private NavMeshAgent agent;

    private bool isComingToPlayer;
    private bool isCarryingBall;
    private bool isFollowBark;
    private bool isBarking;
    private bool isPooping;

    private float nextBarkTime;
    private int currentPoops;

    private float totalDistanceWalked;
    private Vector3 lastPosition;

    private int gender;

    private AudioClip GetUrgentNeedAudio()
    {
        if (hunger < hungryThreshold)
        {
            return (gender == 0) ? HungryMan : HungryWomen;
        }
        else if (thirst < thirstThreshold )
        {
            return (gender == 0) ? ThirstyMan : ThirstyWomen;
        }
        else if (energy < sleepThreshold)
        {
            return (gender == 0) ? energyMan : energyWomen;
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

        totalDistanceWalked = PlayerPrefs.GetFloat("distance", 0f);

        lastPosition = transform.position;

        nextBarkTime = Time.time + Random.Range(minBarkTime, maxBarkTime);

        StartCoroutine(HungerRoutine());
        StartCoroutine(ThirstRoutine());
        StartCoroutine(EnergyRoutine());
        StartCoroutine(BathRoutine());
        StartCoroutine(MainRoutine());
        StartCoroutine(AutoSaveRoutine());
    }

    void Update()
    {
        TrackDistance();

        if (OVRInput.GetDown(OVRInput.Button.Three))
        {
            if (!HasUrgentNeeds()) CallDog();
        }

        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            if (!HasUrgentNeeds()) FetchBall();
        }

        if (!isCarryingBall && ball != null)
        {
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb != null && rb.linearVelocity.magnitude < 0.05f)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        UpdateBallCarry();
    }

    void TrackDistance()
    {
        float frameDistance = Vector3.Distance(transform.position, lastPosition);

        if (frameDistance > 0.001f)
        {
            totalDistanceWalked += frameDistance;
            PlayerPrefs.SetFloat("distance", totalDistanceWalked);
        }

        lastPosition = transform.position;
    }

    bool HasUrgentNeeds()
    {
        return hunger < hungryThreshold || thirst < thirstThreshold || energy < sleepThreshold || bath < bathThreshold;
    }

    void SaveStats()
    {
        PlayerPrefs.SetInt("hunger", hunger);
        PlayerPrefs.SetInt("thirst", thirst);
        PlayerPrefs.SetInt("energy", energy);
        PlayerPrefs.SetInt("bath", bath);
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
            hunger = Mathf.Clamp(hunger - 1, 1, 100);
            SaveStats();
        }
    }

    IEnumerator ThirstRoutine()
    {
        while (true)
        {
            float waitTime = thirst > 50 ? drinkTickAbove50 : drinkTickBelow50;
            yield return new WaitForSeconds(waitTime);
            thirst = Mathf.Clamp(thirst - 1, 1, 100);
            SaveStats();
        }
    }

    IEnumerator EnergyRoutine()
    {
        while (true)
        {
            float waitTime = energy > 50 ? energyTickAbove50 : energyTickBelow50;
            yield return new WaitForSeconds(waitTime);
            energy = Mathf.Clamp(energy - 1, 1, 100);
            SaveStats();
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
            if (isComingToPlayer || isCarryingBall || isPooping)
            {
                yield return null;
                continue;
            }

            if (HasUrgentNeeds())
            {
                if (bath < bathThreshold && currentPoops < maxPoops)
                {
                    yield return GoPoop();
                }
                else if (hunger < hungryThreshold || thirst < thirstThreshold || energy < sleepThreshold)
                {
                    if (!isFollowBark) yield return GoFollowBark();
                    else yield return null;
                }
            }
            else
            {
                yield return WanderForAWhile();
            }

            if (!HasUrgentNeeds() && !isComingToPlayer && !isCarryingBall && !isFollowBark && !isBarking && Time.time >= nextBarkTime)
            {
                yield return BarkRoutine();
            }

            yield return new WaitForSeconds(pauseBetweenWalks);
        }
    }

    IEnumerator WanderForAWhile()
    {
        if (isComingToPlayer || isCarryingBall || isFollowBark || isBarking || isPooping || HasUrgentNeeds()) yield break;

        agent.speed = walkSpeed;
        Vector3 destination = GetRandomNavMeshPoint(transform.position, wanderRadius);

        agent.isStopped = false;
        agent.stoppingDistance = 0f;
        agent.SetDestination(destination);
        animator.SetBool("walking", true);

        float walkTime = Random.Range(minWalkTime, maxWalkTime);
        float timer = 0f;

        while (!isComingToPlayer && !isCarryingBall && !isFollowBark && !isBarking && !isPooping && !HasUrgentNeeds() && timer < walkTime &&
              (agent.pathPending || agent.remainingDistance > agent.stoppingDistance + 0.1f || agent.velocity.magnitude > 0.05f))
        {
            timer += Time.deltaTime;
            yield return null;
        }

        animator.SetBool("walking", false);
    }

    IEnumerator GoPoop()
    {
        if (isPooping) yield break;
        isPooping = true;

        agent.ResetPath();
        agent.isStopped = true;
        animator.SetBool("walking", false);
        animator.SetBool("run", false);

        animator.SetTrigger("sit");
        yield return new WaitForSeconds(1.5f);

        if (poopPrefab != null)
        {
            Vector3 poopPosition = transform.position - transform.forward * 1.25f;
            GameObject poop = Instantiate(poopPrefab, poopPosition, Quaternion.identity);

            PoopBehaviour pb = poop.GetComponent<PoopBehaviour>();
            if (pb != null) pb.dog = this;

            currentPoops++;
            StartCoroutine(RemovePoopAfterTime(poop, popDeleteTime));
        }

        yield return new WaitForSeconds(poopAnimationDuration - 1.5f);

        bath = fullAfterPoop;
        SaveStats();

        agent.isStopped = false;
        isPooping = false;
    }

    public void RemovePoopFromCount()
    {
        currentPoops = Mathf.Max(currentPoops - 1, 0);
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

        while (HasUrgentNeeds())
        {
            if (isPooping)
            {
                yield return null;
                continue;
            }

            if (Time.time >= nextPathUpdateTime)
            {
                nextPathUpdateTime = Time.time + pathUpdateInterval;
                agent.SetDestination(playerTarget.position);
            }

            if (agent.remainingDistance > agent.stoppingDistance + 0.1f && agent.velocity.magnitude > 0.1f)
            {
                animator.SetBool("run", true);
                animator.SetBool("walking", false);
            }
            else
            {
                animator.SetBool("run", false);
                animator.SetBool("walking", false);
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
        if (isComingToPlayer || isCarryingBall || isFollowBark || isPooping || HasUrgentNeeds()) yield break;
        isBarking = true;

        agent.ResetPath();
        agent.isStopped = true;
        animator.SetBool("walking", false);
        animator.SetBool("run", false);
        animator.SetTrigger("bark");

        if (audioSource != null && barkSound != null) {
            audioSource.PlayOneShot(barkSound);
            yield return new WaitForSeconds(barkSound.length);
        }

        nextBarkTime = Time.time + Random.Range(minBarkTime, maxBarkTime);
        agent.isStopped = false;
        isBarking = false;
    }

    public void CallDog()
    {
        if (!isComingToPlayer && !isCarryingBall && !isBarking && !isPooping && !HasUrgentNeeds())
        {
            StartCoroutine(ComeToPlayerRoutine());
        }
    }

    public void FetchBall()
    {
        if (isComingToPlayer || isCarryingBall || isBarking || isPooping || HasUrgentNeeds() || ball == null || playerTarget == null)
            return;

        float distance = Vector3.Distance(playerTarget.position, ball.position);
        if (distance < minBallDistanceFromPlayer) return;

        StartCoroutine(FetchBallRoutine());
    }

    IEnumerator ComeToPlayerRoutine()
    {
        if (playerTarget == null) yield break;
        isComingToPlayer = true;

        agent.isStopped = false;
        agent.speed = runSpeed;
        agent.stoppingDistance = playerStopDistance;

        float pathUpdateInterval = 0.3f;
        float nextPathUpdateTime = 0f;

        animator.SetBool("walking", false);
        animator.SetBool("run", true);
        yield return new WaitForSeconds(1.75f);

        while (isComingToPlayer)
        {
            if (Time.time >= nextPathUpdateTime)
            {
                nextPathUpdateTime = Time.time + pathUpdateInterval;
                agent.SetDestination(playerTarget.position);
            }

            if (agent.remainingDistance > agent.stoppingDistance + 0.1f && agent.velocity.magnitude > 0.1f)
            {
                animator.SetBool("run", true);
                animator.SetBool("walking", false);
            }
            else
            {
                animator.SetBool("run", false);
                animator.SetBool("walking", false);

                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    break;
                }
            }

            yield return null;
        }

        animator.SetBool("run", false);
        animator.SetBool("walking", false);
        yield return new WaitForSeconds(2f);
        agent.stoppingDistance = 0f;
        isComingToPlayer = false;
    }

    IEnumerator FetchBallRoutine()
    {
        if (ball == null || playerTarget == null) yield break;

        isComingToPlayer = true; 
        agent.isStopped = false;
        agent.speed = runSpeed;
        agent.stoppingDistance = ballStopDistance;

        float pathUpdateInterval = 0.3f;
        float nextPathUpdateTime = 0f;

        animator.SetBool("walking", false);
        animator.SetBool("run", true);
        yield return new WaitForSeconds(1.75f);

        while (true)
        {
            if (Time.time >= nextPathUpdateTime)
            {
                nextPathUpdateTime = Time.time + pathUpdateInterval;
                agent.SetDestination(ball.position);
            }

            if (agent.remainingDistance > agent.stoppingDistance + 0.1f && agent.velocity.magnitude > 0.1f)
            {
                animator.SetBool("run", true);
                animator.SetBool("walking", false);
            }
            else
            {
                animator.SetBool("run", false);
                animator.SetBool("walking", false);

                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    break;
                }
            }
            yield return null;
        }

        Rigidbody ballRb = ball.GetComponent<Rigidbody>();
        Collider ballCollider = ball.GetComponent<Collider>();

        if (ballRb != null) ballRb.isKinematic = true;
        if (ballCollider != null) ballCollider.enabled = false;

        isCarryingBall = true;
        isComingToPlayer = false; 
        agent.stoppingDistance = bringBallStopDistance;

        nextPathUpdateTime = 0f;

        while (isCarryingBall)
        {
            if (Time.time >= nextPathUpdateTime)
            {
                nextPathUpdateTime = Time.time + pathUpdateInterval;
                agent.SetDestination(playerTarget.position);
            }

            if (agent.remainingDistance > agent.stoppingDistance + 0.1f && agent.velocity.magnitude > 0.1f)
            {
                animator.SetBool("run", true);
                animator.SetBool("walking", false);
            }
            else
            {
                animator.SetBool("run", false);
                animator.SetBool("walking", false);

                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    break;
                }
            }
            yield return null;
        }

        isCarryingBall = false;
        ball.position = playerTarget.position + playerTarget.forward * 1.2f;

        if (ballCollider != null) ballCollider.enabled = true;
        if (ballRb != null) ballRb.isKinematic = false;

        animator.SetBool("run", false);
        animator.SetBool("walking", false);
        yield return new WaitForSeconds(2f);
        agent.stoppingDistance = 0f;
    }

    void UpdateBallCarry()
    {
        if (!isCarryingBall || ball == null || mouthPoint == null) return;
        ball.position = mouthPoint.position;
        ball.rotation = mouthPoint.rotation;
    }

    IEnumerator RemovePoopAfterTime(GameObject poop, float time)
    {
        yield return new WaitForSeconds(time);
        if (poop != null)
        {
            Destroy(poop);
            currentPoops = Mathf.Max(currentPoops - 1, 0);
        }
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