using System.Collections.Generic;
using UnityEngine;

public class OcclusionFader : MonoBehaviour
{
    public bool isOccluded = false;
    public List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
    public List<Material> originalMaterials = new List<Material>();

    void Awake()
    {
        CameraOcclusionSystem.AddToCulledObjects(this);

        MeshRenderer[] rends = GetComponentsInChildren<MeshRenderer>();
        foreach (var r in rends)
        {
            meshRenderers.Add(r);
            originalMaterials.Add(r.material);
        }
    }

    public void SetOccluded(bool occluded, Material occlusionMaterial)
    {
        if (isOccluded == occluded) return;

        isOccluded = occluded;

        for (int i = 0; i < meshRenderers.Count; i++)
        {
            if (isOccluded)
            {
                meshRenderers[i].material = occlusionMaterial;
            }
            else
            {
                meshRenderers[i].material = originalMaterials[i];
            }
        }
    }
}
