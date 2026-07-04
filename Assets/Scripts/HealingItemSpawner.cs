using UnityEngine;
using System.Collections;

public class HealingItemSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject healingItemPrefab;
    [SerializeField] private float respawnDelay = 30f;
    [SerializeField] private bool spawnOnStart = true;

    private GameObject currentHealingItem;
    private Coroutine respawnRoutine;

    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnHealingItem();
        }
    }

    private void Update()
    {
        // The timer only starts after the previous healing item has been collected/destroyed.
        if (currentHealingItem == null && respawnRoutine == null)
        {
            respawnRoutine = StartCoroutine(RespawnAfterDelay());
        }
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);

        SpawnHealingItem();
        respawnRoutine = null;
    }

    private void SpawnHealingItem()
    {
        if (healingItemPrefab == null) return;
        if (currentHealingItem != null) return;

        currentHealingItem = Instantiate(
            healingItemPrefab,
            transform.position,
            transform.rotation
        );
    }
}