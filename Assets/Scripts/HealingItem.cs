using UnityEngine;

public class HealingItem : MonoBehaviour
{
    [Header("Healing Settings")]
    [SerializeField] private int healAmount = 2;
    [SerializeField] private bool destroyAfterPickup = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Health playerHealth = other.GetComponent<Health>();

        if (playerHealth == null) return;
        if (playerHealth.IsDead) return;
        if (playerHealth.CurrentHealth >= playerHealth.MaxHealth) return;

        playerHealth.Heal(healAmount);

        if (destroyAfterPickup)
        {
            Destroy(gameObject);
        }
    }
}