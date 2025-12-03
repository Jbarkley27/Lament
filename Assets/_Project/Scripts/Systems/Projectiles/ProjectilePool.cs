using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple pool for Projectile instances to avoid allocations during bullet-hell patterns.
/// </summary>
public class ProjectilePool : MonoBehaviour
{
    public Projectile projectilePrefab;
    public int initialSize = 32;

    private readonly Queue<Projectile> pool = new Queue<Projectile>();

    void Awake()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError($"{name}: Projectile prefab not assigned on ProjectilePool.");
            return;
        }

        for (int i = 0; i < initialSize; i++)
        {
            var proj = Instantiate(projectilePrefab, transform);
            proj.gameObject.SetActive(false);
            pool.Enqueue(proj);
        }
    }

    public Projectile Get()
    {
        if (pool.Count == 0)
        {
            var proj = Instantiate(projectilePrefab, transform);
            proj.gameObject.SetActive(false);
            return proj;
        }

        return pool.Dequeue();
    }

    public void Return(Projectile proj)
    {
        if (proj == null)
            return;

        proj.gameObject.SetActive(false);
        pool.Enqueue(proj);
    }
}
