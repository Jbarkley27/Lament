using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DamageNumbersPro;

public class HealthModule : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    private float currentHealth;
    public EnemyBase enemyBase;



    [Header("UI")]
    // public GameObject healthUIPrefab;   // Assigned in inspector
    public GameObject healthUIInstance;
    private Slider slider;
    private TMP_Text percentText;
    public DamageNumber damageNumberPrefab;
    public float heightOffset = 5;
    private bool uiInitialized = false;



    private void Awake()
    {
        currentHealth = maxHealth;
    }





    // Called by EnemyBase.OnSpawned()
    public void InitializeUI()
    {
        if (uiInitialized) return;

        if (healthUIInstance == null)
        {
            Debug.LogError($"Health UI Prefab missing on {name}");
            return;
        }

        // Instantiate under the global health bar canvas
        // Transform root = GlobalEnemyPool.Instance.EnemyHealthCanvas.transform;
        // healthUIInstance = Instantiate(healthUIPrefab, root);

        slider = healthUIInstance.GetComponentInChildren<Slider>();
        percentText = healthUIInstance.GetComponentInChildren<TMP_Text>();

        if (slider == null) Debug.LogError("Slider missing from health UI prefab");
        if (percentText == null) Debug.LogError("TMP_Text missing from health UI prefab");

        healthUIInstance.SetActive(false);

        // Register UI follow script
        // UIFollowTarget follow = healthUIInstance.GetComponent<UIFollowTarget>();
        // if (follow == null)
        // {
        //     follow = healthUIInstance.AddComponent<UIFollowTarget>();
        // }
        slider.maxValue = maxHealth;
        slider.value = currentHealth;
        // percentText.text = Mathf.RoundToInt((currentHealth / maxHealth) * 100f) + "%";
        percentText.text = currentHealth.ToString();

        // follow.target = this.transform;

        uiInitialized = true;
    }




    public void OnSpawn()
    {
        currentHealth = maxHealth;

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
        ShowDamageUI(amount);
        currentHealth -= amount;

        if (currentHealth < 0)
            currentHealth = 0;

        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }



    private void UpdateUI()
    {
        if (!uiInitialized) return;

        slider.value = currentHealth;
        // percentText.text = Mathf.RoundToInt((currentHealth / maxHealth) * 100f) + "%";
        percentText.text = currentHealth.ToString();
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

        // Reset health bar UI
        if (slider != null)
            slider.value = maxHealth;

        if (percentText != null)
            percentText.text = maxHealth.ToString();


        // Re-enable visual model if you hide/dissolve it on death
        // if (enemyModel != null)
        //     enemyModel.SetActive(true);

        // Re-enable collider(s)
        // if (mainCollider != null)
        //     mainCollider.enabled = true;

        // Re-enable hit flash / damage feedback systems if any
    }



    public void ShowDamageUI(int damage)
    {
        Vector3 offsetVec = new Vector3(transform.position.x, heightOffset, transform.position.z);

        //Spawn new popup at transform.position with a random number between 0 and 100.
        if (damageNumberPrefab) damageNumberPrefab.Spawn(offsetVec, damage.ToString());
    }
}
