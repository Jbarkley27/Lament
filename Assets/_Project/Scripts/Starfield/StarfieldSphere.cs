using UnityEngine;

public class StarfieldSphere : MonoBehaviour
{
    public ParticleSystem particleSystemComponent;
    public ParticleSystem.VelocityOverLifetimeModule velocityModule;


    void Start()
    {
        velocityModule = particleSystemComponent.velocityOverLifetime;
        velocityModule.enabled = true;
    }

    void Update()
    {
        HandleParallax();
    }


    public void HandleParallax()
    {
        Vector3 playerLinearVelocity = -GlobalDataStore.Instance.PlayerMovement.GetPlayerVelocity();

        velocityModule.x = new ParticleSystem.MinMaxCurve(playerLinearVelocity.x);
        velocityModule.y = new ParticleSystem.MinMaxCurve(playerLinearVelocity.y);
        velocityModule.z = new ParticleSystem.MinMaxCurve(playerLinearVelocity.z);
    }
}
