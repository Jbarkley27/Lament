using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthModule : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth;
    private int currentHealth;
    private int currentShield;
    public ShieldGrade shieldGrade;



    [Header("UI")]
    public Slider healthSlider;
    public Slider shieldSlider;



    void Start()
    {
        InitializeUI();
    }




    public void InitializeUI()
    {
        maxHealth = GlobalDataStore.Instance.PlayerStatModule.Health;
        currentHealth = maxHealth;

        healthSlider.value = currentHealth;
        healthSlider.maxValue = maxHealth;

        // Initialize shield if applicable
        currentShield = GlobalDataStore.Instance.PlayerStatModule.Shield.shieldCapacity;
        if (currentShield <= 0)
        {
            shieldSlider.gameObject.SetActive(false);
        }
        else
        {
            shieldSlider.value = currentShield;
            shieldSlider.maxValue = currentShield;
        }
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
        UpdateUI();


        if (currentHealth <= 0)
        {
            Die();
        }
    }



    private void UpdateUI()
    {
        healthSlider.value = currentHealth;
        
        // update shield UI if applicable
        if (shieldSlider != null && shieldSlider.gameObject.activeSelf)
        {
            shieldSlider.value = currentShield;
        }
    }




    private void Die()
    {
        // Handle player death (e.g., trigger game over)
        Debug.Log("Player has died.");
    }

}



