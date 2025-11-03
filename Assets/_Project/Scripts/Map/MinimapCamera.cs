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
    }
}
