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
    // El perro SOLO irá a por la pelota
    // si está MÁS LEJOS que esta distancia.
    public float minBallDistanceFromPlayer = 3f;

    // Distancia para capturar pelota.
    public float ballStopDistance = 2f;

    // Distancia para dejar pelota cerca del jugador.
    public float bringBallStopDistance = 2f;

    [Header("Movimiento")]
    public float walkSpeed = 1.4f;
    public float runSpeed = 4f;
    public float wanderRadius = 8f;
    public float playerStopDistance = 1.5f;

    [Header("Comportamiento")]
    public float lookAtPlayerTime = 5f;

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

    private float nextBarkTime;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        nextBarkTime =
            Time.time + Random.Range(minBarkTime, maxBarkTime);

        nextPoopTime =
            Time.time + Random.Range(minPoopTime, maxPoopTime);

        StartCoroutine(WanderRoutine());
    }

    void Update()
    {
        // BOTÓN A QUEST
        // El perro viene hacia ti

        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            CallDog();
        }

        // BOTÓN B QUEST
        // El perro busca la pelota

        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            FetchBall();
        }

        // Ladrido ocasional

        if (!comingToPlayer &&
            Time.time >= nextBarkTime)
        {
            animator.SetTrigger("bark");

            nextBarkTime =
                Time.time + Random.Range(minBarkTime, maxBarkTime);
        }

        // HACER CACA

        if (!comingToPlayer &&
            currentPoops < maxPoops &&
            Time.time >= nextPoopTime)
        {
            CreatePoop();

            nextPoopTime =
                Time.time + Random.Range(minPoopTime, maxPoopTime);
        }

        // Dormir pelota si casi no se mueve

        if (!carryingBall && ball != null)
        {
            Rigidbody rb = ball.GetComponent<Rigidbody>();

            if (rb != null)
            {
                if (rb.linearVelocity.magnitude < 0.05f)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }
    }

    IEnumerator WanderRoutine()
    {
        while (true)
        {
            if (!comingToPlayer)
            {
                agent.isStopped = false;

                agent.speed = walkSpeed;
                agent.stoppingDistance = 0.3f;

                animator.SetBool("run", false);
                animator.SetBool("walking", true);

                Vector3 destination =
                    GetRandomNavMeshPoint(
                        transform.position,
                        wanderRadius);

                if (Vector3.Distance(
                        transform.position,
                        destination) > 0.5f)
                {
                    agent.SetDestination(destination);

                    while (!comingToPlayer &&
                           (agent.pathPending ||
                            agent.remainingDistance >
                            agent.stoppingDistance ||
                            agent.velocity.magnitude > 0.05f))
                    {
                        yield return null;
                    }
                }
            }

            yield return null;
        }
    }

    public void CallDog()
    {
        if (!comingToPlayer &&
            playerTarget != null)
        {
            StartCoroutine(ComeToPlayerRoutine());
        }
    }

    public void FetchBall()
    {
        if (comingToPlayer)
            return;

        if (ball == null ||
            playerTarget == null ||
            mouthPoint == null)
            return;

        float distance =
            Vector3.Distance(
                playerTarget.position,
                ball.position);

        // Si la pelota está demasiado cerca del jugador,
        // el perro NO va.

        if (distance < minBallDistanceFromPlayer)
            return;

        StartCoroutine(FetchBallRoutine());
    }

    IEnumerator ComeToPlayerRoutine()
    {
        comingToPlayer = true;

        agent.isStopped = false;
        agent.speed = runSpeed;
        agent.stoppingDistance = playerStopDistance;

        animator.SetBool("walking", false);
        animator.SetBool("run", true);

        agent.SetDestination(playerTarget.position);

        while (agent.pathPending ||
               agent.remainingDistance >
               agent.stoppingDistance)
        {
            yield return null;
        }

        agent.ResetPath();
        agent.isStopped = true;

        animator.SetBool("run", false);
        animator.SetBool("walking", false);

        float timer = 0f;

        while (timer < lookAtPlayerTime)
        {
            LookAtPlayer();

            timer += Time.deltaTime;

            yield return null;
        }

        agent.isStopped = false;

        comingToPlayer = false;
    }

    IEnumerator FetchBallRoutine()
    {
        comingToPlayer = true;

        Rigidbody ballRb =
            ball.GetComponent<Rigidbody>();

        Collider ballCollider =
            ball.GetComponent<Collider>();

        // CORRER HACIA PELOTA

        agent.isStopped = false;
        agent.speed = runSpeed;
        agent.stoppingDistance = ballStopDistance;

        animator.SetBool("walking", false);
        animator.SetBool("run", true);

        while (true)
        {
            agent.SetDestination(ball.position);

            float distanceToBall =
                Vector3.Distance(
                    transform.position,
                    ball.position);

            if (distanceToBall <= ballStopDistance)
            {
                break;
            }

            yield return null;
        }

        // PARAR AGENTE

        agent.ResetPath();
        agent.isStopped = true;

        // APAGAR FÍSICA

        if (ballRb != null)
        {
            ballRb.linearVelocity = Vector3.zero;
            ballRb.angularVelocity = Vector3.zero;

            ballRb.isKinematic = true;
            ballRb.useGravity = false;

            ballRb.Sleep();
        }

        if (ballCollider != null)
        {
            ballCollider.enabled = false;
        }

        // ACTIVAR MODO LLEVAR PELOTA

        carryingBall = true;

        yield return new WaitForSeconds(0.2f);

        // VOLVER AL JUGADOR

        agent.isStopped = false;

        agent.stoppingDistance =
            bringBallStopDistance;

        agent.SetDestination(playerTarget.position);

        while (agent.pathPending ||
               agent.remainingDistance >
               agent.stoppingDistance)
        {
            yield return null;
        }

        agent.ResetPath();
        agent.isStopped = true;

        animator.SetBool("run", false);
        animator.SetBool("walking", false);

        // SOLTAR PELOTA

        carryingBall = false;

        Vector3 dropPosition =
            playerTarget.position +
            playerTarget.forward * 1.0f;

        dropPosition.y =
            playerTarget.position.y + 0.3f;

        ball.position = dropPosition;

        // REACTIVAR FÍSICA

        if (ballCollider != null)
        {
            ballCollider.enabled = true;
        }

        if (ballRb != null)
        {
            ballRb.isKinematic = false;
            ballRb.useGravity = true;

            ballRb.linearVelocity = Vector3.zero;
            ballRb.angularVelocity = Vector3.zero;
        }

        // MIRAR AL JUGADOR

        float timer = 0f;

        while (timer < lookAtPlayerTime)
        {
            LookAtPlayer();

            timer += Time.deltaTime;

            yield return null;
        }

        agent.isStopped = false;
        agent.stoppingDistance = 0.3f;

        comingToPlayer = false;
    }

    void CreatePoop()
    {
        if (poopPrefab == null)
            return;

        Vector3 poopPosition =
            transform.position -
            transform.forward * 0.4f;

        poopPosition.y = transform.position.y;

        GameObject poop =
            Instantiate(
                poopPrefab,
                poopPosition,
                Quaternion.identity);

        // ASIGNAR REFERENCIA AL PERRO

        PoopBehaviour poopBehaviour =
            poop.GetComponent<PoopBehaviour>();

        if (poopBehaviour != null)
        {
            poopBehaviour.dog = this;
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
        yield return new WaitForSeconds(time);

        if (poop != null)
        {
            Destroy(poop);

            currentPoops--;

            if (currentPoops < 0)
            {
                currentPoops = 0;
            }
        }
    }

    public void RemovePoopFromCount()
    {
        currentPoops--;

        if (currentPoops < 0)
        {
            currentPoops = 0;
        }
    }

    void UpdateBallCarry()
    {
        if (!carryingBall)
            return;

        if (ball == null || mouthPoint == null)
            return;

        // CLAVAR pelota EXACTAMENTE
        // en MouthPoint

        ball.position = mouthPoint.position;
        ball.rotation = mouthPoint.rotation;
    }

    void LookAtPlayer()
    {
        if (playerTarget == null)
            return;

        Vector3 direction =
            playerTarget.position -
            transform.position;

        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            transform.rotation =
                Quaternion.LookRotation(direction);
        }
    }

    Vector3 GetRandomNavMeshPoint(
        Vector3 center,
        float radius)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint =
                center +
                Random.insideUnitSphere * radius;

            randomPoint.y = center.y;

            if (NavMesh.SamplePosition(
                    randomPoint,
                    out NavMeshHit hit,
                    radius,
                    NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        return center;
    }

    void LateUpdate()
    {
        // ACTUALIZAR PELOTA CADA FRAME

        UpdateBallCarry();

        // Mientras mira al jugador,
        // no auto-rotamos con navegación.

        if (comingToPlayer)
            return;

        if (agent != null &&
            agent.velocity.sqrMagnitude > 0.01f)
        {
            Vector3 direction =
                agent.velocity.normalized;

            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                transform.rotation =
                    Quaternion.LookRotation(direction);
            }
        }
    }
}