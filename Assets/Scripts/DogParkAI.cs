using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class DogParkAI : MonoBehaviour
{
    [Header("Referencias")]
    public Transform playerTarget;
    public Transform ball;
    public Transform mouthPoint;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip barkSound;

    [Header("Necesidades")]
    [Range(1, 100)] public int hunger = 100;
    [Range(1, 100)] public int thirst = 100;

    public int hungryThreshold = 40;
    public int thirstThreshold = 40;

    public float hungerTickAbove50 = 5f;
    public float hungerTickBelow50 = 10f;

    public float thirstTickAbove50 = 5f;
    public float thirstTickBelow50 = 10f;

    [Header("Seguir jugador por necesidad")]
    public float needStopDistance = 3f;
    public float needBarkRepeatTime = 2f;
    public float needPathUpdateInterval = 0.3f;

    [Header("Pelota")]
    public float minBallDistanceFromPlayer = 3f;
    public float ballStopDistance = 2f;
    public float bringBallStopDistance = 2f;

    [Header("Movimiento")]
    public float walkSpeed = 1.4f;
    public float runSpeed = 4f;
    public float wanderRadius = 8f;
    public float playerStopDistance = 1.5f;

    [Header("Comportamiento")]
    public float lookAtPlayerTime = 5f;
    public float barkDuration = 1.5f;
    public float minBarkTime = 25f;
    public float maxBarkTime = 60f;

    [Header("Cacas")]
    public GameObject poopPrefab;
    public float minPoopTime = 40f;
    public float maxPoopTime = 90f;
    public int maxPoops = 2;

    private int currentPoops;
    private float nextPoopTime;

    private Animator animator;
    private NavMeshAgent agent;

    private bool comingToPlayer;
    private bool carryingBall;
    private bool isBarking;
    private bool isNeedFollowing;

    private float nextBarkTime;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        hunger = PlayerPrefs.GetInt("hunger", hunger);
        thirst = PlayerPrefs.GetInt("thirst", thirst);

        nextBarkTime = Time.time + Random.Range(minBarkTime, maxBarkTime);
        nextPoopTime = Time.time + Random.Range(minPoopTime, maxPoopTime);

        StartCoroutine(HungerRoutine());
        StartCoroutine(ThirstRoutine());
        StartCoroutine(WanderRoutine());
    }

    void Update()
    {
        hunger = PlayerPrefs.GetInt("hunger", hunger);
        thirst = PlayerPrefs.GetInt("thirst", thirst);

        if (OVRInput.GetDown(OVRInput.Button.Three))
        {
            CallDog();
        }

        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            FetchBall();
        }

        if (NeedsAttention() &&
            !isNeedFollowing &&
            !comingToPlayer &&
            !isBarking)
        {
            StartCoroutine(NeedFollowBarkRoutine());
        }

        if (!comingToPlayer &&
            !isBarking &&
            !isNeedFollowing &&
            Time.time >= nextBarkTime)
        {
            StartCoroutine(BarkRoutine());
        }

        if (!comingToPlayer &&
            !isBarking &&
            !isNeedFollowing &&
            currentPoops < maxPoops &&
            Time.time >= nextPoopTime)
        {
            CreatePoop();

            nextPoopTime =
                Time.time + Random.Range(minPoopTime, maxPoopTime);
        }

        if (!carryingBall && ball != null)
        {
            Rigidbody rb = ball.GetComponent<Rigidbody>();

            if (rb != null && rb.linearVelocity.magnitude < 0.05f)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    IEnumerator HungerRoutine()
    {
        while (true)
        {
            float waitTime =
                hunger > 50 ? hungerTickAbove50 : hungerTickBelow50;

            yield return new WaitForSeconds(waitTime);

            hunger = Mathf.Clamp(hunger - 1, 1, 100);
            PlayerPrefs.SetInt("hunger", hunger);
            PlayerPrefs.Save();
        }
    }

    IEnumerator ThirstRoutine()
    {
        while (true)
        {
            float waitTime =
                thirst > 50 ? thirstTickAbove50 : thirstTickBelow50;

            yield return new WaitForSeconds(waitTime);

            thirst = Mathf.Clamp(thirst - 1, 1, 100);
            PlayerPrefs.SetInt("thirst", thirst);
            PlayerPrefs.Save();
        }
    }

    bool NeedsAttention()
    {
        return hunger < hungryThreshold || thirst < thirstThreshold;
    }

    IEnumerator NeedFollowBarkRoutine()
    {
        if (playerTarget == null)
            yield break;

        isNeedFollowing = true;

        agent.isStopped = false;
        agent.speed = runSpeed;
        agent.stoppingDistance = needStopDistance;

        float barkTimer = 0f;
        float nextPathUpdateTime = 0f;

        while (NeedsAttention())
        {
            hunger = PlayerPrefs.GetInt("hunger", hunger);
            thirst = PlayerPrefs.GetInt("thirst", thirst);

            if (Time.time >= nextPathUpdateTime)
            {
                nextPathUpdateTime = Time.time + needPathUpdateInterval;
                agent.SetDestination(playerTarget.position);
            }

            if (agent.remainingDistance > agent.stoppingDistance + 0.2f)
            {
                animator.SetBool("walking", false);
                animator.SetBool("run", true);
            }
            else
            {
                animator.SetBool("run", false);

                barkTimer += Time.deltaTime;

                if (barkTimer >= needBarkRepeatTime)
                {
                    animator.SetTrigger("bark");

                    if (audioSource != null && barkSound != null)
                    {
                        audioSource.PlayOneShot(barkSound);
                    }

                    barkTimer = 0f;
                }
            }

            yield return null;
        }

        animator.SetBool("run", false);
        animator.SetBool("walking", false);

        agent.stoppingDistance = 0.3f;
        agent.speed = walkSpeed;

        isNeedFollowing = false;
    }

    IEnumerator WanderRoutine()
    {
        while (true)
        {
            if (!comingToPlayer &&
                !isBarking &&
                !isNeedFollowing)
            {
                agent.isStopped = false;
                agent.speed = walkSpeed;

                animator.SetBool("run", false);
                animator.SetBool("walking", true);

                Vector3 destination =
                    GetRandomNavMeshPoint(transform.position, wanderRadius);

                agent.SetDestination(destination);

                while (!comingToPlayer &&
                       !isBarking &&
                       !isNeedFollowing &&
                       (agent.pathPending ||
                        agent.remainingDistance > agent.stoppingDistance))
                {
                    yield return null;
                }

                animator.SetBool("walking", false);
            }

            yield return null;
        }
    }

    IEnumerator BarkRoutine()
    {
        isBarking = true;

        agent.ResetPath();
        agent.isStopped = true;

        animator.SetBool("walking", false);
        animator.SetBool("run", false);
        animator.SetTrigger("bark");

        if (audioSource != null && barkSound != null)
        {
            audioSource.PlayOneShot(barkSound);
        }

        yield return new WaitForSeconds(barkDuration);

        isBarking = false;
        agent.isStopped = false;

        nextBarkTime =
            Time.time + Random.Range(minBarkTime, maxBarkTime);
    }

    public void CallDog()
    {
        if (!comingToPlayer && !isBarking && !isNeedFollowing)
        {
            StartCoroutine(ComeToPlayerRoutine());
        }
    }

    public void FetchBall()
    {
        if (comingToPlayer || isBarking || isNeedFollowing)
            return;

        if (ball == null || playerTarget == null)
            return;

        float distance =
            Vector3.Distance(playerTarget.position, ball.position);

        if (distance < minBallDistanceFromPlayer)
            return;

        AddPlayerPrefInt("nBalls", 1);

        StartCoroutine(FetchBallRoutine());
    }

    IEnumerator ComeToPlayerRoutine()
    {
        comingToPlayer = true;

        agent.speed = runSpeed;

        animator.SetBool("walking", false);
        animator.SetBool("run", true);

        agent.SetDestination(playerTarget.position);

        while (agent.pathPending ||
               agent.remainingDistance > playerStopDistance)
        {
            yield return null;
        }

        animator.SetBool("run", false);

        comingToPlayer = false;
    }

    IEnumerator FetchBallRoutine()
    {
        comingToPlayer = true;

        Rigidbody ballRb = ball.GetComponent<Rigidbody>();
        Collider ballCollider = ball.GetComponent<Collider>();

        agent.speed = runSpeed;

        animator.SetBool("walking", false);
        animator.SetBool("run", true);

        while (Vector3.Distance(transform.position, ball.position) > ballStopDistance)
        {
            agent.SetDestination(ball.position);
            yield return null;
        }

        if (ballRb != null)
        {
            ballRb.linearVelocity = Vector3.zero;
            ballRb.angularVelocity = Vector3.zero;
            ballRb.isKinematic = true;
        }

        if (ballCollider != null)
        {
            ballCollider.enabled = false;
        }

        carryingBall = true;

        agent.SetDestination(playerTarget.position);

        while (agent.pathPending ||
               agent.remainingDistance > bringBallStopDistance)
        {
            yield return null;
        }

        carryingBall = false;

        ball.position =
            playerTarget.position + playerTarget.forward;

        if (ballCollider != null)
        {
            ballCollider.enabled = true;
        }

        if (ballRb != null)
        {
            ballRb.isKinematic = false;
        }

        animator.SetBool("run", false);

        comingToPlayer = false;
    }

    void CreatePoop()
    {
        if (poopPrefab == null)
            return;

        Vector3 position =
            transform.position - transform.forward * 0.4f;

        GameObject poop =
            Instantiate(poopPrefab, position, Quaternion.identity);

        PoopBehaviour pb =
            poop.GetComponent<PoopBehaviour>();

        if (pb != null)
        {
            pb.dog = this;
        }

        currentPoops++;

        StartCoroutine(RemovePoopAfterTime(poop, 120f));
    }

    IEnumerator RemovePoopAfterTime(GameObject poop, float time)
    {
        yield return new WaitForSeconds(time);

        if (poop != null)
        {
            Destroy(poop);

            currentPoops--;

            currentPoops = Mathf.Max(currentPoops, 0);
        }
    }

    public void RemovePoopFromCount()
    {
        currentPoops =
            Mathf.Max(currentPoops - 1, 0);

        AddPlayerPrefInt("nPoops", 1);
    }

    private void AddPlayerPrefInt(string key, int amount)
    {
        int current = PlayerPrefs.GetInt(key, 0);

        PlayerPrefs.SetInt(key, current + amount);
        PlayerPrefs.Save();
    }

    void UpdateBallCarry()
    {
        if (!carryingBall)
            return;

        if (ball == null || mouthPoint == null)
            return;

        ball.position = mouthPoint.position;
        ball.rotation = mouthPoint.rotation;
    }

    Vector3 GetRandomNavMeshPoint(Vector3 center, float radius)
    {
        Vector3 random =
            center + Random.insideUnitSphere * radius;

        NavMesh.SamplePosition(
            random,
            out NavMeshHit hit,
            radius,
            NavMesh.AllAreas);

        return hit.position;
    }

    void LateUpdate()
    {
        UpdateBallCarry();
    }
}