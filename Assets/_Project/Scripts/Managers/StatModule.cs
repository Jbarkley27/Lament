using UnityEngine;


[CreateAssetMenu(fileName = "Stat_Module", menuName = "New Stat Module")]
public class StatModule : ScriptableObject
{
    public int Health;
    public int CurrentHealth;
    public int BoostMultipler;
    

    [Header("Thrust Settings")]
    [SerializeField] public float _xForce;
    [SerializeField] public float _yForce;
}