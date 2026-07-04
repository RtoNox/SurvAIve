using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private int damage = 10;

    [Header("Aim Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform aimPivot;
    [SerializeField] private bool rotateAimPivot = true;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private float shootVolume = 1f;
    [SerializeField] private float shootPitchRandomness = 0.05f;

    private static readonly int IsShooting = Animator.StringToHash("IsShooting");
    private static readonly int Shoot = Animator.StringToHash("Shoot");

    private float fireTimer;
    private Vector2 aimDirection;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (aimPivot == null)
        {
            aimPivot = transform;
        }
    }

    private void Update()
    {
        fireTimer -= Time.deltaTime;

        AimAtMouse();

        if (Input.GetMouseButton(0))
        {
            TryShoot();
        }

        UpdateAnimations();
    }

    private void AimAtMouse()
    {
        if (mainCamera == null || firePoint == null) return;

        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0f;

        aimDirection = mouseWorldPosition - firePoint.position;
        aimDirection.Normalize();

        if (rotateAimPivot)
        {
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            aimPivot.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private void TryShoot()
    {
        if (fireTimer > 0f) return;
        if (bulletPrefab == null || firePoint == null) return;

        fireTimer = fireRate;

        SoundEffectPlayer.PlaySound(
            shootSound,
            firePoint.position,
            shootVolume,
            shootPitchRandomness
        );

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        Projectile bulletProjectile = bullet.GetComponent<Projectile>();

        if (bulletProjectile != null)
        {
            bulletProjectile.SetDirection(aimDirection);
            bulletProjectile.SetDamage(damage);
        }

        if (animator != null)
        {
            animator.SetTrigger(Shoot);
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        animator.SetBool(IsShooting, Input.GetMouseButton(0));
    }
}