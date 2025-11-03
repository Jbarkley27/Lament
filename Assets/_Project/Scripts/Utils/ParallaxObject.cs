using UnityEngine;

public class ParallaxObject : MonoBehaviour
{
    public Transform player;                 // usually the player's ship
    [Range(0f, 1f)] 
    private Vector3 lastPlayerPos;

    public float parallaxStrength = 0.1f; 
    public float globalParallaxScale = 0.01f; // smaller = slower motion    

    void Start()
    {
        player = GlobalDataStore.Instance.PlayerVisual.transform;
        if (player != null)
            lastPlayerPos = player.position;
    }

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 delta = player.position - lastPlayerPos;

        // Move opposite the player's motion
        transform.position -= delta * parallaxStrength * globalParallaxScale * .001f;

        lastPlayerPos = player.position;
    }
}
