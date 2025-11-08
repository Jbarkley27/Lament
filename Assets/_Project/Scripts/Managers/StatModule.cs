using UnityEngine;


public class StatModule : MonoBehaviour
{
    [Header("Health")]
    public int Health;
    public int CurrentHealth;

    [Header("Boost")]
    public int BoostMultipler;
    public float BoostMax;
    public float CurrentBoost;
    public float BoostConsumptionRate;
    public float BoostRegenRate;
    public float BoostStartRegenDelay;
    

    [Header("Thrust Settings")]
    [SerializeField] public float _xForce;
    [SerializeField] public float _yForce;



    void Start()
    {
        CurrentBoost = BoostMax;
    }

    
    public bool CanBoost()
    {
        return CurrentBoost > 0;
    }
}