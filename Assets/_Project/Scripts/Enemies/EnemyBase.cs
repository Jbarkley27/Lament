using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    // ------------------------------ //
    //          STATE MACHINE
    // ------------------------------ //
    public enum EnemyState
    {
        Wandering,
        Seeking,
        Attacking
    }

    protected EnemyState currentState;
    public EnemyType enemyType;
    public bool debugMode = false;
    public GameObject enemyVisual;











    // ------------------------------ //
    //          COMPONENTS
    // ------------------------------ //
    protected Rigidbody rb;
    protected EnemyHealthModule health;
    public EnemyCamp owningCamp;

    // Assigned by spawner
    protected Transform player;

    // Movement parameters
    // [Header("Movement")]
    // public float moveSpeed = 3f;
    // public float turnSpeed = 10f;










    // ------------------------------ //
    //      ATTACK GATING SYSTEM
    // ------------------------------ //
    // protected bool canAttack = false;









    // ------------------------------ //
    //           UNITY HOOKS
    // ------------------------------ //
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        health = GetComponent<EnemyHealthModule>();
        if (debugMode)
        {
            if (health != null)
            {
                health.InitializeUI();
            }
        }
    }








    // ------------------------------ //
    //         STATE MACHINE
    // ------------------------------ //
    // private void RunStateMachine()
    // {
    //     switch (currentState)
    //     {
    //         case EnemyState.Wandering:
    //             WanderingState();
    //             break;

    //         case EnemyState.Seeking:
    //             SeekingState();
    //             break;

    //         case EnemyState.Attacking:
    //             AttackingState();
    //             break;
    //     }
    // }

    // protected virtual void WanderingState() { }
    // protected virtual void SeekingState() { }
    // protected virtual void AttackingState() { }








    

    // ------------------------------ //
    //         SPAWN/DESPAWN
    // ------------------------------ //
    public virtual void OnSpawned()
    {
        player = GlobalDataStore.Instance.PlayerVisual.transform;

        if (health != null)
        {
            health.OnSpawn();
            health.ResetHealth();
        }

        // EnterIdleState();
    }




    public virtual void OnDespawned()
    {
        if (health != null)
        {
            // Despawn Health
            health.OnDespawn();
        }
    }









    





    // ------------------------------ //
    //            DEATH
    // ------------------------------ //
    public virtual void HandleDeath()
    {
        Die();
        // GlobalEnemyPool.Instance.DespawnEnemy(gameObject);
    }









    // ------------------------------ //
    //          STATE HELPERS
    // ------------------------------ //
    // protected void EnterWanderingState() => currentState = EnemyState.Wandering;
    // protected void EnterSeekingState() => currentState = EnemyState.Seeking;
    // protected void EnterAttackingState() => currentState = EnemyState.Attacking;











    // ------------------------------ //
    //          MOVEMENT HELPERS
    // ------------------------------ //
    // protected void MoveTowards(Vector3 targetPos)
    // {
    //     Vector3 dir = (targetPos - transform.position).normalized;

    //     rb.MovePosition(transform.position + dir * moveSpeed * Time.deltaTime);

    //     // Rotate to face direction
    //     if (dir.sqrMagnitude > 0.01f)
    //     {
    //         Quaternion lookRot = Quaternion.LookRotation(dir);
    //         transform.rotation = Quaternion.Lerp(
    //             transform.rotation,
    //             lookRot,
    //             Time.deltaTime * turnSpeed
    //         );
    //     }
    // }

    // protected float DistanceToPlayer()
    // {
    //     if (player == null) return Mathf.Infinity;
    //     return Vector3.Distance(transform.position, player.position);
    // }

    public void Die()
    {
        // owningCamp?.NotifyEnemyDied(this);
        enemyVisual.SetActive(false);

        // Play explosion effect
        GlobalDataStore.Instance.ExplosionManager.SpawnExplosion(
            ExplosionManager.ExplosionType.Basic_Enemy,
            transform.position
        );

        gameObject.SetActive(false);
    }
}
