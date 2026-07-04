using UnityEngine;

public class EnemyFlyingAI : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float shootingDistance = 6f;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private int damage = 10;

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 40;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;

    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int IsShooting = Animator.StringToHash("IsShooting");
    private static readonly int IsDead = Animator.StringToHash("IsDead");
    private static readonly int Hurt = Animator.StringToHash("Hurt");
    private static readonly int Shoot = Animator.StringToHash("Shoot");

    private Rigidbody2D rb;
    private Transform player;

    private int currentHealth;
    private float fireTimer;
    private bool isDead;
    private bool isFacingRight = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (isDead) return;

        fireTimer -= Time.deltaTime;

        FacePlayer();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        if (player == null)
        {
            StopMoving();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > shootingDistance)
        {
            MoveTowardPlayer();
        }
        else
        {
            StopMoving();
            TryShootAtPlayer();
        }
    }

    private void MoveTowardPlayer()
    {
        Vector2 direction = player.position - transform.position;
        direction.Normalize();

        rb.velocity = direction * moveSpeed;
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

    private void FacePlayer()
    {
        if (player == null) return;

        float direction = player.position.x - transform.position.x;

        if (direction > 0f && !isFacingRight)
        {
            Flip();
        }
        else if (direction < 0f && isFacingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    private void StopMoving()
    {
        rb.velocity = Vector2.zero;
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;

        if (animator != null)
        {
            animator.SetTrigger(Hurt);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        StopMoving();

        if (animator != null)
        {
            animator.SetBool(IsDead, true);
        }

        Collider2D enemyCollider = GetComponent<Collider2D>();

        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        Destroy(gameObject, 1.5f);
    }

    private void UpdateAnimations()
    {
        if (animator == null || rb == null) return;

        bool isMoving = rb.velocity.magnitude > 0.1f;
        bool isShooting = player != null && Vector2.Distance(transform.position, player.position) <= shootingDistance;

        animator.SetBool(IsMoving, isMoving);
        animator.SetBool(IsShooting, isShooting);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootingDistance);

        if (firePoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(firePoint.position, 0.15f);
        }
    }
}