using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Seeker))]
public class GroundEnemyAI : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform player;

    [Header("Pathfinding Settings")]
    [SerializeField] private float pathRefreshRate = 0.35f;
    [SerializeField] private float waypointReachDistance = 0.35f;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float jumpForce = 11f;
    [SerializeField] private bool faceMoveDirection = true;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Jump Point Settings")]
    [SerializeField] private LayerMask jumpPointLayer;
    [SerializeField] private float jumpPointCheckRadius = 0.45f;
    [SerializeField] private float minimumHeightDifferenceToJump = 0.5f;
    [SerializeField] private float jumpCooldown = 0.35f;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootingRange = 8f;
    [SerializeField] private float fireRate = 1.2f;
    [SerializeField] private int damage = 8;

    [SerializeField, Range(0f, 1f)] private float accuracy = 0.75f;
    [SerializeField] private float maxInaccuracyAngle = 25f;

    [Header("Line Of Sight Settings")]
    [SerializeField] private LayerMask lineOfSightMask;
    [SerializeField] private bool drawLineOfSightGizmo = true;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;

    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int IsJumping = Animator.StringToHash("IsJumping");
    private static readonly int IsShooting = Animator.StringToHash("IsShooting");
    private static readonly int Shoot = Animator.StringToHash("Shoot");
    private static readonly int Die = Animator.StringToHash("Die");

    private Rigidbody2D rb;
    private Seeker seeker;
    private Health health;

    private Path currentPath;
    private int currentWaypointIndex;

    private float fireTimer;
    private float nextPathTime;
    private float jumpCooldownTimer;

    private Vector2 aimDirection;

    private bool isDead;
    private bool isFacingRight = true;
    private bool isGrounded;
    private bool playerInRange;
    private bool hasClearShot;
    private bool shouldStopAndShoot;
    private bool pathRequestInProgress;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        seeker = GetComponent<Seeker>();
        health = GetComponent<Health>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (groundCheckPoint == null)
        {
            GameObject checkPoint = new GameObject("GroundCheck");
            checkPoint.transform.parent = transform;
            checkPoint.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            groundCheckPoint = checkPoint.transform;
        }

        if (health != null)
        {
            health.OnDeath.AddListener(HandleDeath);
        }
    }

    private void Start()
    {
        FindPlayerIfMissing();
        RequestNewPath();
    }

    private void Update()
    {
        if (isDead) return;

        fireTimer -= Time.deltaTime;
        jumpCooldownTimer -= Time.deltaTime;

        FindPlayerIfMissing();
        UpdateGroundCheck();

        if (player == null)
        {
            shouldStopAndShoot = false;
            UpdateAnimations();
            return;
        }

        AimAtPlayer();

        playerInRange = PlayerInShootingRange();
        hasClearShot = HasClearShot();
        shouldStopAndShoot = playerInRange && hasClearShot;

        if (Time.time >= nextPathTime && !pathRequestInProgress)
        {
            RequestNewPath();
        }

        if (shouldStopAndShoot)
        {
            TryShoot();
        }

        FaceEnemy();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        if (player == null)
        {
            StopHorizontalMovement();
            return;
        }

        if (shouldStopAndShoot)
        {
            StopHorizontalMovement();
            return;
        }

        FollowPath();
        CheckForJumpPoint();
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

    private void UpdateGroundCheck()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheckPoint.position,
            groundCheckRadius,
            groundLayer
        );
    }

    private void RequestNewPath()
    {
        if (player == null || seeker == null) return;

        nextPathTime = Time.time + pathRefreshRate;
        pathRequestInProgress = true;

        seeker.StartPath(transform.position, player.position, OnPathComplete);
    }

    private void OnPathComplete(Path path)
    {
        pathRequestInProgress = false;

        if (path.error)
        {
            currentPath = null;
            currentWaypointIndex = 0;
            return;
        }

        currentPath = path;
        currentWaypointIndex = 0;
    }

    private void FollowPath()
    {
        if (currentPath == null || currentPath.vectorPath == null || currentPath.vectorPath.Count == 0)
        {
            StopHorizontalMovement();
            return;
        }

        if (currentWaypointIndex >= currentPath.vectorPath.Count)
        {
            StopHorizontalMovement();
            return;
        }

        Vector2 enemyPosition = transform.position;
        Vector2 waypointPosition = currentPath.vectorPath[currentWaypointIndex];

        float distanceToWaypoint = Vector2.Distance(enemyPosition, waypointPosition);

        if (distanceToWaypoint <= waypointReachDistance)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= currentPath.vectorPath.Count)
            {
                StopHorizontalMovement();
                return;
            }

            waypointPosition = currentPath.vectorPath[currentWaypointIndex];
        }

        Vector2 directionToWaypoint = waypointPosition - enemyPosition;

        if (Mathf.Abs(directionToWaypoint.x) < 0.05f)
        {
            return;
        }

        float directionX = Mathf.Sign(directionToWaypoint.x);

        rb.velocity = new Vector2(directionX * moveSpeed, rb.velocity.y);
    }

    private void StopHorizontalMovement()
    {
        rb.velocity = new Vector2(0f, rb.velocity.y);
    }

    private void CheckForJumpPoint()
    {
        if (!isGrounded) return;
        if (jumpCooldownTimer > 0f) return;
        if (player == null) return;

        Collider2D jumpPointCollider = Physics2D.OverlapCircle(
            transform.position,
            jumpPointCheckRadius,
            jumpPointLayer
        );

        if (jumpPointCollider == null) return;

        GroundEnemyJumpPoint jumpPoint = jumpPointCollider.GetComponent<GroundEnemyJumpPoint>();

        if (jumpPoint == null) return;

        bool playerIsHigher = player.position.y > transform.position.y + minimumHeightDifferenceToJump;

        if (jumpPoint.OnlyJumpIfPlayerIsHigher && !playerIsHigher)
        {
            return;
        }

        Jump(jumpPoint.JumpForceMultiplier);
    }

    private void Jump(float jumpForceMultiplier)
    {
        jumpCooldownTimer = jumpCooldown;

        rb.velocity = new Vector2(
            rb.velocity.x,
            jumpForce * jumpForceMultiplier
        );
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
            Vector2 finalShootDirection = ApplyAccuracy(aimDirection);

            projectile.SetDirection(finalShootDirection);
            projectile.SetDamage(damage);
        }

        if (animator != null)
        {
            animator.SetTrigger(Shoot);
        }
    }

    private void FaceEnemy()
    {
        if (!faceMoveDirection) return;

        if (shouldStopAndShoot)
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
        if (rb.velocity.x > 0.1f && !isFacingRight)
        {
            Flip();
        }
        else if (rb.velocity.x < -0.1f && isFacingRight)
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

        bool isMoving = Mathf.Abs(rb.velocity.x) > 0.1f && isGrounded;
        bool isJumping = !isGrounded && rb.velocity.y > 0.1f;
        bool isCurrentlyShooting = shouldStopAndShoot && fireTimer > fireRate * 0.5f;

        animator.SetBool(IsMoving, isMoving);
        animator.SetBool(IsJumping, isJumping);
        animator.SetBool(IsShooting, isCurrentlyShooting);
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, shootingRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, jumpPointCheckRadius);

        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }

        if (currentPath != null && currentPath.vectorPath != null)
        {
            Gizmos.color = Color.cyan;

            for (int i = 0; i < currentPath.vectorPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath.vectorPath[i], currentPath.vectorPath[i + 1]);
            }
        }

        if (!drawLineOfSightGizmo) return;
        if (firePoint == null || player == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(firePoint.position, player.position);
    }
}