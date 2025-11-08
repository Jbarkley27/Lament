using UnityEngine;
using UnityEngine.UI;

public class OverviewMapIcon : MonoBehaviour
{
    [Header("Map Setup")]
    private RectTransform mapRect;       // The UI map panel (assume square for circular map)
    public GameObject iconPrefab;        // Prefab of the UI icon (Image or custom)
    private RectTransform iconRect;

    private float mapRadius;             // Radius of the circular map

    void Start()
    {
        mapRect = GlobalDataStore.Instance.OverviewMapRoot.GetComponent<RectTransform>();

        if (mapRect == null || iconPrefab == null)
        {
            Debug.LogWarning($"[MapIcon] Missing setup on {gameObject.name}");
            return;
        }

        // Assuming mapRect is square
        mapRadius = Mathf.Min(mapRect.rect.width, mapRect.rect.height) / 2f;

        // Instantiate icon under map
        GameObject icon = Instantiate(iconPrefab, mapRect);
        iconRect = icon.GetComponent<RectTransform>();
    }

    void Update()
    {
        if (iconRect == null || mapRect == null)
            return;

        Vector3 worldPos = transform.position;

        // Normalize X and Z (or Y if using top-down world)
        float normalizedX = Mathf.InverseLerp(GlobalDataStore.Instance.MapManager.WorldMin.x, GlobalDataStore.Instance.MapManager.WorldMax.x, worldPos.x) - 0.5f;
        float normalizedZ = Mathf.InverseLerp(GlobalDataStore.Instance.MapManager.WorldMin.y, GlobalDataStore.Instance.MapManager.WorldMax.y, worldPos.z) - 0.5f;

        Vector2 normalizedPos = new Vector2(normalizedX, normalizedZ);

        // Clamp to unit circle
        if (normalizedPos.sqrMagnitude > 0.25f) // because normalizedPos ranges -0.5 to 0.5
        {
            normalizedPos = normalizedPos.normalized * 0.5f;
        }

        // Convert to UI position
        iconRect.anchoredPosition = normalizedPos * (mapRadius * 2f);

        // Optional: rotate icon to match world Y rotation
        iconRect.localRotation = Quaternion.Euler(0, 0, -transform.eulerAngles.y);
    }

    void OnDestroy()
    {
        if (iconRect != null)
            Destroy(iconRect.gameObject);
    }
}
