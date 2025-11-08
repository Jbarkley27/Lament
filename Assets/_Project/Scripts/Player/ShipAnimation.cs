using UnityEngine;
using UnityEngine.VFX;

public class ShipAnimation : MonoBehaviour
{
    [Header("General")]
    public PlayerMovement playerMovement;

    [Header("Animation")]
    [SerializeField] private Animator _animator;
    [SerializeField] private float _dampTime;


    [Header("VFX")]
    public VisualEffect speedLines;
    public ParticleSystem BoostVFX;
    public ParticleSystem.EmissionModule BoostVFXEmissionModule;


    void Start()
    {
        if(StarfieldManager.Instance.BoostVFX)
        {
            BoostVFX = StarfieldManager.Instance.BoostVFX;
            BoostVFXEmissionModule = BoostVFX.emission;
        }
    }

    public void Update()
    {
        HandleAnimations();
        HandleSpeedVFX();
        if(BoostVFX) BoostVFXEmissionModule.enabled = GlobalDataStore.Instance.InputManager.IsBoosting;
    }



    public void HandleAnimations()
    {
        if (_animator == null)
            return;

        float finalRollAmount = playerMovement._rotateDifference;

        if (playerMovement._rotateDirection < 0)
        {
            finalRollAmount *= -1;
        }

        float scaledRoll = ScaleValue(finalRollAmount);

        _animator.SetFloat("RotateDifference", scaledRoll, _dampTime, Time.deltaTime);
    }





    public float ScaleValue(float value)
    {
        float min = -45f;
        float max = 45f;

        // Ensure the value is clamped within the original range
        value = Mathf.Clamp(value, min, max);

        // Scale the value to the range -1 to 1
        return value / max; // Equivalent to (value - min) / (max - min) * 2 - 1
    }




    // VFX -----------------------------------------------------------------------
    public void HandleSpeedVFX()
    {
        if (!GlobalDataStore.Instance.InputManager.IsBoosting
            || !GlobalDataStore.Instance.PlayerStatModule.CanBoost())
        {
            speedLines.gameObject.SetActive(false);
            return;
        }

        speedLines.gameObject.SetActive(true);
    }
}
