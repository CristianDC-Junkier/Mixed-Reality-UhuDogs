using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class DogController : MonoBehaviour
{
    [Header("References")]
    public DogStats dogStats;
    public DogStateMachineNew stateMachine;
    public NavMeshAgent agent;
    public Animator animator;
    public Transform playerTarget;

    [Header("Ball")]
    public Transform ball;
    public Transform mouthPoint;
    public float ballStopDistance = 2f;
    public float bringBallStopDistance = 2f;

    [Header("Movement")]
    public float walkSpeed = 1.5f;
    public float runSpeed = 4f;
    public float wanderRadius = 6f;
    public float playerStopDistance = 1.5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip barkSound;
    public float barkDuration = 1.5f;

    [Header("Poop")]
    public GameObject poopPrefab;
    public float poopAnimationDuration = 3f;

    // =====================================================
    // INTERNAL
    // =====================================================
    private bool isBusy;
    private bool carryingBall;
    private DogStateMachineNew.DogState lastState;

    // =====================================================
    // START
    // =====================================================
    void Start()
    {
        if (dogStats == null)
        {
            dogStats = FindFirstObjectByType<DogStats>();
        }

        if (stateMachine == null)
        {
            stateMachine = FindFirstObjectByType<DogStateMachineNew>();
        }

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        lastState = stateMachine.currentState;
    }

    // =====================================================
    // UPDATE
    // =====================================================
    void Update()
    {
        if (isBusy) return;

        if (lastState != stateMachine.currentState)
        {
            StopAllCoroutines();
            HandleState(stateMachine.currentState);
            lastState = stateMachine.currentState;
        }

        UpdateBallCarry();
    }

    // =====================================================
    // HANDLE STATE
    // =====================================================
    void HandleState(
        DogStateMachineNew.DogState state)
    {
        switch (state)
        {
            case DogStateMachineNew.DogState.Wander:
                StartCoroutine(WanderRoutine());
                break;

            case DogStateMachineNew.DogState.FollowPlayer:
                StartCoroutine(FollowPlayerRoutine());
                break;

            case DogStateMachineNew.DogState.FetchBall:
                StartCoroutine(FetchBallRoutine());
                break;

            case DogStateMachineNew.DogState.NeedFood:
                StartCoroutine(NeedSomethingRoutine());
                break;

            case DogStateMachineNew.DogState.NeedWater:
                StartCoroutine(NeedSomethingRoutine());
                break;

            case DogStateMachineNew.DogState.Poop:
                StartCoroutine(PoopRoutine());
                break;

            case DogStateMachineNew.DogState.Sit:
                StartCoroutine(SitRoutine());
                break;

            case DogStateMachineNew.DogState.Idle:
                Idle();
                break;
        }
    }

    // =====================================================
    // IDLE
    // =====================================================
    void Idle()
    {
        agent.ResetPath();
        agent.isStopped = true;

        animator.SetBool("walking", false);
        animator.SetBool("run", false);
    }

    // =====================================================
    // WANDER
    // =====================================================
    IEnumerator WanderRoutine()
    {
        agent.isStopped = false;
        agent.speed = walkSpeed;

        animator.SetBool("walking", true);
        animator.SetBool("run", false);

        Vector3 destination = GetRandomNavMeshPoint(transform.position, wanderRadius);

        agent.SetDestination(destination);

        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }

        animator.SetBool("walking", false);
    }

    // =====================================================
    // FOLLOW PLAYER
    // =====================================================
    IEnumerator FollowPlayerRoutine()
    {
        isBusy = true;
        agent.isStopped = false;
        agent.speed = runSpeed;

        animator.SetBool("walking", false);
        animator.SetBool("run", true);

        agent.SetDestination(playerTarget.position);

        while (agent.pathPending || agent.remainingDistance > playerStopDistance)
        {
            yield return null;
        }

        animator.SetBool("run", false);
        stateMachine.FinishFollowPlayer();
        isBusy = false;
    }

    // =====================================================
    // FETCH BALL
    // =====================================================
    IEnumerator FetchBallRoutine()
    {
        if (ball == null)
        {
            yield break;
        }

        isBusy = true;
        Rigidbody ballRb = ball.GetComponent<Rigidbody>();
        Collider ballCollider = ball.GetComponent<Collider>();

        agent.isStopped = false;
        agent.speed = runSpeed;

        animator.SetBool("walking", false);

        animator.SetBool("run", true);

        while (Vector3.Distance(transform.position, ball.position) > ballStopDistance)
        {
            agent.SetDestination(ball.position);
            yield return null;
        }

        // PICK BALL
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

        // RETURN PLAYER
        agent.SetDestination(playerTarget.position);

        while (agent.pathPending || agent.remainingDistance > bringBallStopDistance)
        {
            yield return null;
        }

        // DROP BALL
        carryingBall = false;
        ball.position = playerTarget.position + playerTarget.forward;

        if (ballCollider != null)
        {
            ballCollider.enabled = true;
        }

        if (ballRb != null)
        {
            ballRb.isKinematic = false;
        }

        animator.SetBool("run", false);
        dogStats.RegisterBallThrow();
        stateMachine.FinishFetchBall();

        isBusy = false;
    }

    // =====================================================
    // NEED FOOD / WATER
    // =====================================================
    IEnumerator NeedSomethingRoutine()
    {
        isBusy = true;
        agent.isStopped = false;
        agent.speed = walkSpeed;

        animator.SetBool("walking", true);
        animator.SetBool("run", false);
        agent.SetDestination(playerTarget.position);

        while (agent.pathPending || agent.remainingDistance > playerStopDistance)
        {
            yield return null;
        }

        animator.SetBool("walking", false);
        yield return BarkRoutine();

        isBusy = false;
    }

    // =====================================================
    // BARK
    // =====================================================
    IEnumerator BarkRoutine()
    {
        animator.SetTrigger("bark");

        if (audioSource != null &&
            barkSound != null)
        {
            audioSource.PlayOneShot(barkSound);
        }

        yield return new WaitForSeconds(barkDuration);
    }

    // =====================================================
    // POOP
    // =====================================================
    IEnumerator PoopRoutine()
    {
        isBusy = true;
        agent.ResetPath();
        agent.isStopped = true;

        animator.SetBool("walking", false);

        animator.SetBool("run", false);

        // TODO:
        // Añadir trigger poop
        // animator.SetTrigger("poop");
        yield return new WaitForSeconds(poopAnimationDuration);

        if (poopPrefab != null)
        {
            Vector3 position = transform.position - transform.forward * 0.4f;
            Instantiate(poopPrefab, position, Quaternion.identity);
        }

        dogStats.RegisterPoop();
        isBusy = false;
    }

    // =====================================================
    // SIT
    // =====================================================
    IEnumerator SitRoutine()
    {
        isBusy = true;
        agent.ResetPath();
        agent.isStopped = true;

        animator.SetBool("walking", false);
        animator.SetBool("run", false);
        animator.SetTrigger("sit");

        while (dogStats.energy < dogStats.maxStat * 0.7f)
        {
            yield return null;
        }

        animator.SetTrigger("stand");
        isBusy = false;
    }

    // =====================================================
    // BALL CARRY
    // =====================================================
    void UpdateBallCarry()
    {
        if (!carryingBall) return;
        if (ball == null || mouthPoint == null) return;

        ball.position = mouthPoint.position;
        ball.rotation = mouthPoint.rotation;
    }

    // =====================================================
    // RANDOM NAVMESH
    // =====================================================
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
