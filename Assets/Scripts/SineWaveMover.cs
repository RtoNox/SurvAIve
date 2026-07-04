using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SineWaveMoverWithFlip : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float horizontalSpeed = 2f;
    [SerializeField] private float horizontalRange = 4f;
    [SerializeField] private float verticalAmplitude = 1.5f;
    [SerializeField] private float verticalFrequency = 2f;
    
    [Header("Starting Position")]
    [SerializeField] private Vector2 startPosition = Vector2.zero;
    
    [Header("Animation")]
    [SerializeField] private AnimationClip animationClip; // Drag your animation here
    [SerializeField] private bool playOnStart = true;
    
    [Header("Flipping")]
    [SerializeField] private bool flipOnDirectionChange = true;
    [SerializeField] private bool useSpriteRenderer = true; // True for SpriteRenderer, False for Transform scale
    
    private Animator animator;
    private Animation animationComponent;
    private SpriteRenderer spriteRenderer;
    private float timeElapsed = 0f;
    private float previousXPosition;
    private bool wasMovingRight = true;
    
    void Start()
    {
        // Get components
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        animationComponent = GetComponent<Animation>();
        
        // Setup animation
        SetupAnimation();
        
        // Set starting position
        transform.position = new Vector3(startPosition.x, startPosition.y, transform.position.z);
        
        // Initialize flip tracking
        previousXPosition = transform.position.x;
        wasMovingRight = true;
        
        // Ensure starting flip is correct
        UpdateFlip(true);
    }
    
    void SetupAnimation()
    {
        // Try Animator first (Mecanim)
        if (animator != null && animationClip != null)
        {
            animator.Play(animationClip.name);
            Debug.Log($"Playing animation: {animationClip.name} via Animator");
            return;
        }
        
        // Try legacy Animation component
        if (animationComponent != null && animationClip != null)
        {
            if (animationComponent.GetClip(animationClip.name) == null)
            {
                animationComponent.AddClip(animationClip, animationClip.name);
            }
            
            if (playOnStart)
            {
                animationComponent.Play(animationClip.name);
                Debug.Log($"Playing animation: {animationClip.name} via Animation component");
            }
            return;
        }
        
        // Try playing by name if no clip is assigned
        if (animator != null)
        {
            animator.Play("Idle"); // Or whatever your default state is
        }
    }
    
    void Update()
    {
        // Increment time
        timeElapsed += Time.deltaTime;
        
        // ----- HORIZONTAL MOVEMENT (Left ↔ Right) -----
        float horizontalOffset = Mathf.PingPong(timeElapsed * horizontalSpeed, horizontalRange * 2) - horizontalRange;
        float xPos = startPosition.x + horizontalOffset;
        
        // ----- VERTICAL MOVEMENT (Sine wave up and down) -----
        float yOffset = Mathf.Sin(timeElapsed * verticalFrequency) * verticalAmplitude;
        float yPos = startPosition.y + yOffset;
        
        // Store previous position for direction detection
        float previousX = transform.position.x;
        
        // Apply position
        transform.position = new Vector3(xPos, yPos, transform.position.z);
        
        // Check direction and flip if needed
        if (flipOnDirectionChange)
        {
            bool isMovingRight = xPos > previousX;
            
            // Only flip when direction changes
            if (isMovingRight != wasMovingRight)
            {
                UpdateFlip(isMovingRight);
                wasMovingRight = isMovingRight;
            }
        }
    }
    
    void UpdateFlip(bool isMovingRight)
    {
        if (useSpriteRenderer && spriteRenderer != null)
        {
            // Flip using SpriteRenderer (preferred method)
            spriteRenderer.flipX = !isMovingRight;
            Debug.Log($"Flipping sprite: {(isMovingRight ? "Right" : "Left")}");
        }
        else
        {
            // Flip using Transform scale (works for any renderer)
            Vector3 scale = transform.localScale;
            scale.x = isMovingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
            Debug.Log($"Flipping scale: {(isMovingRight ? "Right" : "Left")}");
        }
    }
    
    // Optional: Visualize the movement path in the editor
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Vector3 center = new Vector3(startPosition.x, startPosition.y, transform.position.z);
            Gizmos.DrawWireCube(center, new Vector3(horizontalRange * 2, verticalAmplitude * 2, 0));
            
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(new Vector3(startPosition.x, startPosition.y, transform.position.z), 0.2f);
        }
    }
}