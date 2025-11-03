using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;



public class PlayerMovement : MonoBehaviour
{

    [Header("General")]
    [SerializeField] public Rigidbody _rb;
    public InputManager _inputManager;



    [Header("Rotate Settings")]
    [SerializeField] private float _rotateSpeed = 1.0f;
    public float _rotateDirection;
    public float _rotateDifference;


    public StatModule PlayerStatModule;

    private void Start()
    {

    }


    void Update()
    {

    }

    public Vector3 GetPlayerVelocity()
    {
        return _rb.linearVelocity;
    }


    private void FixedUpdate()
    {
        Thrust();
        RotateTowards(WorldCursor.instance.GetDirectionFromWorldCursor(transform.position));
        Boost();
    }









    // THRUST HANDLING ------------------------------------------------------
    private void Thrust()
    {
        if (_inputManager.ThrustInput.magnitude == 0 || _inputManager.IsBoosting) return;

        float xThrustInput = _inputManager.ThrustInput.x;
        float yThrustInput = _inputManager.ThrustInput.y;

        // Project camera-relative directions onto horizontal plane
        // Get camera-relative directions
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        // Flatten the vectors to the XZ plane
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 movementDirectionX = camRight * xThrustInput * PlayerStatModule._xForce;
        Vector3 movementDirectionY = camForward * yThrustInput * PlayerStatModule._yForce;

        _rb.AddForce(movementDirectionX, ForceMode.Force);
        _rb.AddForce(movementDirectionY, ForceMode.Force);
    }










    // ROTATION HANDLING -----------------------------------------------------
    private void RotateTowards(Vector3 targetDirection)
    {
        if (targetDirection == Vector3.zero)
            return;

        // Calculate the target rotation
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

        targetRotation = targetRotation.normalized;

        // only rotate around the y axis
        targetRotation.x = 0;
        targetRotation.z = 0;


        // Smoothly interpolate between current and target rotation
        Quaternion smoothedRotation = Quaternion.Slerp(
            _rb.rotation,                              // Current rotation
            targetRotation,                            // Target rotation
            _rotateSpeed * Time.deltaTime              // Interpolation factor
        );

        // used for animation
        _rotateDifference = Quaternion.Angle(gameObject.transform.rotation, targetRotation);
        _rotateDirection = Vector3.Dot(targetDirection, transform.right);


        smoothedRotation = smoothedRotation.normalized;

        // Apply the smooth rotation to the Rigidbody
        _rb.MoveRotation(smoothedRotation);
    }





    // BOOST HANDLING ------------------------------------------------------
    private void Boost()
    {
        if (!_inputManager.IsBoosting) return;
            _rb.AddForce(gameObject.transform.forward * PlayerStatModule.BoostMultipler, ForceMode.Impulse);
    }
}
