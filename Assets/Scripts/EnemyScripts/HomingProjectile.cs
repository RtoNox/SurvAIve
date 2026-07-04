using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HomingProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private bool destroyOnHit = true;

    [Header("Hit Settings")]
    [SerializeField] private LayerMask hitLayer;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private float hitVolume = 1f;
    [SerializeField] private float hitPitchRandomness = 0.1f;

    private Rigidbody2D rb;
    private Transform target;

    private Vector2 moveDirection = Vector2.right;

    private int damage;
    private float homingTurnSpeed;
    private bool hasHomingData;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        if (target != null && hasHomingData)
        {
            HomeTowardsTarget();
        }

        rb.velocity = moveDirection * moveSpeed;

        RotateToMoveDirection();
    }

    public void SetDirection(Vector2 direction)
    {
        if (direction == Vector2.zero) return;

        moveDirection = direction.normalized;
    }

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    public void SetHomingTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetHomingTurnSpeed(float newHomingTurnSpeed)
    {
        homingTurnSpeed = newHomingTurnSpeed;
        hasHomingData = true;
    }

    private void HomeTowardsTarget()
    {
        Vector2 directionToTarget = (Vector2)(target.position - transform.position);

        if (directionToTarget == Vector2.zero) return;

        Vector2 desiredDirection = directionToTarget.normalized;

        float maxRadiansDelta = homingTurnSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime;
        float angle = Vector2.SignedAngle(moveDirection, desiredDirection);
        float clampedAngle = Mathf.Clamp(angle, -maxRadiansDelta * Mathf.Rad2Deg, maxRadiansDelta * Mathf.Rad2Deg);

        float newAngle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg + clampedAngle;
        float newAngleRad = newAngle * Mathf.Deg2Rad;
        moveDirection = new Vector2(Mathf.Cos(newAngleRad), Mathf.Sin(newAngleRad)).normalized;
    }

    private void RotateToMoveDirection()
    {
        if (moveDirection == Vector2.zero) return;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & hitLayer) == 0) return;

        Health health = other.GetComponent<Health>();

        if (health != null)
        {
            health.TakeDamage(damage);
        }

        if (destroyOnHit)
        {
            SoundEffectPlayer.PlaySound(
                hitSound,
                transform.position,
                hitVolume,
                hitPitchRandomness
            );

            Destroy(gameObject);
        }
    }
}