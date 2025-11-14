using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Create camp prefabs next

[System.Serializable]
public class EnemyWaveConfig
{
    public List<EnemyTypeSpawnInfo> enemiesToSpawn;
    public float spawnInterval;
    public float minEnemiesBeforeNextWave;
}

[System.Serializable]
public class EnemyTypeSpawnInfo
{
    public EnemyType enemyType;
    public int enemyCount;
}

public class EnemyCamp : MonoBehaviour
{
    public List<EnemyWaveConfig> enemyWaves;
    public float despawnDelay = 5f;
    public Collider enemySpawnArea;

    private int currentWaveIndex = 0;
    private bool playerInRange = false;
    private Coroutine despawnCoroutine;
    private Coroutine spawnCoroutine;

    // NEW: Track active enemies
    private List<EnemyBase> activeEnemies = new List<EnemyBase>();

    // NEW: How many enemies left to kill in the current wave
    private int enemiesRemainingInWave = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (despawnCoroutine != null)
            {
                StopCoroutine(despawnCoroutine);
                despawnCoroutine = null;
            }

            // Resume or start spawning
            StartNextWave();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            despawnCoroutine = StartCoroutine(DespawnAfterDelay());
        }
    }

    // only called when player leaves camp area
    private IEnumerator DespawnAfterDelay()
    {
        yield return new WaitForSeconds(despawnDelay);

        // Despawn all active enemies
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] != null && activeEnemies[i].gameObject.activeSelf)
            {
                activeEnemies[i].gameObject.SetActive(false);
            }
        }

        activeEnemies.Clear();
    }

    // Called by enemies when they die
    public void NotifyEnemyDied(EnemyBase enemy)
    {
        if (activeEnemies.Contains(enemy))
            activeEnemies.Remove(enemy);

        enemiesRemainingInWave--;

        // If wave finished and player is still here → start next wave
        if (enemiesRemainingInWave <= GetCurrentWaveConfig().minEnemiesBeforeNextWave)
        {
            currentWaveIndex++;

            if (currentWaveIndex >= enemyWaves.Count)
            {
                // All waves complete → destroy camp
                Destroy(gameObject);
                return;
            }

            if (playerInRange)
                StartNextWave();
        }
    }

    private void StartNextWave()
    {
        // Prevent multiple starts
        if (spawnCoroutine != null)
            return;

        if (currentWaveIndex < enemyWaves.Count)
        {
            EnemyWaveConfig config = enemyWaves[currentWaveIndex];

            // Count total enemies this wave must kill
            enemiesRemainingInWave = CountEnemiesInWave(config);

            spawnCoroutine = StartCoroutine(SpawnWave(config));
        }
    }

    public EnemyWaveConfig GetCurrentWaveConfig()
    {
        if (currentWaveIndex < enemyWaves.Count)
            return enemyWaves[currentWaveIndex];
        return null;
    }

    private int CountEnemiesInWave(EnemyWaveConfig wave)
    {
        int count = 0;
        foreach (var e in wave.enemiesToSpawn)
            count += e.enemyCount;
        return count;
    }

    private IEnumerator SpawnWave(EnemyWaveConfig waveConfig)
    {
        foreach (var spawnInfo in waveConfig.enemiesToSpawn)
        {
            for (int i = 0; i < spawnInfo.enemyCount; i++)
            {
                if (!playerInRange)
                {
                    // Player left mid-wave → stop spawning until they return
                    spawnCoroutine = null;
                    yield break;
                }

                SpawnEnemy(spawnInfo.enemyType);
                yield return new WaitForSeconds(waveConfig.spawnInterval);
            }
        }

        spawnCoroutine = null;
    }

    private void SpawnEnemy(EnemyType type)
    {
        Vector3 spawnPos = GetRandomPointInCollider(enemySpawnArea);
        
        EnemyBase enemy = GlobalEnemyPool.Instance.SpawnEnemy(type, spawnPos).GetComponent<EnemyBase>();

        enemy.owningCamp = this;
        activeEnemies.Add(enemy);
    }

    // Random inside collider
    private Vector3 GetRandomPointInCollider(Collider col)
    {
        Vector3 extents = col.bounds.extents;
        Vector3 center = col.bounds.center;

        float x = Random.Range(center.x - extents.x, center.x + extents.x);
        float y = Random.Range(center.y - extents.y, center.y + extents.y);
        float z = Random.Range(center.z - extents.z, center.z + extents.z);

        return new Vector3(x, y, z);
    }
}
