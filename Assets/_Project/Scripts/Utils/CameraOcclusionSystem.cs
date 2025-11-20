using System.Collections.Generic;
using UnityEngine;

// TODO: CONNECTOR
public class CameraOcclusionSystem : MonoBehaviour
{
    public Camera cam;
    public float distance = 200f;
    public LayerMask occlusionLayerMask;
    public static List<OcclusionFader> AllCulledObjects = new List<OcclusionFader>();
    public Material occlusionMaterial;

    void Update()
    {
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit[] hits = Physics.RaycastAll(ray, distance, occlusionLayerMask);

        List<OcclusionFader> objectsOcculedThisFrame = new List<OcclusionFader>();

        foreach (var h in hits)
        {
            GameObject obj = h.collider.gameObject;
            OcclusionFader occlusionFader = obj.GetComponent<OcclusionFader>();
            if (occlusionFader != null)
                objectsOcculedThisFrame.Add(occlusionFader);
        }

        // Reactivate objects that are not occluding
        foreach (var obj in AllCulledObjects)
        {
            if (!objectsOcculedThisFrame.Contains(obj))
            {
                obj.SetOccluded(false, occlusionMaterial);
            }
            else
            {
                obj.SetOccluded(true, occlusionMaterial);
            }
        }
    }

    public static void AddToCulledObjects(OcclusionFader obj)
    {
        if (!AllCulledObjects.Contains(obj))
            AllCulledObjects.Add(obj);
    }
}
