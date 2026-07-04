using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;
    
    [Header("Camera Settings")]
    [SerializeField] private float followSpeed = 8f;
    [SerializeField] private float maxOffsetDistance = 3f;
    [SerializeField] private Vector2 offset = new Vector2(0, 1f);
    
    [Header("Cursor Influence")]
    [SerializeField] private float cursorInfluence = 0.6f;
    
    [Header("Pixel Perfect")]
    [SerializeField] private float pixelsPerUnit = 16f;
    
    private Camera mainCamera;
    private Vector3 targetPosition;
    private Vector3 cursorWorldPosition;
    private Vector3 velocity = Vector3.zero;
    
    void Start()
    {
        mainCamera = GetComponent<Camera>();
        
        if (player == null)
        {
            Debug.LogWarning("Player not assigned to CameraFollow script!");
        }
    }
    
    void FixedUpdate() // Changed from LateUpdate to FixedUpdate
    {
        if (player == null) return;
        
        // Get cursor position in world space
        Vector3 cursorScreenPos = Input.mousePosition;
        cursorScreenPos.z = Mathf.Abs(mainCamera.transform.position.z - player.position.z);
        cursorWorldPosition = mainCamera.ScreenToWorldPoint(cursorScreenPos);
        
        // Calculate direction from player to cursor
        Vector3 directionToCursor = (cursorWorldPosition - player.position).normalized;
        
        // Calculate distance from player to cursor, clamped to maxOffsetDistance
        float distanceToCursor = Vector3.Distance(player.position, cursorWorldPosition);
        float clampedDistance = Mathf.Min(distanceToCursor, maxOffsetDistance);
        
        // Calculate desired offset based on cursor position
        Vector3 cursorOffset = directionToCursor * clampedDistance * cursorInfluence;
        
        // Combine with base offset
        Vector3 desiredOffset = new Vector3(offset.x, offset.y, 0) + cursorOffset;
        
        // Calculate target position
        targetPosition = player.position + desiredOffset;
        targetPosition.z = transform.position.z;
        
        // Use SmoothDamp
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            targetPosition, 
            ref velocity, 
            1f / followSpeed
        );
        
        // Round to pixel grid AFTER SmoothDamp
        RoundToPixelGrid();
    }
    
    void RoundToPixelGrid()
    {
        Vector3 pos = transform.position;
        float ppu = pixelsPerUnit;
        
        // Round to nearest pixel
        pos.x = Mathf.Round(pos.x * ppu) / ppu;
        pos.y = Mathf.Round(pos.y * ppu) / ppu;
        
        transform.position = pos;
    }
}