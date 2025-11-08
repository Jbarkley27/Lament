using UnityEngine;

public class ContinuousRotation : MonoBehaviour
{
    [Header("Enable rotation on each axis")]
    public bool rotateX = true;
    public bool rotateY = true;
    public bool rotateZ = true;

    [Header("Rotation speed (degrees per second)")]
    public float speedX = 30f;
    public float speedY = 45f;
    public float speedZ = 60f;

    [Tooltip("Apply rotation in local space (true) or world space (false)")]
    public bool useLocalSpace = true;

    void Update()
    {
        float deltaX = rotateX ? speedX * Time.deltaTime : 0f;
        float deltaY = rotateY ? speedY * Time.deltaTime : 0f;
        float deltaZ = rotateZ ? speedZ * Time.deltaTime : 0f;

        Vector3 rotation = new Vector3(deltaX, deltaY, deltaZ);

        if (useLocalSpace)
            transform.Rotate(rotation, Space.Self);
        else
            transform.Rotate(rotation, Space.World);
    }
}
