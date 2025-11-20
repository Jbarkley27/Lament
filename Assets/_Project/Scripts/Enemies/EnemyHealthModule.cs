using UnityEngine;
using UnityEngine.UI;
using DamageNumbersPro;

public class EnemyHealthModule : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 100;
    private int currentHealth;
    public int maxShield = 0;
    private int currentShield;
    public ShieldGrade shieldGrade;
    public EnemyBase enemyBase;



    [Header("UI")]
    public GameObject healthUIInstance;
    public Slider healthSlider;
    public DamageNumber damageNumberPrefab;
    public float heightOffset = 5;
    private bool uiInitialized = false;
    public Slider shieldSlider;








    // Called by EnemyBase.OnSpawned()
    public void InitializeUI()
    {
        if (uiInitialized) return;

        if (healthUIInstance == null)
        {
            Debug.LogError($"Health UI Prefab missing on {name}");
            return;
        }

        Logger.Log("Initializing Enemy Health UI for " + name);

        currentHealth = maxHealth;

        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;


        // Initialize shield if applicable
        if (currentShield <= 0)
        {
            shieldSlider.gameObject.SetActive(false);
        }
        else
        {
            shieldSlider.gameObject.SetActive(true);
            shieldSlider.maxValue = maxShield;
            shieldSlider.value = currentShield;
        }

        uiInitialized = true;
    }




    public void OnSpawn()
    {
        if (!uiInitialized)
            InitializeUI();

        UpdateUI();
        healthUIInstance.SetActive(true);
    }




    public void OnDespawn()
    {
        if (healthUIInstance != null)
            healthUIInstance.SetActive(false);
    }



    public void TakeDamage(int amount)
    {
        int finalDamage = amount;

        // Apply shield reduction if shield is active using shield grade
        // then subtract from shield first before health
        if (currentShield > 0)
        {
            finalDamage = Mathf.CeilToInt(amount * (1 - shieldGrade.damageReduction));

            if (finalDamage >= currentShield)
            {
                finalDamage -= currentShield;
                currentShield = 0;
            }
            else
            {
                currentShield -= finalDamage;
                finalDamage = 0;
            }
        }

        currentHealth -= finalDamage;
        ShowDamageUI(finalDamage);
        UpdateUI();
        if (currentHealth <= 0)
        {
            Die();
        }
    }



    private void UpdateUI()
    {
        if (!uiInitialized) return;

        healthSlider.value = currentHealth;
        
        // update shield UI if applicable
        if (currentShield > 0 && shieldSlider != null)
        {
            shieldSlider.value = currentShield;
        }
    }




    private void Die()
    {
        enemyBase.HandleDeath();

        if (healthUIInstance != null)
            healthUIInstance.SetActive(false);
    }



    public void ResetHealth()
    {
        currentHealth = maxHealth;

        // shields
        currentShield = maxShield;
        if (shieldSlider != null && maxShield > 0)
            shieldSlider.value = currentShield;

        // Reset health bar UI
        if (healthSlider != null)
            healthSlider.value = maxHealth;
    }



    public void ShowDamageUI(int damage)
    {
        Vector3 offsetVec = new Vector3(transform.position.x, heightOffset, transform.position.z);
        if (damageNumberPrefab) damageNumberPrefab.Spawn(offsetVec, damage.ToString());
    }
}
