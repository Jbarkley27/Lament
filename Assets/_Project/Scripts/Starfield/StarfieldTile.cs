using UnityEngine;

public class StarfieldTile : MonoBehaviour
{
    public GameObject starfieldRoot;
    public bool playerInSection = false;
    public ParticleSystem particleSystemComponent;
    public ParticleSystem.VelocityOverLifetimeModule velocityModule;
    public bool alwaysOn = false;

    void Start()
    {
        velocityModule = particleSystemComponent.velocityOverLifetime;

        if (!alwaysOn)
        {
            playerInSection = false;
            starfieldRoot.SetActive(false);
        }
    }


    public void ShowStarfield()
    {
        Logger.Log("Player Entered - " + gameObject.name, Logger.LogTypeCategory.Debug);
        playerInSection = true;
        starfieldRoot.SetActive(true);
        StarfieldManager.Instance.AddToList(this);
    }

    public void OnTriggerStay()
    {
        playerInSection = true;
        starfieldRoot.SetActive(true);
        if (!StarfieldManager.Instance.InList(this))
            StarfieldManager.Instance.AddToList(this);
    }

    public void OnTriggerExit()
    {
        Logger.Log("Player Left - " + gameObject.name, Logger.LogTypeCategory.Debug);
        playerInSection = false;
        starfieldRoot.SetActive(false);
        StarfieldManager.Instance.RemoveFromList(this);
    }

    //    void Update()
    // {
    //     HandleParallax();
    // }


    // public void HandleParallax()
    // {
    //     Vector3 playerLinearVelocity = -GlobalDataStore.Instance.PlayerMovement.GetPlayerVelocity();

    //     velocityModule.x = new ParticleSystem.MinMaxCurve(playerLinearVelocity.x);
    //     velocityModule.y = new ParticleSystem.MinMaxCurve(playerLinearVelocity.y);
    //     velocityModule.z = new ParticleSystem.MinMaxCurve(playerLinearVelocity.z);
    // }
}
