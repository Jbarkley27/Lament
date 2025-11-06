using UnityEngine;
using UnityEngine.UI;

public class OverviewmapIcon : MonoBehaviour
{
    [Header("Map Setup")]
    private RectTransform mapRect;      // The UI map panel (assign once, or auto-find)
    public GameObject iconPrefab;      // Prefab of the UI icon (Image or custom)
    private RectTransform iconRect;

    void Start()
    {
        mapRect = GlobalDataStore.Instance.OverviewMapRoot.GetComponent<RectTransform>();

        if (mapRect == null || iconPrefab == null)
        {
            Debug.LogWarning($"[MapIcon] Missing setup on {gameObject.name}");
            return;
        }

        // Instantiate icon under map
        GameObject icon = Instantiate(iconPrefab, mapRect);
        iconRect = icon.GetComponent<RectTransform>();
    }

    void Update()
    {
        if (iconRect == null || mapRect == null)
            return;

        Vector3 worldPos = transform.position;

        // Only use X (horizontal) and Z (depth) for map projection
        float normalizedX = Mathf.InverseLerp(GlobalDataStore.Instance.MapManager.WorldMin.x, GlobalDataStore.Instance.MapManager.WorldMax.x, worldPos.x);
        float normalizedZ = Mathf.InverseLerp(GlobalDataStore.Instance.MapManager.WorldMin.y, GlobalDataStore.Instance.MapManager.WorldMax.y, worldPos.z);

        // Convert normalized values to UI anchored position
        float mapWidth = mapRect.rect.width;
        float mapHeight = mapRect.rect.height;

        float uiX = (normalizedX * mapWidth) - (mapWidth / 2f);
        float uiY = (normalizedZ * mapHeight) - (mapHeight / 2f);

        iconRect.anchoredPosition = new Vector2(uiX, uiY);

        // Optional: rotate icon to match world Y rotation
        iconRect.localRotation = Quaternion.Euler(0, 0, -transform.eulerAngles.y);
    }

    void OnDestroy()
    {
        if (iconRect != null)
            Destroy(iconRect.gameObject);
    }
}
