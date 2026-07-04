using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private bool destroyOnDeath = false;
    [SerializeField] private float destroyDelay = 1.5f;

    [Header("Events")]
    public UnityEvent OnDamaged;
    public UnityEvent OnHealed;
    public UnityEvent OnDeath;

    private int currentHealth;
    private bool isDead;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;
        if (damageAmount <= 0) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnDamaged?.Invoke();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int healAmount)
    {
        if (isDead) return;
        if (healAmount <= 0) return;

        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealed?.Invoke();
    }

    public void FullHeal()
    {
        if (isDead) return;

        currentHealth = maxHealth;

        OnHealed?.Invoke();
    }

    public void SetMaxHealth(int newMaxHealth, bool fillHealth = true)
    {
        if (newMaxHealth <= 0) return;

        maxHealth = newMaxHealth;

        if (fillHealth)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        currentHealth = 0;

        OnDeath?.Invoke();

        if (destroyOnDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
    }
}