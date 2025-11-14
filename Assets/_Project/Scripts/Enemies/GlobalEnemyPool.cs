using System.Collections.Generic;
using UnityEngine;


public enum EnemyType
{
    Atom,
    Nidus,
    Ignis,
    Strix,
    Grail,
    Gravem,
    Krag,
    Voss
}


public class GlobalEnemyPool : MonoBehaviour
{
    public static GlobalEnemyPool Instance;

    [System.Serializable]
    public struct EnemyPrefab
    {
        public EnemyType type;
        public GameObject prefab;
        public int prewarmCount;
    }

    public Transform activeEnemyContainer;
    public Transform inactiveEnemyContainer;

    [Header("Enemy Prefabs")]
    public List<EnemyPrefab> enemyPrefabs;

    private Dictionary<EnemyType, Queue<GameObject>> poolDict = new();

    [Header("Global Enemy Settings")]
    public Canvas EnemyHealthCanvas;


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializePools();
    }

    private void InitializePools()
    {
        foreach (var ep in enemyPrefabs)
        {
            var queue = new Queue<GameObject>();
            for (int i = 0; i < ep.prewarmCount; i++)
            {
                GameObject enemy = Instantiate(ep.prefab);
                enemy.SetActive(false);
                queue.Enqueue(enemy);

                // parent to inactive container
                enemy.transform.SetParent(inactiveEnemyContainer);

                // give a unique name for debugging
                enemy.name = $"{ep.type}_Pooled_{i}";

            }
            poolDict[ep.type] = queue;
            Logger.Log($"Created {poolDict[ep.type].Count} {ep.type} enemies");
        }
    }



    public GameObject SpawnEnemy(EnemyType type, Vector3 position)
    {
        if (!poolDict.ContainsKey(type))
        {
            Debug.LogWarning($"Enemy type {type} not found in pool.");
            return null;
        }

        GameObject enemy = poolDict[type].Count > 0 ? poolDict[type].Dequeue() : Instantiate(GetPrefab(type));
        enemy.transform.position = position;
        enemy.SetActive(true);
        enemy.transform.SetParent(activeEnemyContainer);
        return enemy;
    }
    


    public void DespawnEnemy(GameObject enemy)
    {
        enemy.SetActive(false);
        enemy.transform.SetParent(inactiveEnemyContainer);

        // Optional: reset physics or AI here
        EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
        if (enemyBase != null)
        {
            poolDict[enemyBase.enemyType].Enqueue(enemy);
        }
        else
        {
            Debug.LogWarning("Despawned object missing EnemyBase component, destroying instead.");
            Destroy(enemy);
        }
    }

    private GameObject GetPrefab(EnemyType type)
    {
        foreach (var ep in enemyPrefabs)
        {
            if (ep.type == type)
                return ep.prefab;
        }
        return null;
    }
}
