using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float doubleJumpForce = 10f;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Super Jump Settings")]
    [SerializeField] private float superJumpInitialForce = 8f;
    [SerializeField] private float superJumpHoldForce = 20f;
    [SerializeField] private float maxSuperJumpHoldTime = 0.5f;

    private bool hasSuperJump;
    private bool isSuperJumping;
    private float superJumpHoldTimer;

    [Header("References")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask groundLayer;
    
    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private bool canDoubleJump;
    private bool isFacingRight = true;
    
    private Animator animator;
    private static readonly int IsRunning = Animator.StringToHash("IsRunning");
    private static readonly int IsJumping = Animator.StringToHash("IsJumping");
    private static readonly int IsFalling = Animator.StringToHash("IsFalling");
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        if (groundCheckPoint == null)
        {
            GameObject checkPoint = new GameObject("GroundCheck");
            checkPoint.transform.parent = transform;
            checkPoint.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheckPoint = checkPoint.transform;
        }
    }
    
    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
        
        if (isGrounded)
        {
            canDoubleJump = true;
        }
        
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
        {
            Jump();
        }

        if ((Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W)) && isSuperJumping)
        {
            ContinueSuperJump();
        }

        if ((Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.W)) && isSuperJumping)
        {
            StopSuperJump();
        }

        if (moveInput > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && isFacingRight)
        {
            Flip();
        }
        
        UpdateAnimations();
    }
    
    void FixedUpdate()
    {
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    void Jump()
    {
        if (isGrounded)
        {
            if (hasSuperJump)
            {
                StartSuperJump();
                hasSuperJump = false;
                return;
            }

            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            canDoubleJump = true;
        }
        else if (canDoubleJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
            canDoubleJump = false;
        }
    }

    void StartSuperJump()
    {
        isSuperJumping = true;
        superJumpHoldTimer = maxSuperJumpHoldTime;

        rb.velocity = new Vector2(rb.velocity.x, superJumpInitialForce);
        canDoubleJump = false;
    }

    void ContinueSuperJump()
    {
        if (superJumpHoldTimer > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y + superJumpHoldForce * Time.deltaTime);
            superJumpHoldTimer -= Time.deltaTime;
        }
        else
        {
            StopSuperJump();
        }
    }

    void StopSuperJump()
    {
        isSuperJumping = false;
    }

    public void GiveSuperJump()
    {
        hasSuperJump = true;
    }

    public void RemoveSuperJump()
    {
        hasSuperJump = false;
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    
    void UpdateAnimations()
    {
        if (animator != null)
        {
            animator.SetBool(IsRunning, Mathf.Abs(moveInput) > 0.1f && isGrounded);
            animator.SetBool(IsJumping, !isGrounded && rb.velocity.y > 0.1f);
            animator.SetBool(IsFalling, !isGrounded && rb.velocity.y < -0.1f);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}