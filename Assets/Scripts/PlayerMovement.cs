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
    
    [Header("References")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask groundLayer;
    
    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private bool canDoubleJump;
    private bool isFacingRight = true;
    
    // Animation properties (optional)
    private Animator animator;
    private static readonly int IsRunning = Animator.StringToHash("IsRunning");
    private static readonly int IsJumping = Animator.StringToHash("IsJumping");
    private static readonly int IsFalling = Animator.StringToHash("IsFalling");
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // Create ground check point if not assigned
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
        // Get input
        moveInput = Input.GetAxisRaw("Horizontal");
        
        // Check if grounded
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
        
        // Reset double jump when grounded
        if (isGrounded)
        {
            canDoubleJump = true;
        }
        
        // Jump input
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
        {
            Jump();
        }
        
        // Flip sprite based on movement direction
        if (moveInput > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && isFacingRight)
        {
            Flip();
        }
        
        // Update animations
        UpdateAnimations();
    }
    
    void FixedUpdate()
    {
        // Apply horizontal movement
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }
    
    void Jump()
    {
        if (isGrounded)
        {
            // First jump
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            canDoubleJump = true;
        }
        else if (canDoubleJump)
        {
            // Double jump
            rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
            canDoubleJump = false;
        }
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
    
    // Visualize ground check in editor
    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}