using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SpawnOption
{
    public GameObject prefab;
    [Range(0f, 1f)] public float weight = 1f;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Options")]
    public List<SpawnOption> enemies = new List<SpawnOption>();

    [Header("Spawn Settings")]
    public float spawnInterval = 3f;
    public int maxAlive = 30;
    public bool startOnAwake = true;

    [Tooltip("How many enemies each spawn point creates per spawn cycle.")]
    public int enemiesPerSpawnPoint = 1;

    [Tooltip("Delay between enemies spawned from the same spawn point.")]
    public float delayBetweenEnemiesAtSamePoint = 0.35f;

    [Header("Difficulty Scaling")]
    public bool increaseEnemiesOverTime = true;

    [Tooltip("Every X seconds, each spawn point will spawn one more enemy per cycle.")]
    public float increaseAmountEverySeconds = 120f;

    [Tooltip("How many extra enemies each spawn point gains whenever the timer triggers.")]
    public int enemiesAddedPerIncrease = 1;

    [Tooltip("Optional safety cap so the game does not become impossible by accident.")]
    public int maxEnemiesPerSpawnPoint = 5;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    private int currentAlive = 0;
    private bool isSpawning = false;
    private Coroutine spawnRoutine;
    private Coroutine difficultyRoutine;

    private readonly List<GameObject> aliveEnemies = new List<GameObject>();

    private void Start()
    {
        if (startOnAwake)
        {
            StartSpawning();
        }
    }

    public void StartSpawning()
    {
        if (isSpawning) return;

        isSpawning = true;
        spawnRoutine = StartCoroutine(SpawnLoop());

        if (increaseEnemiesOverTime)
        {
            difficultyRoutine = StartCoroutine(DifficultyIncreaseLoop());
        }
    }

    public void StopSpawning()
    {
        isSpawning = false;

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        if (difficultyRoutine != null)
        {
            StopCoroutine(difficultyRoutine);
            difficultyRoutine = null;
        }
    }

    private IEnumerator SpawnLoop()
    {
        while (isSpawning)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (currentAlive >= maxAlive)
            {
                continue;
            }

            yield return SpawnAtEverySpawnPoint();
        }
    }

    private IEnumerator SpawnAtEverySpawnPoint()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint == null) continue;

                yield return SpawnMultipleFromPoint(spawnPoint.position);
            }
        }
        else
        {
            yield return SpawnMultipleFromPoint(transform.position);
        }
    }

    private IEnumerator SpawnMultipleFromPoint(Vector3 spawnPosition)
    {
        int amountToSpawn = Mathf.Max(1, enemiesPerSpawnPoint);

        for (int i = 0; i < amountToSpawn; i++)
        {
            if (currentAlive >= maxAlive)
            {
                yield break;
            }

            SpawnSingleEnemy(spawnPosition);

            if (i < amountToSpawn - 1)
            {
                yield return new WaitForSeconds(delayBetweenEnemiesAtSamePoint);
            }
        }
    }

    private void SpawnSingleEnemy(Vector3 spawnPosition)
    {
        GameObject prefab = GetRandomEnemy();
        if (prefab == null) return;

        GameObject enemy = Instantiate(prefab, spawnPosition, Quaternion.identity);

        currentAlive++;
        aliveEnemies.Add(enemy);

        Health hp = enemy.GetComponent<Health>();
        if (hp != null)
        {
            hp.OnDeath.AddListener(() => HandleEnemyDeath(enemy));
        }
    }

    private void HandleEnemyDeath(GameObject enemy)
    {
        currentAlive = Mathf.Max(0, currentAlive - 1);
        aliveEnemies.Remove(enemy);
    }

    private IEnumerator DifficultyIncreaseLoop()
    {
        while (isSpawning)
        {
            yield return new WaitForSeconds(increaseAmountEverySeconds);

            enemiesPerSpawnPoint += enemiesAddedPerIncrease;
            enemiesPerSpawnPoint = Mathf.Clamp(enemiesPerSpawnPoint, 1, maxEnemiesPerSpawnPoint);
        }
    }

    private GameObject GetRandomEnemy()
    {
        if (enemies.Count == 0) return null;

        float totalWeight = 0f;

        foreach (SpawnOption enemyOption in enemies)
        {
            if (enemyOption == null || enemyOption.prefab == null) continue;
            if (enemyOption.weight <= 0f) continue;

            totalWeight += enemyOption.weight;
        }

        if (totalWeight <= 0f) return null;

        float randomValue = Random.value * totalWeight;

        foreach (SpawnOption enemyOption in enemies)
        {
            if (enemyOption == null || enemyOption.prefab == null) continue;
            if (enemyOption.weight <= 0f) continue;

            if (randomValue <= enemyOption.weight)
            {
                return enemyOption.prefab;
            }

            randomValue -= enemyOption.weight;
        }

        return null;
    }
}