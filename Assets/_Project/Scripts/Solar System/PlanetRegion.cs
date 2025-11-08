using System.Collections.Generic;
using UnityEngine;

public class PlanetRegion : MonoBehaviour
{
    public Color backgroundColor = Color.white; // desired space tint
    public float transitionSpeed = 2f;
    private Camera mainCamera;
    public List<GameObject> gameObjectsToCancelOnceRegionIsLeft = new List<GameObject>();

    void Start()
    {
        mainCamera = Camera.main;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerVisual"))
            StopAllCoroutines();
        // StartCoroutine(LerpBackground(mainCamera, backgroundColor));
        ShowElements();
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerVisual"))
            StopAllCoroutines();
            // StartCoroutine(LerpBackground(mainCamera, StarfieldManager.Instance.DeadSpaceBackgroundColor));
        HideElements();
    }

    System.Collections.IEnumerator LerpBackground(Camera cam, Color targetColor)
    {
        Color startColor = cam.backgroundColor;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * transitionSpeed;
            cam.backgroundColor = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }
    }

    public void HideElements()
    {
        foreach (GameObject element in gameObjectsToCancelOnceRegionIsLeft)
        {
            element.SetActive(false);
        }
    }
    
    public void ShowElements()
    {
        foreach (GameObject element in gameObjectsToCancelOnceRegionIsLeft)
        {
            element.SetActive(true);
        }
    }
}
