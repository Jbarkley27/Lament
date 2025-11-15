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
    private List<EnemyBase> activeEnemies = new List<EnemyBase>();
    private int enemiesRemainingInWave = 0;
    public bool campCompleted = false;


    private void OnTriggerEnter(Collider other)
    {
        if (campCompleted) return;
        if (other.CompareTag("PlayerVisual"))
        {
            Logger.Log("Player entered enemy camp area.");
            playerInRange = true;

            if (despawnCoroutine != null)
            {
                Logger.Log("Cancelling despawn coroutine as player returned.");
                StopCoroutine(despawnCoroutine);
                despawnCoroutine = null;
            }

            // Resume or start spawning
            StartNextWave();
        }
    }




    private void OnTriggerExit(Collider other)
    {
        if (campCompleted) return;
        if (other.CompareTag("PlayerVisual"))
        {
            Logger.Log("Player left enemy camp area.");
            playerInRange = false;
            despawnCoroutine = StartCoroutine(DespawnAfterDelay());
        }
    }




    // only called when player leaves camp area
    private IEnumerator DespawnAfterDelay()
    {
        Logger.Log("Despawning enemies in " + despawnDelay + " seconds...");
        yield return new WaitForSeconds(despawnDelay);

        // check again if player returned
        if (playerInRange)
        {
            Logger.Log("Player returned before despawn. Aborting despawn.");
            StopCoroutine(despawnCoroutine);
            yield break;
        }

        Logger.Log("Despawning all enemies from camp.");

        // Despawn all active enemies
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] != null && activeEnemies[i].gameObject.activeSelf)
            {
                GlobalEnemyPool.Instance.DespawnEnemy(activeEnemies[i].gameObject);
            }
        }

        activeEnemies.Clear();
    }




    // Called by enemies when they die
    public void NotifyEnemyDied(EnemyBase enemy)
    {
        if (campCompleted) return;
        if (activeEnemies.Contains(enemy))
            activeEnemies.Remove(enemy);

        Logger.Log($"Enemy {enemy.gameObject.name} died. Remaining in wave: " + (enemiesRemainingInWave - 1));
        enemiesRemainingInWave--;

        // If wave finished and player is still here → start next wave
        if (enemiesRemainingInWave <= 0)
        {
            currentWaveIndex++;

            if (currentWaveIndex >= enemyWaves.Count)
            {
                // All waves complete → destroy camp
                Logger.Log("All waves completed. Initiating camp rewards and cleanup.");
                StartCoroutine(InitiateCampRewards());
                return;
            }

            if (playerInRange)
                StartNextWave();
        }
    }

    private void StartNextWave()
    {
        if (campCompleted) return;
        // Prevent multiple starts
        if (spawnCoroutine != null)
            return;


        if (currentWaveIndex < enemyWaves.Count)
        {
            Logger.Log("Starting wave " + (currentWaveIndex + 1));
            EnemyWaveConfig config = enemyWaves[currentWaveIndex];

            // Count total enemies this wave must kill
            enemiesRemainingInWave = CountEnemiesInWave(config);

            spawnCoroutine = StartCoroutine(SpawnWave(config));
        }
        else
        {
            Logger.Log("All waves completed.");
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
        if (campCompleted) yield break;
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
        if (campCompleted) return;
        Vector3 spawnPos = FindValidSpawnPosition(Random.Range(5f, 6f));
        spawnPos.y = 0; // ground level

        EnemyBase enemy = GlobalEnemyPool.Instance.SpawnEnemy(type, spawnPos).GetComponent<EnemyBase>();

        enemy.owningCamp = this;
        activeEnemies.Add(enemy);
    }


    private Vector3 FindValidSpawnPosition(float radius, int maxAttempts = 10)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 candidate = GetRandomPointInCollider();

            // Check for overlap
            if (!Physics.CheckSphere(candidate, radius, LayerMask.GetMask("Enemy")))
            {
                return candidate;
            }
        }

        // If all attempts failed → just return a random position anyway
        Logger.Log("WARNING: Could not find valid spawn position after max attempts.");
        return GetRandomPointInCollider();
    }



    // Random inside collider
    private Vector3 GetRandomPointInCollider()
    {
        Vector3 extents = enemySpawnArea.bounds.extents;
        Vector3 center = enemySpawnArea.bounds.center;

        float x = Random.Range(center.x - extents.x, center.x + extents.x);
        float y = 0f; // ground level
        float z = Random.Range(center.z - extents.z, center.z + extents.z);

        return new Vector3(x, y, z);
    }


    private IEnumerator InitiateCampRewards()
    {
        if (campCompleted) yield break;
        campCompleted = true;
        Logger.Log("Camp completed! Initiating rewards...");
        yield return new WaitForSeconds(2f);

        // Instantiate rewards here (e.g., loot drops, experience orbs, etc.)

        activeEnemies.Clear();

        Destroy(gameObject);
    }
}
