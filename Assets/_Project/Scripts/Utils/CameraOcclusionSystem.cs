using System.Collections.Generic;
using UnityEngine;

// TODO: CONNECTOR
public class CameraOcclusionSystem : MonoBehaviour
{
    public Camera cam;
    public LayerMask occlusionLayerMask;
    public static List<OcclusionFader> AllCulledObjects = new List<OcclusionFader>();


   void Update()
    {
        Vector3 playerPos = GlobalDataStore.Instance.PlayerVisual.transform.position;
        Vector3 camPos = cam.transform.position;
        Vector3 dir = playerPos - camPos;
        float distanceToPlayer = dir.magnitude;
        dir.Normalize();

        RaycastHit[] hits = Physics.RaycastAll(camPos, dir, distanceToPlayer, occlusionLayerMask);

        List<OcclusionFader> objectsOccludedThisFrame = new List<OcclusionFader>();

        foreach (var h in hits)
        {
            OcclusionFader occlusionFader = h.collider.GetComponent<OcclusionFader>();
            if (occlusionFader != null)
                objectsOccludedThisFrame.Add(occlusionFader);
        }

        foreach (var obj in AllCulledObjects)
        {
            if (!objectsOccludedThisFrame.Contains(obj))
                obj.SetOccluded(false);
            else
                obj.SetOccluded(true);
        }
    }


    public static void AddToCulledObjects(OcclusionFader obj)
    {
        if (!AllCulledObjects.Contains(obj))
            AllCulledObjects.Add(obj);
    }
}
