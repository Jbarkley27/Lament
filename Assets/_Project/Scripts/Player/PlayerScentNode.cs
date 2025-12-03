using UnityEngine;

public class PlayerScentNode : MonoBehaviour
{
    // public Collider scentZoneCollider;
    public LayerMask freeSpaceLayerMask;


    Vector3 GetRandomPointAround(Vector3 center, float minDist, float maxDist)
    {
        // Random direction on XZ plane
        Vector2 dir = Random.insideUnitCircle.normalized;

        // Random distance between min and max
        float dist = Random.Range(minDist, maxDist);

        Vector3 point = center + new Vector3(dir.x, 0, dir.y) * dist;
        return point;
    }


    public Vector3 GetValidPlayerScentNode(
        float maxDistance
    )
    {
        for (int i = 0; i < 20; i++) // try 20 attempts
        {
            // Vector3 center = GlobalDataStore.Instance.PlayerVisual.transform.position;
            Vector3 extents = maxDistance * Vector3.one;
            float minDist = extents.magnitude * 0.3f;
            float maxDist = extents.magnitude * Random.Range(.95f, 1.1f);

            Vector3 candidate = GetRandomPointAround(
                    GlobalDataStore.Instance.PlayerVisual.transform.position, minDist, maxDist);

            // Must be inside scent zone
            if (!Physics.CheckSphere(candidate, 3, freeSpaceLayerMask))
            {
                return candidate;
            }
        }

        Logger.Log("WARNING: Could not find valid scent node after max attempts, assigned player.");
        // Fallback: just use player's position if no valid spot (should rarely happen)
        return GlobalDataStore.Instance.PlayerVisual.transform.position;
    }

}