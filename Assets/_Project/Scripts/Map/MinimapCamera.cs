using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    public float height = 100f; // how high above the player

    void LateUpdate()
    {
        Transform target = GlobalDataStore.Instance.PlayerVisual.transform;
        if (target == null) return;

        Vector3 newPos = target.position;
        newPos.y += height;
        transform.position = newPos;

        // copy the main camera's rotation on the y-axis
        Vector3 newRot = transform.eulerAngles;
        newRot.y = Camera.main.transform.eulerAngles.y;
        transform.eulerAngles = newRot;
    }
}
