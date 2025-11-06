using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using System.Collections.Generic;

public class BlasterManager : MonoBehaviour
{
    public static BlasterManager Instance { get; private set; }

    [Header("References")]
    private GameObject playerFireSource;
    [SerializeField] private InputManager inputManager;

    [Header("Weapon State")]
    public Blaster EquippedBlaster;
    public bool isRegeneratingAmmo;
    public bool canRegenerateAmmo = true;
    public bool isFiring;
    private Coroutine regenRoutine;
    public enum FiringMode {AUTO, MANUAL};

    [Header("Shooting Control")]
    public bool canFire = true;

    [Header("UI")]
    [SerializeField] private TMP_Text currentAmmoText;
    [SerializeField] private TMP_Text maxAmmoText;
    [SerializeField] private Image blasterIcon;
    [SerializeField] private CanvasGroup inputCG;
    [SerializeField] private Slider ammoSlider;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Duplicate WeaponSystems detected, destroying new instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeUI();
        playerFireSource = GlobalDataStore.Instance.PlayerFireSource;
    }

    private void Update()
    {
        HandleShootingInput();
        inputCG.alpha = GlobalDataStore.Instance.InputManager.IsShootingBlaster ? 0.2f : 0.9f;
    }

    private void InitializeUI()
    {
        EquippedBlaster.CurrentAmmo = EquippedBlaster.MaxAmmo;
        UpdateUI();
        blasterIcon.sprite = EquippedBlaster.BlasterIcon;
        ammoSlider.maxValue = EquippedBlaster.MaxAmmo;
        ammoSlider.value = EquippedBlaster.CurrentAmmo;
    }

    // ------------------------
    // SHOOTING LOGIC
    // ------------------------

    private void HandleShootingInput()
    {
        if (!canFire)
            return;

        bool shootingInput = inputManager.IsShootingBlaster;

        // For semi-auto weapons, only fire when the button is first pressed
        if (EquippedBlaster.FireType == FiringMode.MANUAL)
        {
            if (shootingInput && !isFiring && inputManager.HasReleasedShooting)
                StartShooting();
        }
        // For automatic weapons, fire as long as the button is held
        else if (EquippedBlaster.FireType == FiringMode.AUTO)
        {
            if (shootingInput && !isFiring)
                StartShooting();
        }
    }

    private void StartShooting()
    {
        if (!canFire || isFiring)
            return;

        Logger.Log("StartShooting()", Logger.LogTypeCategory.Debug);
        canFire = false;
        isFiring = true;
        canRegenerateAmmo = false;
        isRegeneratingAmmo = false;

        if (regenRoutine != null)
            StopCoroutine(regenRoutine);

        StartCoroutine(ShootRoutine());
    }

    private IEnumerator ShootRoutine()
    {
        Logger.Log("ShootRoutine()", Logger.LogTypeCategory.Debug);
        for (int i = 0; i < EquippedBlaster.ProjectilesPerShot; i++)
        {
            if (!HasAmmo()) break;
            Logger.Log("Shooting Now", Logger.LogTypeCategory.Debug);

            GameObject projectile = Instantiate(
                EquippedBlaster.ProjectilePrefab,
                playerFireSource.transform.position,
                Quaternion.LookRotation(GlobalDataStore.Instance.PlayerVisual.transform.forward)
            );

            ScreenShakeManager.Instance.DoShake(EquippedBlaster.ScreenShakeProfile);

            projectile.GetComponent<ProjectileBase>().Initialize(
                GetProjectileDirection(EquippedBlaster.Accuracy),
                EquippedBlaster.ProjectileSpeed,
                EquippedBlaster.Range,
                EquippedBlaster.BaseDamage,
                EquippedBlaster.impactEffects,
                EquippedBlaster.KnockbackForce
            );

            GlobalDataStore.Instance.PlayerMovement.KnockBack(EquippedBlaster.RecoilForce);

            EquippedBlaster.CurrentAmmo--;
            UpdateUI();

            yield return new WaitForSeconds(EquippedBlaster.ProjectilesPerShotFireRate);
        }
        

        if(EquippedBlaster.FireType == FiringMode.MANUAL)
        {
            inputManager.HasReleasedShooting = false;
        }

        // Cooldown before next shot
        yield return new WaitForSeconds(EquippedBlaster.FireRate);
        EndShooting();
    }

    private void EndShooting()
    {
        canFire = true;
        isFiring = false;
        canRegenerateAmmo = true;

        if (regenRoutine != null)
            StopCoroutine(regenRoutine);

        regenRoutine = StartCoroutine(RegenerateAmmo());
    }

    // ------------------------
    // AMMO LOGIC
    // ------------------------

    private IEnumerator RegenerateAmmo()
    {
        yield return new WaitForSeconds(2f);

        if (EquippedBlaster.CurrentAmmo >= EquippedBlaster.MaxAmmo)
            yield break;

        while (EquippedBlaster.CurrentAmmo < EquippedBlaster.MaxAmmo && !isFiring)
        {
            EquippedBlaster.CurrentAmmo++;
            UpdateUI();
            yield return new WaitForSeconds(EquippedBlaster.AmmoRegenRate);
        }
    }

    private bool HasAmmo(int amount = 1)
    {
        return EquippedBlaster.CurrentAmmo - amount >= 0;
    }

    private void UpdateUI()
    {
        currentAmmoText.text = EquippedBlaster.CurrentAmmo.ToString();
        maxAmmoText.text = EquippedBlaster.MaxAmmo.ToString();
        ammoSlider.value = EquippedBlaster.CurrentAmmo;
    }

    // ------------------------
    // HELPERS
    // ------------------------

    private Vector3 GetProjectileDirection(float spread)
    {
        Vector3 forward = GlobalDataStore.Instance.PlayerVisual.transform.forward;
        float offset = Random.Range(-spread, spread);
        return (forward + new Vector3(offset, 0, 0)).normalized;
    }
}

[System.Serializable]
public struct Blaster
{
    public float FireRate;
    public int MaxAmmo;
    public int BaseDamage;
    public float AmmoRegenRate;
    public int CurrentAmmo;
    public GameObject ProjectilePrefab;
    public float Accuracy;
    public int ProjectilesPerShot;
    public float ProjectilesPerShotFireRate;
    public float ProjectileSpeed;
    public float Range;
    public BlasterManager.FiringMode FireType;
    public Sprite BlasterIcon;
    public ScreenShakeManager.ShakeProfile ScreenShakeProfile;
    public float KnockbackForce;
    public float RecoilForce;
    public List<GameObject> impactEffects;
}
