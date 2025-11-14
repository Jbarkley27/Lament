using UnityEngine;

public class UIFollowTarget : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2f, 0);

    private RectTransform rect;
    private Camera cam;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 screenPos = cam.WorldToScreenPoint(target.position + offset);
        rect.position = screenPos;
    }
}
