using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class DogAI : MonoBehaviour
{
    public Transform bowlTarget;

    public float minTimeToGetHungry = 20f;
    public float maxTimeToGetHungry = 45f;
    public float eatDuration = 5f;

    public float minWanderWait = 2f;
    public float maxWanderWait = 5f;
    public float wanderRadius = 4f;

    public float minSitTime = 2f;
    public float maxSitTime = 5f;

    [Range(0f, 1f)] public float sitChance = 0.35f;
    [Range(0f, 1f)] public float barkChance = 0.2f;

    private Animator animator;
    private NavMeshAgent agent;

    private float hungerTimer;
    private float nextHungerTime;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        ChooseNextHungerTime();

        StartCoroutine(DogRoutine());
    }

    IEnumerator DogRoutine()
    {
        while (true)
        {
            hungerTimer += Random.Range(1f, 3f);

            if (hungerTimer >= nextHungerTime)
            {
                yield return GoEat();
                hungerTimer = 0f;
                ChooseNextHungerTime();
            }
            else
            {
                if (Random.value < sitChance)
                {
                    yield return SitForAWhile();
                }
                else
                {
                    yield return Wander();
                }

                if (Random.value < barkChance)
                {
                    animator.SetTrigger("bark");
                    yield return new WaitForSeconds(1.2f);
                }
            }

            yield return new WaitForSeconds(Random.Range(minWanderWait, maxWanderWait));
        }
    }

    IEnumerator Wander()
    {
        Vector3 destination = GetRandomNavMeshPoint(transform.position, wanderRadius);

        agent.isStopped = false;
        agent.SetDestination(destination);

        animator.SetBool("walking", true);

        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance + 0.1f || agent.velocity.magnitude > 0.05f)
        {
            animator.SetBool("walking", true);
            yield return null;
        }

        animator.SetBool("walking", false);
    }

    IEnumerator GoEat()
    {
        if (bowlTarget == null)
            yield break;

        agent.isStopped = false;
        agent.SetDestination(bowlTarget.position);

        animator.SetBool("walking", true);

        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            animator.SetBool("walking", true);
            yield return null;
        }

        animator.SetBool("walking", false);

        animator.SetBool("eating", true);
        yield return new WaitForSeconds(eatDuration);
        animator.SetBool("eating", false);
    }

    IEnumerator SitForAWhile()
    {
        agent.isStopped = true;
        animator.SetBool("walking", false);

        animator.SetTrigger("sit");

        yield return new WaitForSeconds(Random.Range(minSitTime, maxSitTime));

        animator.SetTrigger("stand");

        yield return new WaitForSeconds(1.5f);

        agent.isStopped = false;
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

    void ChooseNextHungerTime()
    {
        nextHungerTime = Random.Range(minTimeToGetHungry, maxTimeToGetHungry);
    }
}