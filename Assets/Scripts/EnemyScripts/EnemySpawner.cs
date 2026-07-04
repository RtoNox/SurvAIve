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
    public int maxAlive = 5;
    public bool startOnAwake = true;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    private int currentAlive = 0;
    private bool isSpawning = false;
    private Coroutine spawnRoutine;

    // Track spawned enemies
    private List<GameObject> aliveEnemies = new List<GameObject>();

    void Start()
    {
        if (startOnAwake)
            StartSpawning();
    }

    // ================= CONTROL =================

    public void StartSpawning()
    {
        if (isSpawning) return;

        isSpawning = true;
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        isSpawning = false;

        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);
    }

    // ================= SPAWNING =================

    IEnumerator SpawnLoop()
    {
        while (isSpawning)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (currentAlive >= maxAlive)
                continue;

            Spawn();
        }
    }

    void Spawn()
    {
        GameObject prefab = GetRandomEnemy();
        if (prefab == null) return;

        Vector3 pos = GetSpawnPosition();

        GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);

        currentAlive++;
        aliveEnemies.Add(enemy);

        Health hp = enemy.GetComponent<Health>();
        if (hp != null)
        {
            hp.OnDeath.AddListener (() =>
            {
                currentAlive--;
                aliveEnemies.Remove(enemy);
            });
        }
    }

    GameObject GetRandomEnemy()
    {
        if (enemies.Count == 0) return null;

        float totalWeight = 0f;
        foreach (var e in enemies)
            totalWeight += e.weight;

        float rand = Random.value * totalWeight;

        foreach (var e in enemies)
        {
            if (rand <= e.weight)
                return e.prefab;

            rand -= e.weight;
        }

        return enemies[0].prefab;
    }

    Vector3 GetSpawnPosition()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            return point.position;
        }

        return transform.position;
    }
}