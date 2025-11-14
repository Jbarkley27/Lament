using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    // ------------------------------ //
    //          STATE MACHINE
    // ------------------------------ //
    public enum EnemyState
    {
        Idle,
        Wandering,
        Seeking,
        Attacking
    }

    protected EnemyState currentState;
    public EnemyType enemyType;

    // ------------------------------ //
    //          COMPONENTS
    // ------------------------------ //
    protected Rigidbody rb;
    protected HealthModule health;
    public EnemyCamp owningCamp;

    // Assigned by spawner
    protected Transform player;

    // Movement parameters
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float turnSpeed = 10f;

    // Tilt behavior
    [Header("Tilt")]
    public bool useTilt = false;
    public float tiltAmount = 10f;

    // Cached velocity for tilt
    private Vector3 lastMoveDirection = Vector3.zero;

    // ------------------------------ //
    //      ATTACK GATING SYSTEM
    // ------------------------------ //
    protected bool canAttack = false;

    // Subclasses override this:
    protected virtual bool RequestAttackPermission() => true;

    // ------------------------------ //
    //           UNITY HOOKS
    // ------------------------------ //
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        health = GetComponent<HealthModule>();

        if (health != null)
            health.OnDeath += HandleDeath;
    }

    protected virtual void Update()
    {
        RunStateMachine();
        ApplyTilt();
    }

    // ------------------------------ //
    //         STATE MACHINE
    // ------------------------------ //
    private void RunStateMachine()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                IdleState();
                break;

            case EnemyState.Wandering:
                WanderingState();
                break;

            case EnemyState.Seeking:
                SeekingState();
                break;

            case EnemyState.Attacking:
                AttackingState();
                break;
        }
    }

    protected virtual void IdleState() { }
    protected virtual void WanderingState() { }
    protected virtual void SeekingState() { }
    protected virtual void AttackingState() { }

    // ------------------------------ //
    //         SPAWN/DESPAWN
    // ------------------------------ //
    public virtual void OnSpawned(Transform playerRef)
    {
        player = playerRef;

        if (health != null)
            health.ResetHealth();

        rb.linearVelocity = Vector3.zero;
        lastMoveDirection = Vector3.zero;

        EnterIdleState();
    }

    public virtual void OnDespawned()
    {
        rb.linearVelocity = Vector3.zero;
        lastMoveDirection = Vector3.zero;

        player = null;

        currentState = EnemyState.Idle;
    }

    // ------------------------------ //
    //             TILT
    // ------------------------------ //
    private void ApplyTilt()
    {
        if (!useTilt) return;

        Vector3 horizontalVel = rb.linearVelocity;
        horizontalVel.y = 0f;

        if (horizontalVel.sqrMagnitude < 0.01f)
        {
            transform.localRotation = Quaternion.Lerp(
                transform.localRotation,
                Quaternion.identity,
                Time.deltaTime * 5f
            );
            return;
        }

        lastMoveDirection = horizontalVel.normalized;

        Quaternion targetTilt =
            Quaternion.Euler(
                lastMoveDirection.z * -tiltAmount,
                0,
                lastMoveDirection.x * tiltAmount
            );

        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            targetTilt,
            Time.deltaTime * 5f
        );
    }

    // ------------------------------ //
    //            DEATH
    // ------------------------------ //
    protected virtual void HandleDeath()
    {
        Die();
        GlobalEnemyPool.Instance.DespawnEnemy(gameObject);
    }

    // ------------------------------ //
    //          STATE HELPERS
    // ------------------------------ //
    protected void EnterIdleState() => currentState = EnemyState.Idle;
    protected void EnterWanderingState() => currentState = EnemyState.Wandering;
    protected void EnterSeekingState() => currentState = EnemyState.Seeking;
    protected void EnterAttackingState() => currentState = EnemyState.Attacking;

    // ------------------------------ //
    //          MOVEMENT HELPERS
    // ------------------------------ //
    protected void MoveTowards(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position).normalized;

        rb.MovePosition(transform.position + dir * moveSpeed * Time.deltaTime);

        // Rotate to face direction
        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                lookRot,
                Time.deltaTime * turnSpeed
            );
        }
    }

    protected float DistanceToPlayer()
    {
        if (player == null) return Mathf.Infinity;
        return Vector3.Distance(transform.position, player.position);
    }

    public void Die()
    {
        owningCamp?.NotifyEnemyDied(this);
        gameObject.SetActive(false);
    }
}
