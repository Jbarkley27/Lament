using UnityEngine;

[ExecuteAlways]
public class PlanetOrbitEditor : MonoBehaviour
{
    public Transform sun;       // Center of orbit
    public float orbitRadius = 10f;  // Distance from sun
    public float orbitSpeed = 10f;   // Degrees per second
    public float rotationSpeed = 30f; // Planet self-rotation
    public Color orbitColor = Color.white; // Gizmo color

    private void Update()
    {
        if (sun == null) return;

        // Orbit around Sun
        transform.position = sun.position + (transform.position - sun.position).normalized * orbitRadius;
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);

        #if UNITY_EDITOR
                // Orbit visually in editor without playing
                if (!Application.isPlaying)
                    transform.RotateAround(sun.position, Vector3.up, orbitSpeed * Time.deltaTime);
        #endif
    }
    
    void OnDrawGizmos()
    {
        if (sun == null) return;

        Gizmos.color = orbitColor;

        // Draw the orbit circle in XZ plane
        const int segments = 64;
        Vector3 previousPoint = sun.position + new Vector3(orbitRadius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = (i / (float)segments) * 2f * Mathf.PI;
            Vector3 newPoint = sun.position + new Vector3(Mathf.Cos(angle) * orbitRadius, 0, Mathf.Sin(angle) * orbitRadius);
            Gizmos.DrawLine(previousPoint, newPoint);
            previousPoint = newPoint;
        }
    }

}
