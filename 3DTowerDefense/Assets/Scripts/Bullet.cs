using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 25f;
    [SerializeField] private int damage = 35;
    [SerializeField] private float hitRadius = 0.5f;
    [SerializeField] private float lifeTime = 5f;

    private Transform target;
    private Vector3 moveDirection;
    private bool isHoming = false;
    private float lifeTimer = 0f;

    public void Seek(Transform targetTransform)
    {
        target = targetTransform;
        isHoming = true;
    }

    // Non-homing initialization: fixed direction in world space
    public void InitDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;
        isHoming = false;
    }

    private void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        float distanceThisFrame = speed * Time.deltaTime;

        if (isHoming)
        {
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 dir = target.position - transform.position;

            // Check if we hit the target this frame
            if (dir.magnitude <= distanceThisFrame + hitRadius)
            {
                HitTarget(target);
                return;
            }

            // Move towards target
            transform.Translate(dir.normalized * distanceThisFrame, Space.World);
            transform.LookAt(target);
        }
        else
        {
            // Non-homing: move straight and detect collisions via raycast
            Vector3 start = transform.position;
            Vector3 end = start + moveDirection * (distanceThisFrame + hitRadius);

            if (Physics.Raycast(start, moveDirection, out RaycastHit hitInfo, distanceThisFrame + hitRadius))
            {
                Enemy enemy = hitInfo.collider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    Destroy(gameObject);
                    return;
                }
            }

            transform.Translate(moveDirection * distanceThisFrame, Space.World);
            if (moveDirection != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }

    private void HitTarget(Transform targetTransform)
    {
        if (targetTransform != null)
        {
            Enemy enemy = targetTransform.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
        Destroy(gameObject);
    }
}
