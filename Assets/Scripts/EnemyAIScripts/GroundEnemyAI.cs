using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GroundRangedEnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private bool faceMoveDirection = true;

    [Header("Target Settings")]
    [SerializeField] private Transform player;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private int damage = 10;

    [Header("Aim Settings")]
    [SerializeField] private Transform aimPivot;
    [SerializeField] private bool rotateAimPivot = true;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;

    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int IsShooting = Animator.StringToHash("IsShooting");
    private static readonly int Shoot = Animator.StringToHash("Shoot");
    private static readonly int Die = Animator.StringToHash("Die");
    private static readonly int AimX = Animator.StringToHash("AimX");
    private static readonly int AimY = Animator.StringToHash("AimY");

    private Rigidbody2D rb;
    private Health health;

    private float fireTimer;
    private Vector2 aimDirection;
    private bool isDead;
    private bool isFacingRight = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (aimPivot == null)
        {
            aimPivot = transform;
        }

        if (health != null)
        {
            health.OnDeath.AddListener(HandleDeath);
        }
    }

    private void Start()
    {
        FindPlayerIfMissing();
    }

    private void Update()
    {
        if (isDead) return;

        fireTimer -= Time.deltaTime;

        FindPlayerIfMissing();

        if (player == null) return;

        AimAtPlayer();
        TryShoot();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        if (player == null) return;

        MoveTowardsPlayer();
    }

    private void FindPlayerIfMissing()
    {
        if (player != null) return;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void MoveTowardsPlayer()
    {
        float directionX = Mathf.Sign(player.position.x - transform.position.x);

        rb.velocity = new Vector2(directionX * moveSpeed, rb.velocity.y);

        if (faceMoveDirection)
        {
            if (directionX > 0f && !isFacingRight)
            {
                Flip();
            }
            else if (directionX < 0f && isFacingRight)
            {
                Flip();
            }
        }
    }

    private void AimAtPlayer()
    {
        if (firePoint == null || player == null) return;

        aimDirection = player.position - firePoint.position;
        aimDirection.Normalize();

        if (rotateAimPivot && aimPivot != null)
        {
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            aimPivot.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private void TryShoot()
    {
        if (fireTimer > 0f) return;
        if (bulletPrefab == null || firePoint == null) return;
        if (aimDirection == Vector2.zero) return;

        fireTimer = fireRate;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        Projectile projectile = bullet.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.SetDirection(aimDirection);
            projectile.SetDamage(damage);
        }

        if (animator != null)
        {
            animator.SetTrigger(Shoot);
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        animator.SetBool(IsMoving, Mathf.Abs(rb.velocity.x) > 0.1f);
        animator.SetBool(IsShooting, fireTimer > fireRate * 0.5f);

        animator.SetFloat(AimX, aimDirection.x);
        animator.SetFloat(AimY, aimDirection.y);
    }

    private void HandleDeath()
    {
        isDead = true;
        rb.velocity = Vector2.zero;

        if (animator != null)
        {
            animator.SetTrigger(Die);
        }

        enabled = false;
    }
}