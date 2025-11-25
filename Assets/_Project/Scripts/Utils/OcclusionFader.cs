using System.Collections.Generic;
using UnityEngine;

public class OcclusionFader : MonoBehaviour
{
    public bool isOccluded = false;
    public List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
    public List<Material> originalMaterials = new List<Material>();
    private Material _occlusionMaterialInstance;

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



    void Start()
    {
        // Force material instantiation (prevents stutter later)
        if (GlobalDataStore.Instance == null || GlobalDataStore.Instance.CameraOcclusionMaterial == null)
        {
            Debug.LogError("GlobalDataStore or CameraOcclusionMaterial is null!", this);
            return;
        }

        _occlusionMaterialInstance = new Material(GlobalDataStore.Instance.CameraOcclusionMaterial);
    }



    public void SetOccluded(bool occluded)
    {
        if (isOccluded == occluded) return;

        isOccluded = occluded;

        for (int i = 0; i < meshRenderers.Count; i++)
        {
            if (isOccluded)
            {
                meshRenderers[i].material = _occlusionMaterialInstance;
            }
            else
            {
                meshRenderers[i].material = originalMaterials[i];
            }
        }
    }
}
