using System.Collections.Generic;
using UnityEngine;


// Rename file to maybe straight projectile
[RequireComponent(typeof(Rigidbody))]
public class ProjectileBase : MonoBehaviour
{
    public Rigidbody rb;
    public Vector3 moveDirection;
    public float baseSpeed;
    public float lifetime;
    public float timeAlive;
    public int damage;
    public float knockbackForce;
    public GameObject hitEffectPrefab;
    private List<GameObject> impactEffectsToApply = new List<GameObject>();

    public void Initialize(Vector3 direction, float speed, float life, int damage, List<GameObject> impactEffects, GameObject hitEffectPrefab, float knockback = 0)
    {
        rb = GetComponent<Rigidbody>();
        moveDirection = direction;
        baseSpeed = speed;
        lifetime = life;
        timeAlive = 0f;
        this.damage = damage;
        this.knockbackForce = knockback;
        impactEffectsToApply = impactEffects;
        this.hitEffectPrefab = hitEffectPrefab;

        // Initial velocity
        rb.linearVelocity = moveDirection * baseSpeed;

        rb.AddForce(moveDirection * baseSpeed, ForceMode.Impulse);
        Destroy(gameObject, lifetime);
    }


    void Update()
    {
        
    }



    private void OnCollisionEnter(Collision collision)
    {
        Logger.Log("Projectile hit " + collision.gameObject.name);
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Hit Enemy");

            // Point of Impact
            ApplyProjectileImpactEffect(collision, impactEffectsToApply);

            Rigidbody enemyRb;
            collision.gameObject.TryGetComponent(out enemyRb);

            if (enemyRb)
            {
                enemyRb.AddForce(moveDirection * knockbackForce);
            }

            ApplyHitFlashEffect(collision.gameObject);
            ApplyDamage(collision.gameObject);
            ApplyImpactVFX(collision);

            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Asteroid"))
        {
            Asteroid asteroidHit = collision.gameObject.GetComponentInChildren<Asteroid>();

            if (asteroidHit)
            {
                asteroidHit.TakeDamage(damage);
            }

            ApplyImpactVFX(collision);
            ApplyHitFlashEffect(collision.gameObject);
            Logger.Log("Hit Asteroid");
            Destroy(gameObject);
        }
        else if (
            !collision.gameObject.CompareTag("PlayerVisual")
            ||
            !collision.gameObject.CompareTag("Player")
        )
        {
            // Optionally handle other collisions (e.g., walls) here
            Destroy(gameObject);
        }
    }


    void ApplyProjectileImpactEffect(Collision collision, List<GameObject> impactEffects)
    {
        // Get the first contact point
        ContactPoint contact = collision.contacts[0];

        // Position to spawn the VFX
        Vector3 spawnPos = contact.point;

        // The surface normal points *outward* from what you hit
        Vector3 normal = contact.normal;

        // Create rotation that faces along the normal
        Quaternion rotation = Quaternion.LookRotation(normal);

        for (int i = 0; i < impactEffectsToApply.Count; i++)
        {
            // Spawn the VFX
            GameObject vfx = Instantiate(impactEffectsToApply[i], spawnPos, rotation);
            var main = vfx.GetComponent<ParticleSystem>().main;
            main.maxParticles = Random.Range(5, 10);
            vfx.GetComponent<ParticleSystem>().Play();
            Destroy(vfx, 2f);
        }
    }

    public void ApplyHitFlashEffect(GameObject gameObject)
    {
        HitFlashModule hitFlashModule;
        gameObject.TryGetComponent(out hitFlashModule);

        if (hitFlashModule)
        {
            hitFlashModule.FlashAll();
        }
    }


    public void ApplyImpactVFX(Collision collision)
    {
       // Get the first contact point
        ContactPoint contact = collision.contacts[0];

        // Position to spawn the VFX
        Vector3 spawnPos = contact.point;

        // The surface normal points *outward* from what you hit
        Vector3 normal = contact.normal;

        // Create rotation that faces along the normal
        Quaternion rotation = Quaternion.LookRotation(normal);

        // Spawn the VFX
        GameObject vfx = Instantiate(hitEffectPrefab, spawnPos, rotation);
        Destroy(vfx, 2f);
    }


    public void ApplyDamage(GameObject gameObject)
    {
        HealthModule healthModule;
        gameObject.TryGetComponent(out healthModule);

        if (healthModule)
        {
            healthModule.TakeDamage(damage);
        }
    }
    
}
