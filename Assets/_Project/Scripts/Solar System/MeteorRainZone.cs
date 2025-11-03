using UnityEngine;
using System.Collections;

public class MeteorRainZone : MonoBehaviour
{
    [Header("Area Settings")]
    public float radius = 20f;
    public float height = 30f;
    public float duration = 5f;
    public float spawnRate = 4f; // meteors per second

    [Header("Meteor Prefabs")]
    public GameObject meteorPrefab;
    public GameObject impactDebugPrefab;

    [Header("Meteor Settings")]
    public float fallSpeed = 20f;
    public float minScale = 0.5f;
    public float maxScale = 2f;

    [Header("Fall Direction")]
    [Range(0f, 90f)] public float fallAngle = 45f;  // 0 = horizontal, 90 = straight down
    [Range(0f, 360f)] public float yaw = 0f;        // compass direction in degrees (0 = +Z)

    [Header("Debug")]
    public bool showGizmos = true;
    public bool showDebugImpactSphere = true;

    private bool isRaining = false;
    private Vector3 fallDir;

    private void OnValidate()
    {
        CalculateFallDirection();
    }

    private void Awake()
    {
        CalculateFallDirection();
    }

    void Start()
    {
        StartMeteorRain();
    }

    private void CalculateFallDirection()
    {
        // Convert yaw and angle into a world-space direction
        float radYaw = Mathf.Deg2Rad * yaw;
        float radAngle = Mathf.Deg2Rad * fallAngle;

        // Start from a forward vector and rotate it
        Vector3 dirHorizontal = new Vector3(Mathf.Sin(radYaw), 0f, Mathf.Cos(radYaw));
        fallDir = Quaternion.AngleAxis(fallAngle, Vector3.Cross(Vector3.up, dirHorizontal)) * dirHorizontal;

        fallDir.Normalize();
    }

    public void StartMeteorRain()
    {
        if (!isRaining)
            StartCoroutine(RainRoutine());
    }

    private IEnumerator RainRoutine()
    {
        isRaining = true;
        float timer = 0f;

        while (timer < duration)
        {
            SpawnMeteor();
            yield return new WaitForSeconds(1f / spawnRate);
            timer += 1f / spawnRate;
        }

        isRaining = false;
    }

    private void SpawnMeteor()
    {
        Vector2 randomPoint = Random.insideUnitCircle * radius;
        Vector3 targetPos = transform.position + new Vector3(randomPoint.x, 0, randomPoint.y);
        Vector3 spawnPos = targetPos - fallDir * height;

        GameObject meteor = Instantiate(meteorPrefab, spawnPos, Quaternion.identity);
        float scale = Random.Range(minScale, maxScale);
        meteor.transform.localScale = Vector3.one * scale;

        var mp = meteor.GetComponentInChildren<MeteorProjectile>();
        mp.Init(targetPos, fallDir, Random.Range(fallSpeed, fallSpeed + 15f), impactDebugPrefab, showDebugImpactSphere);
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);

        // Draw fall direction arrow
        Gizmos.color = Color.yellow;
        Vector3 dirEnd = transform.position + fallDir * 10f;
        Gizmos.DrawLine(transform.position, dirEnd);
        Gizmos.DrawSphere(dirEnd, 0.3f);
    }
}
