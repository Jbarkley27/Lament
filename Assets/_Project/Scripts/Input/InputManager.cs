using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Header("General")]
    public PlayerInput _playerInput;
    public PlayerMovement _playerMovement;


    [Header("Thrust")]
    public Vector2 ThrustInput;


    [Header("Cursor")]
    public Vector2 CursorInput;
    public Vector3 CursorPosition;



    [Header("Boosting")]
    public bool IsBoosting = false;


    [Header("Current Device Settings")]
    public InputDevice CurrentDevice;
    public enum InputDevice { K_M, GAMEPAD };



    private void Start()
    {
        CursorInput = new Vector2(0, 0);
        _playerInput = GetComponent<PlayerInput>();
        _playerMovement = GetComponent<PlayerMovement>();
    }



    void Update()
    {
        GetCurrentDevice();
    }



    public void Cursor(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            CursorInput = context.ReadValue<Vector2>();
        }
    }




    public void Boost()
    {
        if (GlobalDataStore.Instance.MapManager.minimapOpen)
        {
            IsBoosting = false;
            return;
        }
        

        if (_playerInput.actions["Boost"].IsPressed())
        {
            IsBoosting = true;
        }
        else if (_playerInput.actions["Boost"].WasReleasedThisFrame())
        {
            IsBoosting = false;
        }
    }


    public void Thrust(InputAction.CallbackContext context)
    {
        if (GlobalDataStore.Instance.MapManager.minimapOpen)
        {
            ThrustInput = Vector2.zero;
            return;
        }

        
        if (context.performed)
        {
            ThrustInput = context.ReadValue<Vector2>();
        }
    }


    public void OpenMap(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            GlobalDataStore.Instance.MapManager.ToggleMap();
        }
    }



    public void GetCurrentDevice()
    {
        if (_playerInput.currentControlScheme == "M&K")
        {
            CurrentDevice = InputDevice.K_M;
        }
        else if (_playerInput.currentControlScheme == "Gamepad")
        {
            CurrentDevice = InputDevice.GAMEPAD;
        }
    }
}
