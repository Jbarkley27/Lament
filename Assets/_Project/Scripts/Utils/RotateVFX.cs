using UnityEngine;

public class VFXRotateWithVelocity : MonoBehaviour
{
    public Rigidbody targetRigidbody; // Assign the Rigidbody you want to track in the Inspector
    public float rotationSpeed = 5f; // Adjust for desired rotation speed

    void FixedUpdate()
    {
        // Ensure the target Rigidbody is assigned and is moving
        if (targetRigidbody != null && targetRigidbody.linearVelocity.magnitude > 0.01f) 
        {
            // Calculate the direction of movement
            Vector3 movementDirection = targetRigidbody.linearVelocity.normalized;

           
        }
    }
}
