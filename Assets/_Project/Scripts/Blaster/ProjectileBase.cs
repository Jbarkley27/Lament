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

    public void Initialize(Vector3 direction, float speed, float life, int damage, List<GameObject> impactEffects, float knockback = 0)
    {
        rb = GetComponent<Rigidbody>();
        moveDirection = direction;
        baseSpeed = speed;
        lifetime = life;
        timeAlive = 0f;
        this.damage = damage;
        this.knockbackForce = knockback;
        impactEffectsToApply = impactEffects;

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
        Debug.Log("Projectile hit " + collision.gameObject.name);
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
        Debug.Log("Effects " + impactEffectsToApply.Count);

        for(int i = 0; i < impactEffectsToApply.Count; i++)
        {
            Debug.Log($"Collision Spawn Pos - {spawnPos} : VFX - {impactEffectsToApply[i].name}");
            // Spawn the VFX
            GameObject vfx = Instantiate(impactEffectsToApply[i], spawnPos, rotation);
            // var vfx = Instantiate(particlePrefab, position, rotation);
            var main = vfx.GetComponent<ParticleSystem>().main;
            main.maxParticles = Random.Range(10, 30);
            vfx.GetComponent<ParticleSystem>().Play();
            // (Optional) destroy it after a few seconds
            Destroy(vfx, 2f);
        }

    }
}
