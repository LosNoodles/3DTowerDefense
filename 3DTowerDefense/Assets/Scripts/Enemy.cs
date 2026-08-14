using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;
using UnityEngine.Events;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float moveSpeed = 3.5f;

    [Header("Events")]
    public UnityEvent<Enemy> OnTargetReached;

    private int currentHealth;

    private NavMeshAgent agent;
    private IObjectPool<Enemy> pool;
    private Transform target;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Initialize(Vector3 spawnPosition, Transform target, IObjectPool<Enemy> pool)
    {
        this.pool = pool;
        this.target = target;

        currentHealth = maxHealth;

        // Position the enemy and align it with the NavMesh. If the exact spawnPosition is not
        // on the NavMesh, sample the nearest NavMesh position so the agent can find a path.
        gameObject.SetActive(true);

        // Try to sample a point on the NavMesh near the spawn position
        UnityEngine.AI.NavMeshHit hit;
        float sampleRadius = 2f;
        Vector3 navPos = spawnPosition;
        if (UnityEngine.AI.NavMesh.SamplePosition(spawnPosition, out hit, sampleRadius, UnityEngine.AI.NavMesh.AllAreas))
        {
            navPos = hit.position;
        }

        // Move transform and warp agent to the valid navmesh position
        transform.position = navPos;
        agent.Warp(navPos);

        agent.speed = moveSpeed;
        agent.stoppingDistance = 0f;
        agent.isStopped = false;

        // Set destination to the target's position
        if (target != null)
            agent.SetDestination(target.position);
    }

    private void Update()
    {
        if (agent.pathPending || target == null)
            return;

        // Check if the agent has reached the end of the path on the NavMesh
        bool reachedOnNavMesh = false;
        if (!agent.hasPath || agent.remainingDistance <= 0.1f || agent.velocity.sqrMagnitude < 0.01f)
        {
            reachedOnNavMesh = true;
        }

        // Verify the enemy is physically close to the target transform horizontally (ignores Y offset)
        if (reachedOnNavMesh)
        {
            Vector3 enemyPos = transform.position;
            Vector3 targetPos = target.position;

            float horizontalDistance = Vector2.Distance(
                new Vector2(enemyPos.x, enemyPos.z),
                new Vector2(targetPos.x, targetPos.z)
            );

            if (horizontalDistance <= 1.5f)
            {
                ReachTarget();
            }
        }
    }

    private void ReachTarget()
    {
        OnTargetReached?.Invoke(this);

        pool.Release(this);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        pool.Release(this);
    }

    private void OnDisable()
    {
        agent.ResetPath();
    }
}