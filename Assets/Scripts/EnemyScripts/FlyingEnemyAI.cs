using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AIPath))]
[RequireComponent(typeof(AIDestinationSetter))]
public class FlyingEnemyAI : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform player;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private bool faceMoveDirection = true;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootingRange = 8f;
    [SerializeField] private float fireRate = 1.2f;
    [SerializeField] private int damage = 8;
    [SerializeField] private float homingTurnSpeed = 180f;

    [SerializeField, Range(0f, 1f)] private float accuracy = 0.75f;
    [SerializeField] private float maxInaccuracyAngle = 25f;

    [Header("Line Of Sight Settings")]
    [SerializeField] private LayerMask lineOfSightMask;
    [SerializeField] private bool drawLineOfSightGizmo = true;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;

    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int IsFlying = Animator.StringToHash("IsFlying");
    private static readonly int IsShooting = Animator.StringToHash("IsShooting");
    private static readonly int Shoot = Animator.StringToHash("Shoot");
    private static readonly int Die = Animator.StringToHash("Die");

    private Rigidbody2D rb;
    private AIPath aiPath;
    private AIDestinationSetter destinationSetter;
    private Health health;

    private float fireTimer;
    private Vector2 aimDirection;
    private bool isDead;
    private bool isFacingRight = true;
    private bool playerInRange;
    private bool hasClearShot;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        aiPath = GetComponent<AIPath>();
        destinationSetter = GetComponent<AIDestinationSetter>();
        health = GetComponent<Health>();

        rb.gravityScale = 0f;

        aiPath.maxSpeed = moveSpeed;
        aiPath.canMove = true;
        aiPath.canSearch = true;
        aiPath.isStopped = false;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (health != null)
        {
            health.OnDeath.AddListener(HandleDeath);
        }
    }

    private void Start()
    {
        FindPlayerIfMissing();
        SetAStarTarget();
    }

    private void Update()
    {
        if (isDead) return;

        fireTimer -= Time.deltaTime;

        FindPlayerIfMissing();
        SetAStarTarget();

        if (player == null)
        {
            SetMovementState(false);
            UpdateAnimations();
            return;
        }

        AimAtPlayer();

        playerInRange = PlayerInShootingRange();
        hasClearShot = HasClearShot();

        bool canStopAndShoot = playerInRange && hasClearShot;

        SetMovementState(!canStopAndShoot);

        FaceEnemy();

        if (canStopAndShoot)
        {
            TryShoot();
        }

        UpdateAnimations();
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

    private void SetAStarTarget()
    {
        if (destinationSetter == null) return;

        destinationSetter.target = player;
    }

    private void AimAtPlayer()
    {
        if (firePoint == null || player == null) return;

        aimDirection = player.position - firePoint.position;
        aimDirection.Normalize();
    }

    private bool PlayerInShootingRange()
    {
        if (player == null) return false;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        return distanceToPlayer <= shootingRange;
    }

    private bool HasClearShot()
    {
        if (firePoint == null || player == null) return false;

        Vector2 origin = firePoint.position;
        Vector2 targetPosition = player.position;
        Vector2 directionToPlayer = targetPosition - origin;
        float distanceToPlayer = directionToPlayer.magnitude;

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            directionToPlayer.normalized,
            distanceToPlayer,
            lineOfSightMask
        );

        if (hit.collider == null)
        {
            return false;
        }

        return hit.collider.CompareTag("Player");
    }

    private Vector2 ApplyAccuracy(Vector2 direction)
    {
        if (direction == Vector2.zero) return direction;

        float inaccuracyAmount = 1f - accuracy;
        float spreadAngle = maxInaccuracyAngle * inaccuracyAmount;

        float randomAngle = Random.Range(-spreadAngle, spreadAngle);

        Quaternion rotation = Quaternion.Euler(0f, 0f, randomAngle);
        Vector2 inaccurateDirection = rotation * direction;

        return inaccurateDirection.normalized;
    }

    private void SetMovementState(bool shouldMove)
    {
        if (aiPath == null) return;

        aiPath.isStopped = !shouldMove;

        if (!shouldMove)
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void FaceEnemy()
    {
        if (!faceMoveDirection) return;

        if (playerInRange && hasClearShot)
        {
            FaceAimDirection();
        }
        else
        {
            FaceMovementDirection();
        }
    }

    private void FaceMovementDirection()
    {
        if (aiPath == null) return;

        Vector2 velocity = aiPath.velocity;

        if (velocity.x > 0.1f && !isFacingRight)
        {
            Flip();
        }
        else if (velocity.x < -0.1f && isFacingRight)
        {
            Flip();
        }
    }

    private void FaceAimDirection()
    {
        if (aimDirection.x > 0.1f && !isFacingRight)
        {
            Flip();
        }
        else if (aimDirection.x < -0.1f && isFacingRight)
        {
            Flip();
        }
    }

    private void TryShoot()
    {
        if (fireTimer > 0f) return;
        if (bulletPrefab == null || firePoint == null) return;
        if (aimDirection == Vector2.zero) return;

        fireTimer = fireRate;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        Vector2 finalShootDirection = ApplyAccuracy(aimDirection);

        Projectile normalProjectile = bullet.GetComponent<Projectile>();

        if (normalProjectile != null)
        {
            normalProjectile.SetDirection(finalShootDirection);
            normalProjectile.SetDamage(damage);
        }

        HomingProjectile homingProjectile = bullet.GetComponent<HomingProjectile>();

        if (homingProjectile != null)
        {
            homingProjectile.SetDirection(finalShootDirection);
            homingProjectile.SetDamage(damage);
            homingProjectile.SetHomingTarget(player);
            homingProjectile.SetHomingTurnSpeed(homingTurnSpeed);
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

        bool isMoving = aiPath != null && !aiPath.isStopped && aiPath.velocity.magnitude > 0.1f;
        bool isCurrentlyShooting = playerInRange && hasClearShot && fireTimer > fireRate * 0.5f;

        animator.SetBool(IsMoving, isMoving);
        animator.SetBool(IsFlying, true);
        animator.SetBool(IsShooting, isCurrentlyShooting);
    }

    private void HandleDeath()
    {
        isDead = true;

        if (aiPath != null)
        {
            aiPath.canMove = false;
            aiPath.canSearch = false;
            aiPath.isStopped = true;
        }

        if (destinationSetter != null)
        {
            destinationSetter.target = null;
        }

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        if (animator != null)
        {
            animator.SetTrigger(Die);
        }

        enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, shootingRange);

        if (!drawLineOfSightGizmo) return;
        if (firePoint == null || player == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(firePoint.position, player.position);
    }
}