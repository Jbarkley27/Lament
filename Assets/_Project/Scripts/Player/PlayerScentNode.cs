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
        BoxCollider scentZoneCollider
    )
    {
        for (int i = 0; i < 20; i++) // try 20 attempts
        {
            Vector3 center = scentZoneCollider.bounds.center;
            Vector3 extents = scentZoneCollider.bounds.extents;
            float minDist = extents.magnitude * 0.1f;
            float maxDist = extents.magnitude * 0.9f;

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