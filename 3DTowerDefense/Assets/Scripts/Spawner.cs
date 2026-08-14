using UnityEngine;
using UnityEngine.Pool;

public class Spawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform target;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float timeBetweenSpawns = 5f;

    private IObjectPool<Enemy> enemyPool;
    private float nextSpawnTime;

    private void Awake()
    {
        enemyPool = new ObjectPool<Enemy>(
            CreateEnemy,
            OnTakeEnemy,
            OnReleaseEnemy,
            OnDestroyEnemy
        );
    }

    private void Start()
    {
        SpawnEnemy();
        nextSpawnTime = Time.time + timeBetweenSpawns;
    }

    private void Update()
    {
        if (Time.time < nextSpawnTime)
            return;

        SpawnEnemy();
        nextSpawnTime = Time.time + timeBetweenSpawns;
    }

    private void SpawnEnemy()
    {
        Enemy enemy = enemyPool.Get();

        enemy.Initialize(
            spawnPoint.position,
            target,
            enemyPool
        );
    }

    private Enemy CreateEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Spawner: enemyPrefab is not assigned.");
            return null;
        }

        GameObject go = Instantiate(enemyPrefab);
        Enemy enemy = go.GetComponent<Enemy>();
        if (enemy == null)
        {
            enemy = go.GetComponentInChildren<Enemy>();
        }

        if (enemy == null)
        {
            Debug.LogError("Spawner: Instantiated prefab does not contain an Enemy component.", go);
            Destroy(go);
            return null;
        }

        enemy.OnTargetReached.AddListener(OnEnemyReachedTarget);
        enemy.gameObject.SetActive(false);
        return enemy;
    }

    private void OnEnemyReachedTarget(Enemy enemy)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseHealth(1);
        }
    }

    private void OnTakeEnemy(Enemy enemy)
    {
        // Handled in Enemy.Initialize to prevent premature activation and NavMesh snapping
    }

    private void OnReleaseEnemy(Enemy enemy)
    {
        enemy.gameObject.SetActive(false);
    }

    private void OnDestroyEnemy(Enemy enemy)
    {
        Destroy(enemy.gameObject);
    }
}