using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GroundEnemyAI : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform player;

    [Header("Pathfinding Settings")]
    [SerializeField] private GroundPathfinder groundPathfinder;
    [SerializeField] private float pathRefreshRate = 0.5f;
    [SerializeField] private float nodeReachDistance = 0.35f;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private bool faceMoveDirection = true;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootingRange = 8f;
    [SerializeField] private float fireRate = 1.2f;
    [SerializeField] private int damage = 8;

    [Header("Line Of Sight Settings")]
    [SerializeField] private LayerMask lineOfSightMask;
    [SerializeField] private bool drawLineOfSightGizmo = true;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;

    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int IsJumping = Animator.StringToHash("IsJumping");
    private static readonly int IsFalling = Animator.StringToHash("IsFalling");
    private static readonly int IsShooting = Animator.StringToHash("IsShooting");
    private static readonly int Shoot = Animator.StringToHash("Shoot");
    private static readonly int Die = Animator.StringToHash("Die");

    private Rigidbody2D rb;
    private Health health;

    private List<GroundPathNode.Connection> currentPath = new List<GroundPathNode.Connection>();

    private float fireTimer;
    private float pathRefreshTimer;

    private int currentPathIndex;

    private Vector2 aimDirection;

    private bool isDead;
    private bool isFacingRight = true;
    private bool isGrounded;
    private bool playerInRange;
    private bool hasClearShot;
    private bool shouldStopAndShoot;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
        FindPathfinderIfMissing();
        RefreshPath();
    }

    private void Update()
    {
        if (isDead) return;

        fireTimer -= Time.deltaTime;
        pathRefreshTimer -= Time.deltaTime;

        FindPlayerIfMissing();
        FindPathfinderIfMissing();

        isGrounded = Physics2D.OverlapCircle(
            groundCheckPoint.position,
            groundCheckRadius,
            groundLayer
        );

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

        if (pathRefreshTimer <= 0f)
        {
            RefreshPath();
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

    private void FindPathfinderIfMissing()
    {
        if (groundPathfinder != null) return;

        groundPathfinder = FindObjectOfType<GroundPathfinder>();
    }

    private void RefreshPath()
    {
        pathRefreshTimer = pathRefreshRate;

        if (groundPathfinder == null || player == null)
        {
            currentPath.Clear();
            currentPathIndex = 0;
            return;
        }

        currentPath = groundPathfinder.FindPath(transform.position, player.position);
        currentPathIndex = 0;
    }

    private void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            MoveDirectlyTowardPlayer();
            return;
        }

        if (currentPathIndex >= currentPath.Count)
        {
            MoveDirectlyTowardPlayer();
            return;
        }

        GroundPathNode.Connection currentConnection = currentPath[currentPathIndex];

        if (currentConnection == null || currentConnection.targetNode == null)
        {
            currentPathIndex++;
            return;
        }

        Vector2 targetPosition = currentConnection.targetNode.transform.position;
        Vector2 currentPosition = transform.position;

        float distanceToNode = Vector2.Distance(currentPosition, targetPosition);

        if (distanceToNode <= nodeReachDistance)
        {
            currentPathIndex++;
            return;
        }

        MoveTowardPosition(targetPosition);

        if (currentConnection.requiresJump && isGrounded)
        {
            Jump();
        }
    }

    private void MoveDirectlyTowardPlayer()
    {
        if (player == null)
        {
            StopHorizontalMovement();
            return;
        }

        MoveTowardPosition(player.position);
    }

    private void MoveTowardPosition(Vector2 targetPosition)
    {
        float directionX = Mathf.Sign(targetPosition.x - transform.position.x);

        rb.velocity = new Vector2(directionX * moveSpeed, rb.velocity.y);
    }

    private void StopHorizontalMovement()
    {
        rb.velocity = new Vector2(0f, rb.velocity.y);
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
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
        bool isFalling = !isGrounded && rb.velocity.y < -0.1f;
        bool isCurrentlyShooting = shouldStopAndShoot && fireTimer > fireRate * 0.5f;

        animator.SetBool(IsMoving, isMoving);
        animator.SetBool(IsGrounded, isGrounded);
        animator.SetBool(IsJumping, isJumping);
        animator.SetBool(IsFalling, isFalling);
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

        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }

        if (!drawLineOfSightGizmo) return;
        if (firePoint == null || player == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(firePoint.position, player.position);
    }
}