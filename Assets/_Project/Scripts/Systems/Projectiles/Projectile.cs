using System;
using UnityEngine;

/// <summary>
/// Simple transform-driven projectile (no Rigidbody) for bullet-hell style shots.
/// Moves along a fixed direction at a constant speed and despawns after lifetime.
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Defaults")]
    public float defaultSpeed = 20f;
    public float defaultLifetime = 5f;
    public int defaultDamage = 10;
    [Tooltip("Optional VFX prefab to spawn on impact.")]
    public GameObject hitVfxPrefab;

    private Vector3 direction;
    private float speed;
    private float remainingLifetime;
    private int damage;
    private Action<Projectile> onDespawn;
    private bool isActive;

    public void Fire(Vector3 origin,
                     Vector3 forward,
                     float? overrideSpeed = null,
                     float? overrideLifetime = null,
                     Action<Projectile> onDespawnCallback = null,
                     int? overrideDamage = null)
    {
        transform.position = origin;
        direction = forward.normalized;
        speed = overrideSpeed ?? defaultSpeed;
        remainingLifetime = overrideLifetime ?? defaultLifetime;
        damage = overrideDamage ?? defaultDamage;
        onDespawn = onDespawnCallback;
        isActive = true;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!isActive)
            return;

        float dt = Time.deltaTime;
        transform.position += direction * speed * dt;

        remainingLifetime -= dt;
        if (remainingLifetime <= 0f)
        {
            Despawn();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive)
            return;

        Vector3 hitPoint = other.ClosestPoint(transform.position);
        SpawnHitVfx(hitPoint, Quaternion.LookRotation(direction, Vector3.up));

        var playerHealth = other.GetComponentInParent<PlayerHealthModule>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }

        Despawn();
    }

    private void SpawnHitVfx(Vector3 position, Quaternion rotation)
    {
        if (hitVfxPrefab == null)
            return;

        GameObject vfx = Instantiate(hitVfxPrefab, position, rotation);
        // Optional: destroy after duration if particle does not auto-clean
        var ps = vfx.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            Destroy(vfx, ps.main.duration + ps.main.startLifetime.constantMax);
        }
        else
        {
            Destroy(vfx, 2f);
        }
    }

    public void Despawn()
    {
        if (!isActive)
            return;

        isActive = false;
        onDespawn?.Invoke(this);
        onDespawn = null;
        gameObject.SetActive(false);
    }
}
