using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class DogAI : MonoBehaviour
{
    [Header("Referencias")]
    public Transform bowlTarget;

    [Header("Comida")]
    public GameObject foodObjectToHide;

    [Header("Posición exacta comida")]
    public Vector3 eatWorldPosition;
    public float eatRotationY = 0f;

    [Header("Hambre")]
    [Range(1,100)]
    public int hunger = 20;

    public int hungryThreshold = 40;
    public int fullAfterEating = 100;

    public float eatAnimationDuration = 3.5f;

    public float hungerTickAbove50 = 5f;
    public float hungerTickBelow50 = 10f;

    [Header("Movimiento")]
    public float wanderRadius = 4f;
    public float minWalkTime = 6f;
    public float maxWalkTime = 14f;
    public float pauseBetweenWalks = 0.3f;

    [Header("Ladrido")]
    public float minBarkTime = 35f;
    public float maxBarkTime = 90f;
    public float barkDuration = 1.5f;

    private Animator animator;
    private NavMeshAgent agent;

    private bool isEating;
    private bool isBarking;

    private float nextBarkTime;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        nextBarkTime =
            Time.time +
            Random.Range(
                minBarkTime,
                maxBarkTime);

        StartCoroutine(
            HungerRoutine());

        StartCoroutine(
            MainRoutine());
    }

    IEnumerator HungerRoutine()
    {
        while(true)
        {
            float waitTime =
                hunger > 50
                ? hungerTickAbove50
                : hungerTickBelow50;

            yield return new WaitForSeconds(
                waitTime);

            if(!isEating)
            {
                hunger--;

                hunger =
                    Mathf.Clamp(
                        hunger,
                        1,
                        100);
            }
        }
    }

    IEnumerator MainRoutine()
    {
        while(true)
        {
            if(!isEating &&
               hunger < hungryThreshold)
            {
                yield return GoEat();
            }
            else
            {
                yield return WanderForAWhile();
            }

            if(!isEating &&
               !isBarking &&
               Time.time >= nextBarkTime)
            {
                yield return BarkRoutine();
            }

            yield return new WaitForSeconds(
                pauseBetweenWalks);
        }
    }

    IEnumerator WanderForAWhile()
    {
        if(isEating || isBarking)
            yield break;

        Vector3 destination =
            GetRandomNavMeshPoint(
                transform.position,
                wanderRadius);

        agent.isStopped=false;

        agent.SetDestination(
            destination);

        animator.SetBool(
            "walking",
            true);

        float walkTime =
            Random.Range(
                minWalkTime,
                maxWalkTime);

        float timer=0f;

        while(!isEating &&
              !isBarking &&
              timer<walkTime &&
              (agent.pathPending ||
               agent.remainingDistance >
               agent.stoppingDistance+0.1f ||
               agent.velocity.magnitude>0.05f))
        {
            timer += Time.deltaTime;

            yield return null;
        }

        animator.SetBool(
            "walking",
            false);
    }

    IEnumerator GoEat()
    {
        if(bowlTarget==null ||
           isEating)
        {
            yield break;
        }

        isEating=true;

        agent.isStopped=false;

        agent.SetDestination(
            bowlTarget.position);

        animator.SetBool(
            "walking",
            true);

        while(agent.pathPending ||
              agent.remainingDistance >
              agent.stoppingDistance+0.1f)
        {
            yield return null;
        }

        agent.ResetPath();

        agent.isStopped=true;

        animator.SetBool(
            "walking",
            false);

        // TELETRANSPORTE

        agent.enabled=false;

        transform.position=
            eatWorldPosition;

        transform.rotation=
            Quaternion.Euler(
                0f,
                eatRotationY,
                0f);

        agent.enabled=true;

        agent.Warp(
            eatWorldPosition);

        agent.isStopped=true;

        // ACTIVAR COMER (TRIGGER)

        animator.SetTrigger(
            "eating");

        // ESPERAR ANIMACIÓN ENTERA

        yield return new WaitForSeconds(
            eatAnimationDuration);

        // ESCONDER COMIDA

        if(foodObjectToHide!=null)
        {
            foodObjectToHide.SetActive(
                false);
        }

        // SUBIR HAMBRE

        hunger=
            Mathf.Clamp(
                fullAfterEating,
                1,
                100);

        yield return new WaitForSeconds(
            0.2f);

        agent.isStopped=false;

        isEating=false;
    }

    IEnumerator BarkRoutine()
    {
        if(isEating)
            yield break;

        isBarking=true;

        agent.ResetPath();

        agent.isStopped=true;

        animator.SetBool(
            "walking",
            false);

        animator.SetTrigger(
            "bark");

        yield return new WaitForSeconds(
            barkDuration);

        nextBarkTime=
            Time.time+
            Random.Range(
                minBarkTime,
                maxBarkTime);

        agent.isStopped=false;

        isBarking=false;
    }

    Vector3 GetRandomNavMeshPoint(
        Vector3 center,
        float radius)
    {
        for(int i=0;i<30;i++)
        {
            Vector3 randomPoint=
                center+
                Random.insideUnitSphere*
                radius;

            randomPoint.y=
                center.y;

            if(NavMesh.SamplePosition(
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
}