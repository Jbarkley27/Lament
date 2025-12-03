using System.Collections;
using UnityEngine;

/// <summary>
/// Emits projectiles using simple patterns (single, row). Uses transform-based movement.
/// </summary>
public class ProjectileEmitter : MonoBehaviour
{
    public ProjectilePool pool;
    public Transform muzzle;

    private void Awake()
    {
        if (pool == null)
            pool = GetComponentInChildren<ProjectilePool>();
        if (pool == null && GlobalDataStore.Instance != null)
            pool = GlobalDataStore.Instance.ProjectilePool;
        if (muzzle == null)
            muzzle = transform;
    }

    public IEnumerator Emit(ProjectilePattern pattern)
    {
        switch (pattern.patternType)
        {
            case ProjectilePatternType.Single:
                FireSingle(pattern);
                break;
            case ProjectilePatternType.Row:
                FireRow(pattern);
                break;
        }

        yield return null;
    }

    private void FireSingle(ProjectilePattern p)
    {
        SpawnProjectile(muzzle.position, muzzle.forward, p);
    }

    private void FireRow(ProjectilePattern p)
    {
        int count = Mathf.Max(1, p.count);
        float halfArc = p.arcAngle * 0.5f;
        for (int i = 0; i < count; i++)
        {
            float t = count == 1 ? 0.5f : (float)i / (count - 1);
            float angle = Mathf.Lerp(-halfArc, halfArc, t);
            Vector3 dir = Quaternion.AngleAxis(angle, muzzle.up) * muzzle.forward;
            SpawnProjectile(muzzle.position, dir, p);
        }
    }

    private void SpawnProjectile(Vector3 position, Vector3 direction, ProjectilePattern p)
    {
        if (pool == null && GlobalDataStore.Instance != null)
            pool = GlobalDataStore.Instance.ProjectilePool;

        if (pool == null)
        {
            Debug.LogWarning($"{name}: ProjectilePool not set, cannot fire.");
            return;
        }

        Projectile proj = pool.Get();
        proj.Fire(position,
                  direction,
                  p.speed,
                  p.lifetime,
                  returned => pool.Return(returned),
                  p.damage);
    }
}
