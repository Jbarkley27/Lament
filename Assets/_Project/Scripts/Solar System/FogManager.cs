using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogManager : MonoBehaviour
{
    public int maxFogAreas;
    public List<GameObject> currentSystems = new List<GameObject>();
    public enum FogType {LIGHT, MEDIUM, HEAVY_STORM}
    public float spawnIntervalMin;
    public float spawnIntervalMax;
    public float systemStartTime;
    public List<FogPosition> availablePositions = new List<FogPosition>();

    [System.Serializable]
    public struct FogInfo
    {
        public GameObject prefab;
        public FogType fogType;
        public float minLifetime;
        public float maxLifetime;
        public string Title;
        public string Description;
        public Sprite Icon;
    }

    public FogInfo lightFogInfo;
    public FogInfo mediumFogInfo;
    public FogInfo heavyFogInfo;

    [System.Serializable]
    public struct FogPosition
    {
        public GameObject position;
        public bool isTaken;
    }

    void Start()
    {
        InvokeRepeating("CheckForNewSpawn", systemStartTime, Random.Range(spawnIntervalMin, spawnIntervalMax));
    }

    public void CheckForNewSpawn()
    {
        // check if there are enough areas already
        if (currentSystems.Count >= maxFogAreas)
        {
            return;
        }

        // Create new fog area
        FogPosition newPos = GetNewFOGPosition();

        FogInfo chosenPrefab = GetFOGPrefabType();

        GameObject newFOGArea = Instantiate(chosenPrefab.prefab, newPos.position.transform.position, Quaternion.identity);

        currentSystems.Add(newFOGArea);
        newPos.isTaken = true;
        NotificationManager.Show(
            chosenPrefab.Description,
            chosenPrefab.Title,
            chosenPrefab.Icon,
            8
        );

        StartCoroutine(DespawnFOGArea(newFOGArea, newPos, Random.Range(chosenPrefab.minLifetime, chosenPrefab.maxLifetime)));
    }


    public IEnumerator DespawnFOGArea(GameObject fogArea, FogPosition fogPosition, float fogLifeTime)
    {
        yield return new WaitForSeconds(fogLifeTime);

        // slowly dissisapte particles later
        currentSystems.Remove(fogArea);
        Destroy(fogArea);
        fogPosition.isTaken = false;

    }

    public FogPosition GetNewFOGPosition()
    {
        List<FogPosition> filteredPosition = availablePositions.FindAll(position => position.isTaken == false);
        return filteredPosition[Random.Range(0, filteredPosition.Count)];
    }


    public FogInfo GetFOGPrefabType()
    {
        var choices = new List<(FogInfo, float)>
        {
            (lightFogInfo, 50f),
            (mediumFogInfo, 35f),
            (heavyFogInfo, 15f),
        };

        return WeightedRandom.Choose(choices);
    }
}
