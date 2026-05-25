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

    private float nextBarkTime;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        nextBarkTime =
            Time.time + Random.Range(minBarkTime,maxBarkTime);

        nextPoopTime =
            Time.time + Random.Range(minPoopTime,maxPoopTime);

        StartCoroutine(WanderRoutine());
    }

    void Update()
    {
        // Botón A
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            CallDog();
        }

        // Botón B
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            FetchBall();
        }

        // Ladrido automático

        if (!comingToPlayer &&
            !isBarking &&
            Time.time >= nextBarkTime)
        {
            StartCoroutine(BarkRoutine());
        }

        // Cacas

        if (!comingToPlayer &&
            !isBarking &&
            currentPoops < maxPoops &&
            Time.time >= nextPoopTime)
        {
            CreatePoop();

            nextPoopTime =
                Time.time +
                Random.Range(
                    minPoopTime,
                    maxPoopTime);
        }

        // Dormir pelota

        if (!carryingBall && ball!=null)
        {
            Rigidbody rb =
                ball.GetComponent<Rigidbody>();

            if (rb!=null &&
                rb.linearVelocity.magnitude<0.05f)
            {
                rb.linearVelocity=Vector3.zero;
                rb.angularVelocity=Vector3.zero;
            }
        }
    }

    IEnumerator WanderRoutine()
    {
        while(true)
        {
            if(!comingToPlayer &&
               !isBarking)
            {
                agent.isStopped=false;

                agent.speed=walkSpeed;

                animator.SetBool(
                    "run",
                    false);

                animator.SetBool(
                    "walking",
                    true);

                Vector3 destination=
                    GetRandomNavMeshPoint(
                        transform.position,
                        wanderRadius);

                agent.SetDestination(
                    destination);

                while(
                    !comingToPlayer &&
                    !isBarking &&
                    (agent.pathPending ||
                    agent.remainingDistance>
                    agent.stoppingDistance))
                {
                    yield return null;
                }
            }

            yield return null;
        }
    }

    IEnumerator BarkRoutine()
    {
        isBarking=true;

        agent.ResetPath();
        agent.isStopped=true;

        animator.SetBool(
            "walking",
            false);

        animator.SetBool(
            "run",
            false);

        // ANIMACIÓN

        animator.SetTrigger(
            "bark");

        // SONIDO

        if(audioSource!=null &&
           barkSound!=null)
        {
            audioSource.PlayOneShot(
                barkSound);
        }

        yield return new WaitForSeconds(
            barkDuration);

        isBarking=false;

        agent.isStopped=false;

        nextBarkTime=
            Time.time+
            Random.Range(
                minBarkTime,
                maxBarkTime);
    }

    public void CallDog()
    {
        if(!comingToPlayer &&
           !isBarking)
        {
            StartCoroutine(
                ComeToPlayerRoutine());
        }
    }

    public void FetchBall()
    {
        if(comingToPlayer ||
           isBarking)
            return;

        if(ball==null)
            return;

        float distance=
            Vector3.Distance(
                playerTarget.position,
                ball.position);

        if(distance<
           minBallDistanceFromPlayer)
            return;

        StartCoroutine(
            FetchBallRoutine());
    }

    IEnumerator ComeToPlayerRoutine()
    {
        comingToPlayer=true;

        agent.speed=runSpeed;

        animator.SetBool(
            "walking",
            false);

        animator.SetBool(
            "run",
            true);

        agent.SetDestination(
            playerTarget.position);

        while(
            agent.pathPending ||
            agent.remainingDistance>
            playerStopDistance)
        {
            yield return null;
        }

        animator.SetBool(
            "run",
            false);

        comingToPlayer=false;
    }

    IEnumerator FetchBallRoutine()
    {
        comingToPlayer=true;

        Rigidbody ballRb=
            ball.GetComponent<Rigidbody>();

        Collider ballCollider=
            ball.GetComponent<Collider>();

        agent.speed=runSpeed;

        animator.SetBool(
            "walking",
            false);

        animator.SetBool(
            "run",
            true);

        while(
            Vector3.Distance(
                transform.position,
                ball.position)>
            ballStopDistance)
        {
            agent.SetDestination(
                ball.position);

            yield return null;
        }

        if(ballRb!=null)
        {
            ballRb.linearVelocity=
                Vector3.zero;

            ballRb.angularVelocity=
                Vector3.zero;

            ballRb.isKinematic=true;
        }

        if(ballCollider!=null)
        {
            ballCollider.enabled=false;
        }

        carryingBall=true;

        agent.SetDestination(
            playerTarget.position);

        while(
            agent.pathPending ||
            agent.remainingDistance>
            bringBallStopDistance)
        {
            yield return null;
        }

        carryingBall=false;

        ball.position=
            playerTarget.position+
            playerTarget.forward;

        if(ballCollider!=null)
        {
            ballCollider.enabled=true;
        }

        if(ballRb!=null)
        {
            ballRb.isKinematic=false;
        }

        animator.SetBool(
            "run",
            false);

        comingToPlayer=false;
    }

    void CreatePoop()
    {
        if(poopPrefab==null)
            return;

        Vector3 position=
            transform.position-
            transform.forward*0.4f;

        GameObject poop=
            Instantiate(
                poopPrefab,
                position,
                Quaternion.identity);

        PoopBehaviour pb=
            poop.GetComponent<
                PoopBehaviour>();

        if(pb!=null)
        {
            pb.dog=this;
        }

        currentPoops++;

        StartCoroutine(
            RemovePoopAfterTime(
                poop,
                120f));
    }

    IEnumerator RemovePoopAfterTime(
        GameObject poop,
        float time)
    {
        yield return new WaitForSeconds(
            time);

        if(poop!=null)
        {
            Destroy(poop);

            currentPoops--;

            currentPoops=
                Mathf.Max(
                    currentPoops,
                    0);
        }
    }

    public void RemovePoopFromCount()
    {
        currentPoops--;

        currentPoops=
            Mathf.Max(
                currentPoops,
                0);
    }

    void UpdateBallCarry()
    {
        if(!carryingBall)
            return;

        if(ball==null ||
           mouthPoint==null)
            return;

        ball.position=
            mouthPoint.position;

        ball.rotation=
            mouthPoint.rotation;
    }

    Vector3 GetRandomNavMeshPoint(
        Vector3 center,
        float radius)
    {
        Vector3 random=
            center+
            Random.insideUnitSphere*
            radius;

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