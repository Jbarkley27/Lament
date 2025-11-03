using UnityEngine;

public class RectTransformSpinner : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 10f; // degrees per second
    public bool clockwise = true;

    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
            Debug.LogError("SlowSpinner requires a RectTransform (UI element).");
    }

    void Update()
    {
        float direction = clockwise ? -1f : 1f;
        rectTransform.Rotate(0f, 0f, direction * rotationSpeed * Time.deltaTime);
    }
}
