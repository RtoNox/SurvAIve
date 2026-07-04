using UnityEngine;

public class GroundEnemyJumpPoint : MonoBehaviour
{
    [Header("Jump Point Settings")]
    [SerializeField] private float jumpForceMultiplier = 1f;
    [SerializeField] private bool onlyJumpIfPlayerIsHigher = true;

    public float JumpForceMultiplier => jumpForceMultiplier;
    public bool OnlyJumpIfPlayerIsHigher => onlyJumpIfPlayerIsHigher;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.35f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 1.25f);
    }
}