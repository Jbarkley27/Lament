using UnityEngine;

public class ParallaxObject : MonoBehaviour
{
    [Tooltip("Reference to the main camera (auto-assigned if null).")]
    public Transform cameraTransform;

    [Tooltip("How much parallax movement to apply (0 = static, 1 = moves with camera).")]
    [Range(0f, 1f)]
    public float parallaxStrength = 0.1f;

    private Vector3 startPosition;
    private Vector3 cameraStartPosition;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        startPosition = transform.position;
        cameraStartPosition = cameraTransform.position;
    }

    void LateUpdate()
    {
        // Compute the camera’s offset since start
        Vector3 cameraOffset = cameraTransform.position - cameraStartPosition;

        // Apply a fraction of that offset to this object's start position
        transform.position = startPosition + cameraOffset * parallaxStrength;
    }
}
