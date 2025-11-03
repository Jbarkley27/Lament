using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetCamera : MonoBehaviour
{
    public Transform playerCamera;
    [Range(1,10000)]
    public float scaleFactor = 1000f;

    private void LateUpdate()
    {
        transform.rotation = playerCamera.rotation;
        transform.position = playerCamera.position / scaleFactor;
    }
}