using UnityEngine;

public class PlanetDebug : MonoBehaviour
{
    public float spawnDistance = 10f; // How far from the planet the player should spawn

    [ContextMenu("Teleport Player Here")]
    void TeleportPlayer()
    {
        var player = GameObject.FindWithTag("Player"); // Make sure your player has the "Player" tag
        if (player == null)
        {
            Debug.LogWarning("Player not found!");
            return;
        }

        // Move player in front of the planet (arbitrary forward direction)
        Vector3 direction = (player.transform.position - transform.position).normalized;
        if (direction == Vector3.zero) direction = Vector3.forward;


        player.transform.position = new Vector3(transform.position.x, 0, transform.position.z) + direction * spawnDistance;

        // Optional: face the planet
        // player.transform.LookAt(transform.position);

        Debug.Log("Player teleported near " + name);
    }
}
