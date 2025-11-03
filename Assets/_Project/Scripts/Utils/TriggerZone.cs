using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerZone : MonoBehaviour
{
    public UnityEvent OnTriggerEnterEvent;
    public UnityEvent OnTriggerStayEvent;
    public UnityEvent OnTriggerExitEvent;
    public List<string> TagsToCompare;

    void OnTriggerEnter(Collider other)
    {
        if (TagsToCompare.Contains(other.tag))
        {
            OnTriggerEnterEvent.Invoke();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (TagsToCompare.Contains(other.tag))
        {
            OnTriggerStayEvent.Invoke();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (TagsToCompare.Contains(other.tag))
        {
            OnTriggerExitEvent.Invoke();
        }
    }

}
