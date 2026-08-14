using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private float range = 25f;
    [SerializeField] private float fireRate = 1.5f; // Bullets per second
    
    [Header("Setup Fields")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform rotator;

    private float fireCountdown = 0f;
    private Transform targetEnemy;

    private void Start()
    {
        // Periodically target the closest enemy (5 times a second)
        InvokeRepeating(nameof(UpdateTarget), 0f, 0.2f);
    }

    private void UpdateTarget()
    {
        Enemy[] enemies = GameObject.FindObjectsOfType<Enemy>();
        float shortestDistance = Mathf.Infinity;
        Enemy nearestEnemy = null;

        foreach (Enemy enemy in enemies)
        {
            if (!enemy.gameObject.activeInHierarchy) continue;
            
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null && shortestDistance <= range)
        {
            targetEnemy = nearestEnemy.transform;
        }
        else
        {
            targetEnemy = null;
        }
    }

    private void Update()
    {
        if (targetEnemy == null) return;

        // Rotate towards target
        if (rotator != null)
        {
            Vector3 dir = targetEnemy.position - transform.position;
            dir.y = 0; // Rotate horizontally only
            if (dir != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(dir);
                rotator.rotation = Quaternion.Slerp(rotator.rotation, lookRotation, Time.deltaTime * 10f);
            }
        }

        // Shooting
        if (fireCountdown <= 0f)
        {
            Shoot();
            fireCountdown = 1f / fireRate;
        }

        fireCountdown -= Time.deltaTime;
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        // Standard: gib dem Bullet die Flugrichtung (non-homing). Falls das Bullet nur Seek-Unterstützung
        // bietet, kann dieses Verhalten zusätzlich implementiert werden; primär bevorzugen wir jedoch
        // eine Init-/Richtung-basierte Initialisierung.
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        if (bullet != null)
        {
            // Erwartet, dass Bullet eine InitDirection(Vector3) Methode zum Setzen der anfänglichen Flugrichtung hat.
            // Falls nicht vorhanden, sollte das Bullet intern auf Seek fallbacken können.
            bullet.InitDirection(firePoint.forward);
        }
        else
        {
            // Fallback: falls kein Bullet-Script vorhanden, nutze Rigidbody-Geschwindigkeit
            Rigidbody rb = bulletGO.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = firePoint.forward * 20f; // feste Geschwindigkeit
            }
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}