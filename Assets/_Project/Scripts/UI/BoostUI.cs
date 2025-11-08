using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BoostUI: MonoBehaviour
{
    public Slider BoostSlider;
    // private StatModule PlayerStatModule;
    public bool IsRecoveringBoost = false;
    public CanvasGroup boostCG;
    private Coroutine boostRegenCo;

    void Start()
    {
        // PlayerStatModule = GlobalDataStore.Instance.PlayerStatModule;
        BoostSlider.maxValue = GlobalDataStore.Instance.PlayerStatModule.BoostMax;
        BoostSlider.value = GlobalDataStore.Instance.PlayerStatModule.BoostMax;
    }

    public void Update()
    {
        MonitorBoost();
        BoostSlider.value = GlobalDataStore.Instance.PlayerStatModule.CurrentBoost;
        boostCG.alpha = GlobalDataStore.Instance.PlayerStatModule.CurrentBoost >= GlobalDataStore.Instance.PlayerStatModule.BoostMax ? 0 : 1;
    }

    public void MonitorBoost()
    {
        if (GlobalDataStore.Instance.InputManager.IsBoosting
            && GlobalDataStore.Instance.PlayerStatModule.CanBoost())
        {
            if (IsRecoveringBoost)
            {
                IsRecoveringBoost = false;
                StopCoroutine(boostRegenCo);
                // Logger.Log("Stopping Boost Regen", Logger.LogTypeCategory.Debug);
            }

            GlobalDataStore.Instance.PlayerStatModule.CurrentBoost -= 1 * GlobalDataStore.Instance.PlayerStatModule.BoostConsumptionRate;
        }
        else
        {
            if (!IsRecoveringBoost)
            {
                // Logger.Log("Initiating Boost Regen", Logger.LogTypeCategory.Debug);
                boostRegenCo = StartCoroutine(RegenBoost());
            }
        }
    }
    
    public IEnumerator RegenBoost()
    {
        IsRecoveringBoost = true;
        yield return new WaitForSeconds(GlobalDataStore.Instance.PlayerStatModule.BoostStartRegenDelay);

        while (GlobalDataStore.Instance.PlayerStatModule.CurrentBoost < GlobalDataStore.Instance.PlayerStatModule.BoostMax)
        {
            // GlobalDataStore.Instance.PlayerStatModule.CurrentBoost = Mathf.Clamp(
            //     GlobalDataStore.Instance.PlayerStatModule.CurrentBoost,
            //     0,
            //     (GlobalDataStore.Instance.PlayerStatModule.BoostRegenRate * 1) + GlobalDataStore.Instance.PlayerStatModule.CurrentBoost);

            GlobalDataStore.Instance.PlayerStatModule.CurrentBoost += GlobalDataStore.Instance.PlayerStatModule.BoostRegenRate;

            yield return new WaitForSeconds(.2f);
        }

        IsRecoveringBoost = false;
    }
}