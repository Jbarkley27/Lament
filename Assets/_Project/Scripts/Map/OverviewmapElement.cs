using UnityEngine;
using UnityEngine.UI;

public class OverviewmapIcon : MonoBehaviour
{
    [Header("Map Setup")]
    private RectTransform mapRect; // The UI map panel
    public GameObject iconPrefab;  // Prefab of the UI icon (Image or custom)
    private RectTransform iconRect;
    public bool IsPlayer = false;

    [Header("Camera Reference")]
    public Transform cameraTransform; // Reference to your isometric camera (optional)

    private Quaternion mapRotationOffset;

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
        if (IsPlayer)
            icon.transform.SetAsLastSibling();
        else
            icon.transform.SetAsFirstSibling();

        iconRect = icon.GetComponent<RectTransform>();

        // Grab camera transform if not set
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        // Store offset based on camera's Y rotation to correct map alignment
        // The -45° rotation (isometric) needs to be inverted for proper map alignment
        float cameraYRot = cameraTransform != null ? cameraTransform.eulerAngles.y : -45f;
        mapRotationOffset = Quaternion.Euler(0, -cameraYRot, 0);
    }

    void Update()
    {
        if (iconRect == null || mapRect == null)
            return;

        Vector3 worldPos = transform.position;

        // --- Apply rotation offset so map aligns with the isometric world orientation ---
        Vector3 adjustedPos = mapRotationOffset * new Vector3(worldPos.x, 0, worldPos.z);

        // Only use X and Z for map projection
        float normalizedX = Mathf.InverseLerp(
            GlobalDataStore.Instance.MapManager.WorldMin.x,
            GlobalDataStore.Instance.MapManager.WorldMax.x,
            adjustedPos.x
        );

        float normalizedZ = Mathf.InverseLerp(
            GlobalDataStore.Instance.MapManager.WorldMin.y,
            GlobalDataStore.Instance.MapManager.WorldMax.y,
            adjustedPos.z
        );

        // Convert normalized values to UI anchored position
        float mapWidth = mapRect.rect.width;
        float mapHeight = mapRect.rect.height;

        float uiX = (normalizedX * mapWidth) - (mapWidth / 2f);
        float uiY = (normalizedZ * mapHeight) - (mapHeight / 2f);

        iconRect.anchoredPosition = new Vector2(uiX, uiY);

        // --- Correct rotation so the icon faces same world direction as player ---
        if (IsPlayer)
        {
            // Player icon should rotate based on player yaw, adjusted for camera rotation
            float adjustedYaw = transform.eulerAngles.y - (cameraTransform != null ? cameraTransform.eulerAngles.y : -45f);
            iconRect.localRotation = Quaternion.Euler(0, 0, -adjustedYaw);
        }
        else
        {
            // Regular icons can use direct Y rotation if desired
            iconRect.localRotation = Quaternion.Euler(0, 0, -transform.eulerAngles.y);
        }
    }

    void OnDestroy()
    {
        if (iconRect != null)
            Destroy(iconRect.gameObject);
    }
}
