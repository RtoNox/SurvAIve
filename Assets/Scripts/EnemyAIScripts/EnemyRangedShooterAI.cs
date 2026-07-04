using UnityEngine;

public class EnemyRangedShooterAI : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private float shootingDistance = 6f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private int damage = 10;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;

    private static readonly int IsShooting = Animator.StringToHash("IsShooting");
    private static readonly int Shoot = Animator.StringToHash("Shoot");

    private Transform player;
    private EnemyPathAgent2D pathAgent;
    private Health health;
    private float fireTimer;

    private void Start()
    {
        pathAgent = GetComponent<EnemyPathAgent2D>();
        health = GetComponent<Health>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void Update()
    {
        if (health != null && health.IsDead) return;
        if (player == null) return;

        fireTimer -= Time.deltaTime;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > shootingDistance)
        {
            if (pathAgent != null)
            {
                pathAgent.SetCanMove(true);
            }

            SetShootingAnimation(false);
        }
        else
        {
            if (pathAgent != null)
            {
                pathAgent.SetCanMove(false);
            }

            TryShootAtPlayer();
            SetShootingAnimation(true);
        }
    }

    private void TryShootAtPlayer()
    {
        if (fireTimer > 0f) return;
        if (bulletPrefab == null || firePoint == null || player == null) return;

        fireTimer = fireRate;

        Vector2 shootDirection = player.position - firePoint.position;
        shootDirection.Normalize();

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        Projectile bulletProjectile = bullet.GetComponent<Projectile>();

        if (bulletProjectile != null)
        {
            bulletProjectile.SetDirection(shootDirection);
            bulletProjectile.SetDamage(damage);
        }

        if (animator != null)
        {
            animator.SetTrigger(Shoot);
        }
    }

    private void SetShootingAnimation(bool value)
    {
        if (animator == null) return;

        animator.SetBool(IsShooting, value);
    }
}