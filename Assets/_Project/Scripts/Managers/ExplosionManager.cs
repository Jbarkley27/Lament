using UnityEngine;
using System.Collections.Generic;

public class ExplosionManager : MonoBehaviour
{
    public enum ExplosionType
    {
        Basic_Enemy,
    }

    [System.Serializable]
    public struct ExplosionData
    {
        public ExplosionType type;
        public GameObject prefab;
        public float duration;
    }


    public List<ExplosionData> explosionDataList;

    public GameObject GetExplosionPrefab(ExplosionType type)
    {
        foreach (var data in explosionDataList)
        {
            if (data.type == type)
            {
                return data.prefab;
            }
        }
        return null;
    }



    public float GetExplosionDuration(ExplosionType type)
    {
        foreach (var data in explosionDataList)
        {
            if (data.type == type)
            {
                return data.duration;
            }
        }
        return 0f;
    }




    public void SpawnExplosion(ExplosionType type, Vector3 position)
    {
        GameObject prefab = GetExplosionPrefab(type);
        if (prefab != null)
        {
            GameObject newExplosion = Instantiate(prefab, position, Quaternion.identity);
            Destroy(newExplosion, GetExplosionDuration(type)); // Destroy after 5 seconds to clean up
        }
        else
        {
            Debug.LogError("Explosion prefab not found for type: " + type);
        }
    }

}