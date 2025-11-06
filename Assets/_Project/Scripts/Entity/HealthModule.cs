using UnityEngine;
using DamageNumbersPro;

public class HealthModule: MonoBehaviour
{
    public DamageNumber damageNumberPrefab;
    public float heightOffset = 5;
    
    void Start()
    {
       
    }


    
    public void ShowDamageUI()
    {
        Vector3 offsetVec = new Vector3(transform.position.x, heightOffset, transform.position.z);

        //Spawn new popup at transform.position with a random number between 0 and 100.
        if(damageNumberPrefab) damageNumberPrefab.Spawn(offsetVec, Random.Range(1, 999));
    }
}