using UnityEngine;

public interface IProximityToggle
{
    void SetActive(bool active);
}

public class ProximityToggleObject : MonoBehaviour, IProximityToggle
{
    public GameObject rootToToggle; // can be itself or a parent

    public void SetActive(bool active)
    {
        if(rootToToggle != null)
            rootToToggle.SetActive(active);
    }
}
