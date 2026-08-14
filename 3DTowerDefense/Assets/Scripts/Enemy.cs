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

        transform.position = spawnPosition;
        gameObject.SetActive(true);

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.enabled = true;

            UnityEngine.AI.NavMeshHit hit;
            float sampleRadius = 5f;
            Vector3 navPos = spawnPosition;
            if (UnityEngine.AI.NavMesh.SamplePosition(spawnPosition, out hit, sampleRadius, UnityEngine.AI.NavMesh.AllAreas))
            {
                navPos = hit.position;
            }

            transform.position = navPos;
            agent.Warp(navPos);

            agent.speed = moveSpeed;
            agent.stoppingDistance = 0.2f;
            agent.isStopped = false;

            if (target != null)
            {
                agent.SetDestination(target.position);
            }
        }
    }

    private void Update()
    {
        if (agent == null || !agent.isOnNavMesh || agent.pathPending || target == null)
            return;

        if (!agent.hasPath)
            return;

        if (agent.remainingDistance <= agent.stoppingDistance + 0.5f)
        {
            Vector3 enemyPos = transform.position;
            Vector3 targetPos = target.position;

            float horizontalDistance = Vector2.Distance(
                new Vector2(enemyPos.x, enemyPos.z),
                new Vector2(targetPos.x, targetPos.z)
            );

            if (horizontalDistance <= 2.0f)
            {
                ReachTarget();
            }
        }
    }

    private void ReachTarget()
    {
        OnTargetReached?.Invoke(this);

        if (pool != null)
        {
            pool.Release(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
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
        if (pool != null)
        {
            pool.Release(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
        }
    }
}