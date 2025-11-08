using DG.Tweening;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [Header("Health Settings")]
    public float MaxHealth = 100f;
    public float CurrentHealth;
    public float minHealthBeforeDestroy = 5f;
    private bool isDead = false;

    [Header("Particles")]
    public GameObject dustParticles;
    public GameObject asteroidDebriParticles;

    [Header("Scale Settings")]
    private Vector3 originalScale;
    private Vector3 threeFourthScale;
    private Vector3 twoFourthScale;
    private Vector3 oneFourthScale;
    private bool isThreeFourth = false;
    private bool isTwoFourth = false;
    private bool isOneFourth = false;

    [Header("Visual")]
    public Vector3 rotationSpeed = new Vector3(0f, 30f, 0f); // degrees per second
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;      // usually no gravity in space
            rb.linearDamping = 20f;               // no linear drag
            rb.angularDamping = 2f;        // no rotational drag
        }

        rb.angularDamping = Random.Range(rb.angularDamping - 1, rb.angularDamping + 3);
        
        // Randomize initial scale
        originalScale = new Vector3(
            Random.Range(5, 12),
            Random.Range(5, 12),
            Random.Range(5, 12)
        );
        transform.localScale = originalScale;

        // Set scales for health thresholds
        threeFourthScale = originalScale * 0.75f;
        twoFourthScale = originalScale * 0.5f;
        oneFourthScale = originalScale * 0.25f;

        // Scale Health Based On Size
        float sqMag = gameObject.transform.localScale.sqrMagnitude; // This is a float
        int sqMagInt = Mathf.FloorToInt(sqMag) / 3; // rounds down

        MaxHealth += sqMagInt;

        CurrentHealth = MaxHealth;

        // Convert degrees per second to radians per second for Rigidbody
        Vector3 angularVel = rotationSpeed * Mathf.Deg2Rad;
        rb.angularVelocity = angularVel;
    }

    void Update()
    {
        if (!isDead && CurrentHealth <= minHealthBeforeDestroy)
        {
            Logger.Log("OnDeath Triggered");
            AsteroidExplosion(false, 50, true);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        CurrentHealth -= damage;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);

        float threeFourth = MaxHealth * 0.75f;
        float twoFourth = MaxHealth * 0.5f;
        float oneFourth = MaxHealth * 0.25f;

        if (CurrentHealth <= oneFourth)
        {
            if (!isOneFourth)
            {
                AsteroidExplosion(true, damage / 2);
            }
            else
            {
                AsteroidExplosion(false, damage / 3);
            }

            transform.localScale = oneFourthScale;
            isOneFourth = true;
            isTwoFourth = false;
            isThreeFourth = false;
            Logger.Log("Asteroid 1/4");
        }
        else if (CurrentHealth <= twoFourth)
        {
            if (!isTwoFourth)
            {
                AsteroidExplosion(true, damage / 2);
            }
            else
            {
                AsteroidExplosion(false, damage / 3);
            }

            transform.localScale = twoFourthScale;
            isTwoFourth = true;
            isOneFourth = false;
            isThreeFourth = false;
            Logger.Log("Asteroid 2/4");
        }
        else if (CurrentHealth <= threeFourth)
        {
            if (!isThreeFourth)
            {
                AsteroidExplosion(true, damage / 3);
            }
            else
            {
                AsteroidExplosion(false, damage / 3);
            }

            transform.localScale = threeFourthScale;
            isThreeFourth = true;
            isTwoFourth = false;
            isOneFourth = false;
            Logger.Log("Asteroid 3/4");
        }
        else
        {
            // Full health
            AsteroidExplosion(false, damage / 3);
            isOneFourth = false;
            isTwoFourth = false;
            isThreeFourth = false;
        }
    }

    private void OnDeath()
    {
        isDead = true;
        AsteroidExplosion(true, 1);
    }

    public void AsteroidExplosion(bool bigExplosion = false, int extraParticles = 0, bool shouldDestroy = false)
    {
        float scaleFactor = transform.localScale.magnitude / originalScale.magnitude;

        ProximityToggleObject proximityToggleObject;
        gameObject.transform.parent.TryGetComponent(out proximityToggleObject);

        Rigidbody rigidbody;
        proximityToggleObject.rootToToggle.TryGetComponent(out rigidbody);

        Vector3 spawnPos = proximityToggleObject.rootToToggle != null ? proximityToggleObject.rootToToggle.transform.position : transform.position;

        Quaternion newRotation = rigidbody != null ? rigidbody.rotation : Random.rotation;

        // Dust
        GameObject dustVFX = Instantiate(dustParticles, spawnPos, newRotation);
        var dustMain = dustVFX.GetComponent<ParticleSystem>().main;
        dustMain.startSizeMultiplier *= scaleFactor;

        // Rocks
        GameObject rockVFX = Instantiate(asteroidDebriParticles, spawnPos, newRotation);
        var rockMain = rockVFX.GetComponent<ParticleSystem>().main;
        rockMain.startSizeMultiplier *= scaleFactor;

        if (bigExplosion || shouldDestroy)
        {
            dustMain.maxParticles = Mathf.RoundToInt(Random.Range(12 + extraParticles, 20 + extraParticles) * scaleFactor);
            dustVFX.GetComponent<ParticleSystem>().Play();
            Destroy(dustVFX, 2f);

            rockMain.maxParticles = Mathf.RoundToInt(Random.Range(8 + extraParticles, 15 + extraParticles) * scaleFactor);
            rockVFX.GetComponent<ParticleSystem>().Play();
            Destroy(rockVFX, 2f);

            if (shouldDestroy)
            {
                KillAsteroid();
            }

            return;
        }

        dustMain.maxParticles = Mathf.RoundToInt(Random.Range(4, 10 + extraParticles) * scaleFactor);
        dustVFX.GetComponent<ParticleSystem>().Play();
        Destroy(dustVFX, 2f);

        rockMain.maxParticles = Mathf.RoundToInt(Random.Range(4, 7 + extraParticles) * scaleFactor);
        rockVFX.GetComponent<ParticleSystem>().Play();
        Destroy(rockVFX, 2f);
    }

    public void KillAsteroid()
    {
        transform.localScale = Vector3.zero;
        Destroy(gameObject, 2f);
    }
}
