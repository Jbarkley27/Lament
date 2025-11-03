using UnityEngine;

public class MeteorProjectile : MonoBehaviour
{
    private Vector3 target;
    private Vector3 fallDir;
    private float fallSpeed;
    private GameObject impactDebugPrefab;
    private bool hasHit = false;
    private bool showDebugImpactSphere;

    public void Init(Vector3 target, Vector3 fallDir, float fallSpeed, GameObject impactDebugPrefab, bool showDebugImpactSphere)
    {
        this.target = target;
        this.fallDir = fallDir.normalized;
        this.fallSpeed = fallSpeed;
        this.impactDebugPrefab = impactDebugPrefab;
        this.showDebugImpactSphere = showDebugImpactSphere;

        if (showDebugImpactSphere && impactDebugPrefab)
        {
            GameObject marker = GameObject.Instantiate(impactDebugPrefab, target, Quaternion.identity);
            marker.name = "Impact Marker (Debug)";
        }

        Invoke("Impact", 30);
    }

    private void Update()
    {
        if (hasHit) return;

        transform.position += fallDir * fallSpeed * Time.deltaTime;

        // // Check if we passed below target (simple distance check)
        // if (Vector3.Distance(transform.position, target) < 0.5f)
        // {
        //     Impact();
        // }
    }

    private void Impact()
    {
        hasHit = true;
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + fallDir * 2f);
    }
}
