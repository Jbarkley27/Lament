using System.Collections.Generic;
using UnityEngine;

public class AsteroidManager : MonoBehaviour
{
    public float activationDistance = 50f;
    public float deactivationDistance = 60f; // hysteresis

    

    private List<IProximityToggle> managedObjects = new List<IProximityToggle>();

    void Awake()
    {
        GlobalDataStore.Instance.AsteroidManager = this;
    }




    void Update()
    {
        float sqActivate = activationDistance * activationDistance;
        float sqDeactivate = deactivationDistance * deactivationDistance;
        Vector3 playerPos = GlobalDataStore.Instance.PlayerMovement.gameObject.transform.position;

        for (int i = 0; i < managedObjects.Count; i++)
        {
            var obj = managedObjects[i];
            var go = (obj as MonoBehaviour).gameObject;
            float sqDist = (go.transform.position - playerPos).sqrMagnitude;

            if(sqDist < sqActivate)
                obj.SetActive(true);
            else if(sqDist > sqDeactivate)
                obj.SetActive(false);
        }
    }

    // Call this when spawning asteroids dynamically
    public void RegisterObject(IProximityToggle obj)
    {
        if (!managedObjects.Contains(obj))
            managedObjects.Add(obj);
    }

    public void UnregisterObject(IProximityToggle obj)
    {
        managedObjects.Remove(obj);
    }
}
