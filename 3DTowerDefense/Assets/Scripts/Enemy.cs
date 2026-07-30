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

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Initialize(Vector3 spawnPosition, Transform target, IObjectPool<Enemy> pool)
    {
        this.pool = pool;

        currentHealth = maxHealth;

        agent.enabled = false;

        transform.position = spawnPosition;

        agent.enabled = true;

        agent.speed = moveSpeed;
        agent.stoppingDistance = 0f;
        agent.isStopped = false;

        agent.SetDestination(target.position);
    }

    private void Update()
    {
        if (agent.pathPending)
            return;

        if (agent.remainingDistance <= 0.1f)
        {
            ReachTarget();
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