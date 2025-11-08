using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AsteroidZoneSpawner : MonoBehaviour
{
    public GameObject asteroidPrefab;
    public int asteroidCount = 100;
    public float asteroidRadius = 15f;
    public Transform asteroidContainer;

    private Collider spawnCollider;
    public LayerMask layerMask;

    void Start()
    {
        spawnCollider = GetComponent<Collider>();
        if (!asteroidContainer)
        {
            asteroidContainer = new GameObject("Asteroids").transform;
            asteroidContainer.SetParent(transform);
        }

        SpawnAsteroids();
    }

    void SpawnAsteroids()
    {
        int spawned = 0;
        int safety = 0;

        while (spawned < asteroidCount && safety < asteroidCount * 10)
        {
            safety++;

            Vector3 randomPos = GetRandomPointInCollider(spawnCollider);
            randomPos.y = 0;

            if (IsSpaceFree(randomPos, asteroidRadius))
            {
                GameObject asteroid = Instantiate(
                    asteroidPrefab,
                    randomPos,
                    Random.rotation,
                    asteroidContainer
                );

                if (asteroid.GetComponent<ProximityToggleObject>() == null)
                {
                    // This is a visual asteroid
                    return;
                }

                // Register with proximity manager
                var toggle = asteroid.GetComponent<IProximityToggle>();
                GlobalDataStore.Instance.AsteroidManager.RegisterObject(toggle);

                spawned++;
            }
        }

        Debug.Log($"Spawned {spawned}/{asteroidCount} asteroids in {name}");
    }

    Vector3 GetRandomPointInCollider(Collider col)
    {
        if (col is BoxCollider box)
        {
            Vector3 local = new Vector3(
                Random.Range(-box.size.x / 2f, box.size.x / 2f),
                0,
                Random.Range(-box.size.z / 2f, box.size.z / 2f)
            );
            return box.transform.TransformPoint(local + box.center);
        }
        else if (col is SphereCollider sphere)
        {
            Vector3 dir = Random.onUnitSphere;
            float dist = Random.Range(0f, sphere.radius);
            return sphere.transform.TransformPoint(dir * dist + sphere.center);
        }
        else
        {
            // fallback for mesh collider — sample its bounds
            return col.bounds.center + new Vector3(
                Random.Range(-col.bounds.extents.x, col.bounds.extents.x),
                0,
                Random.Range(-col.bounds.extents.z, col.bounds.extents.z)
            );
        }
    }

    bool IsSpaceFree(Vector3 position, float radius)
    {
        return !Physics.CheckSphere(position, radius, layerMask);
    }
}
