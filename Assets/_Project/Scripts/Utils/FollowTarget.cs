using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [Header("Target to follow")]
    [SerializeField] private Transform target;

    [Header("Follow Settings")]
    public float followSpeed = 5f; // how fast it follows
    public Vector3 offset = Vector3.zero; // position offset


    void Start()
    {
        target = GlobalDataStore.Instance.PlayerVisual.transform;
    }

    void LateUpdate()
    {
        if (target == null) return;
        transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * followSpeed);
    }
}
