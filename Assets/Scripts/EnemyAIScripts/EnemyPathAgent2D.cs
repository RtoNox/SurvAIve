using System.Collections.Generic;
using UnityEngine;

public class EnemyPathAgent2D : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;

    [Header("Movement Settings")]
    [SerializeField] private bool isFlyingEnemy;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float waypointReachDistance = 0.25f;
    [SerializeField] private float pathRefreshRate = 0.5f;

    [Header("Grounded Enemy Settings")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float jumpHeightRequirement = 0.75f;
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;

    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int IsJumping = Animator.StringToHash("IsJumping");
    private static readonly int IsFalling = Animator.StringToHash("IsFalling");

    private Rigidbody2D rb;
    private List<PathNode2D> currentPath;
    private int currentPathIndex;
    private float pathRefreshTimer;
    private bool isFacingRight = true;
    private bool canMove = true;
    private bool isGrounded;

    public bool IsFlyingEnemy => isFlyingEnemy;
    public bool HasPath => currentPath != null && currentPath.Count > 0;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (target == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                target = playerObject.transform;
            }
        }
    }

    private void Update()
    {
        if (target == null) return;

        pathRefreshTimer -= Time.deltaTime;

        if (pathRefreshTimer <= 0f)
        {
            pathRefreshTimer = pathRefreshRate;
            RefreshPath();
        }

        if (!isFlyingEnemy)
        {
            CheckGrounded();
        }

        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        if (!canMove)
        {
            StopMoving();
            return;
        }

        FollowPath();
    }

    private void RefreshPath()
    {
        if (PathfindingManager2D.Instance == null) return;

        currentPath = PathfindingManager2D.Instance.FindPath(
            transform.position,
            target.position,
            isFlyingEnemy
        );

        currentPathIndex = 0;
    }

    private void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            StopMoving();
            return;
        }

        if (currentPathIndex >= currentPath.Count)
        {
            StopMoving();
            return;
        }

        Vector2 nextPosition = currentPath[currentPathIndex].transform.position;
        float distanceToNode = Vector2.Distance(transform.position, nextPosition);

        if (distanceToNode <= waypointReachDistance)
        {
            currentPathIndex++;
            return;
        }

        if (isFlyingEnemy)
        {
            MoveFlying(nextPosition);
        }
        else
        {
            MoveGrounded(nextPosition);
        }
    }

    private void MoveFlying(Vector2 nextPosition)
    {
        Vector2 direction = nextPosition - (Vector2)transform.position;
        direction.Normalize();

        rb.velocity = direction * moveSpeed;

        FaceDirection(direction.x);
    }

    private void MoveGrounded(Vector2 nextPosition)
    {
        float directionX = Mathf.Sign(nextPosition.x - transform.position.x);

        rb.velocity = new Vector2(directionX * moveSpeed, rb.velocity.y);

        FaceDirection(directionX);

        float heightDifference = nextPosition.y - transform.position.y;

        if (heightDifference > jumpHeightRequirement && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    private void CheckGrounded()
    {
        if (groundCheckPoint == null) return;

        isGrounded = Physics2D.OverlapCircle(
            groundCheckPoint.position,
            groundCheckRadius,
            groundLayer
        );
    }

    public void SetCanMove(bool value)
    {
        canMove = value;

        if (!canMove)
        {
            StopMoving();
        }
    }

    public void StopMoving()
    {
        if (rb == null) return;

        if (isFlyingEnemy)
        {
            rb.velocity = Vector2.zero;
        }
        else
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
    }

    private void FaceDirection(float directionX)
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

    private void Flip()
    {
        isFacingRight = !isFacingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    private void UpdateAnimations()
    {
        if (animator == null || rb == null) return;

        bool isMoving = Mathf.Abs(rb.velocity.x) > 0.1f || Mathf.Abs(rb.velocity.y) > 0.1f;

        animator.SetBool(IsMoving, isMoving);

        if (!isFlyingEnemy)
        {
            animator.SetBool(IsJumping, !isGrounded && rb.velocity.y > 0.1f);
            animator.SetBool(IsFalling, !isGrounded && rb.velocity.y < -0.1f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (currentPath == null) return;

        Gizmos.color = Color.magenta;

        for (int i = 0; i < currentPath.Count - 1; i++)
        {
            if (currentPath[i] != null && currentPath[i + 1] != null)
            {
                Gizmos.DrawLine(
                    currentPath[i].transform.position,
                    currentPath[i + 1].transform.position
                );
            }
        }

        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}