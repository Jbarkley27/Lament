using UnityEngine;
using UnityEngine.AI;

public static class NavMeshHelper
{
    /// <summary>
    /// Returns a random point on the NavMesh within the rectangle defined by min/max bounds.
    /// </summary>
    /// <param name="min">Bottom-left corner of the rectangle.</param>
    /// <param name="max">Top-right corner of the rectangle.</param>
    /// <param name="maxAttempts">How many times to try finding a valid NavMesh point.</param>
    /// <returns>A valid NavMesh position.</returns>
    public static Vector3 GetRandomPointInNavMesh(Vector3 min, Vector3 max, int maxAttempts = 10)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            float x = Random.Range(min.x, max.x);
            float z = Random.Range(min.z, max.z);
            Vector3 candidate = new Vector3(x, 10, z);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(candidate, out hit, 5f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        // If all attempts fail, just return the center of the rectangle snapped to NavMesh
        Vector3 center = (min + max) * 0.5f;
        NavMeshHit fallbackHit;
        if (NavMesh.SamplePosition(center, out fallbackHit, 10f, NavMesh.AllAreas))
        {
            return fallbackHit.position;
        }

        // Last resort: return the center unmodified
        return center;
    }
}
