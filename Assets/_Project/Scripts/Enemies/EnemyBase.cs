using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class EnemyBase : MonoBehaviour
{
    
    [Header("State Machine")]
    public EnemyState currentState;
    public enum EnemyState
    {
        Wandering,
        Seeking,
        Engaging,
        Attacking,
        AttackCooldown
    }



    [Header("General")]
    protected Rigidbody rb;
    protected EnemyHealthModule health;
    public EnemyCamp owningCamp;
    protected Transform player;
    public EnemyType enemyType;
    public bool debugMode = false;
    public EnemyAnimator enemyAnimator;


    [Header("Wander Settings")]
    private Vector3 currentWanderTarget;
    private Coroutine wanderNodeCoroutine;
    public float newWanderNodeIntervalMin = 3f;
    public float newWanderNodeIntervalMax = 6f;



    [Header("Seek Settings")]
    public float minNewPlayerScentNode = 10f;
    public float maxNewPlayerScentNode = 20f;
    Coroutine seekNodeCoroutine;



    [Header("Movement Settings")]
    private float moveSpeed = 1f;
    public float averageMoveSpeed = 3f;
    // private float steerSpeed = 1f;
    // public float averageSteerSpeed = 10f;
    private float lookSpeed = 5f;
    public float averageLookSpeed = 5f;
    // private float acceleration = 1f;
    // public float averageAcceleration = 12f;
    public float avoidanceRadius = 1.5f;
    private float avoidanceCastDistance = 80f;
    public float averageAvoidanceCastDistance = 80f;
    public float lookaheadTime = 0.2f;
    private Vector3 lookDir = Vector3.forward;
    private Vector3 playerScentNode;
    public bool CanSeePlayerFlag = false;
    private Vector3 desiredDirection;
    // private Vector3 currentVelocity;


    [Header("Attack Settings")]
    public float attackRange = 5f;
    public float averageAttackRange = 5f;
    Coroutine attackCoroutine;
    public bool isAttacking = false;
    public float attackCastTime = 1f;
    public float attackCooldownTime = 7f;
    public bool CanAttack = true;
    [Header("Attack Recoil")]
    public float attackRecoilForce = 6f;
    [Tooltip("Failsafe: force-finish attack sequence if it takes longer than this.")]
    public float attackMaxDuration = 3f;

    [Header("Projectile Settings")]
    public ProjectileEmitter projectileEmitter;
    public ProjectilePattern singleShotPattern = new ProjectilePattern
    {
        patternType = ProjectilePatternType.Single,
        count = 1,
        arcAngle = 0f,
        speed = 30f,
        lifetime = 5f,
        damage = 10
    };
    public ProjectilePattern rowShotPattern = new ProjectilePattern
    {
        patternType = ProjectilePatternType.Row,
        count = 5,
        arcAngle = 30f,
        speed = 30f,
        lifetime = 5f,
        damage = 8
    };
    public AttackProjectileMode projectileMode = AttackProjectileMode.Single;







    [Header("Avoidance Settings")]
    public float coneAngle = 120f;  // width of forward cone
    public int raysPerCone = 7;     // number of rays in the cone
    public List<AvoidanceLayer> avoidanceLayers = new List<AvoidanceLayer>();
    // [SerializeField] private float escapeDuration = 0.5f; // how long to force escape
    // private float escapeTimer = 0f;
    // private bool forcingEscape = false;
    // [SerializeField] private float stuckSpeedThreshold = 0.15f;   // velocity below this = not moving
    // public bool IsStuck;
    private float linearVelMag;

    [Header("Backward Avoidance Settings")]
    public float backConeAngle = 120f;
    public int backRaysPerCone = 5;
    public float backAvoidanceCastDistance = 60f;
    public float backAvoidanceRadius = 1.5f;
    public float backLookaheadTime = 0.2f;



    // [Header("Escape Settings")]
    // public LayerMask escapeBlockLayers;
    // public float escapeForce;
    // public int escapeRaysPerCone = 12;















    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        health = GetComponent<EnemyHealthModule>();

        if (debugMode && health != null)
            health.InitializeUI();
    }






    protected virtual void Update()
    {
        RunStateMachine();
        player = owningCamp.Player;

        linearVelMag = rb.linearVelocity.sqrMagnitude;

        if (currentWanderTarget != Vector3.zero && currentState == EnemyState.Wandering)
        {
            Debug.DrawLine(transform.position, currentWanderTarget, Color.cyan);
        }

        if (playerScentNode != Vector3.zero && currentState != EnemyState.Wandering)
        {
            Debug.DrawLine(transform.position, playerScentNode, Color.magenta);
        }
    }






    protected virtual void FixedUpdate()
    {
        // UpdateStuckDetection();
        MoveRigidbody();
    }









    // ------------------------------ //
    //         STATE MACHINE
    // ------------------------------ //
    private void RunStateMachine()
    {
        switch (currentState)
        {
            case EnemyState.Wandering:
                WanderingState();
                break;

            case EnemyState.Seeking:
                SeekingState();
                break;

            case EnemyState.Attacking:
                EngagingState();
                break;
            
            case EnemyState.Engaging:
                EngagingState();
                break;

            case EnemyState.AttackCooldown:
                EngagingState();
                break;
        }
    }








    protected virtual void WanderingState()
    {
        // Immediately switch to seeking if player exists which will be given by EnemyCamp
        if (player != null)
        {
            EnterSeekingState();
            if(wanderNodeCoroutine != null) StopCoroutine(wanderNodeCoroutine);
        }

        // Start the wander loop once
        if (wanderNodeCoroutine == null)
        {
            wanderNodeCoroutine = StartCoroutine(WanderLoop());
        }

        AssignLookAtPoint(currentWanderTarget, lookSpeed);
    }







    private IEnumerator WanderLoop()
    {
        while (currentState == EnemyState.Wandering)
        {
            // Pick new wander target
            PickNewWanderPoint();

            yield return new WaitForSeconds(Random.Range(newWanderNodeIntervalMin, newWanderNodeIntervalMax));
        }
    }





    private void PickNewWanderPoint()
    {
        if (owningCamp == null)
            return;

        currentWanderTarget = owningCamp.FindValidSpawnPosition(Random.Range(5f, 6f));

        Vector3 toTarget = currentWanderTarget - transform.position;
        toTarget.y = 0;

        SetDesiredDirection(toTarget.normalized);
    }



    // An enemy should be in seeking state only if the player exists and they are out of attack range
    // seeing player doesn't matter here

    protected virtual void SeekingState()
    {
        // Go wander state if we don't know where the player is 
        if (player == null)
        {
            EnterWanderingState();
            if (seekNodeCoroutine != null) StopCoroutine(seekNodeCoroutine);
            return;
        }


        if (player == null)
        {
            EnterSeekingState();
            return;
        }

        // Set look direction towards wander target
        AssignLookAtPoint(player.position, lookSpeed);


        if (PlayerInAttackRange()) // added CanSeePlayerFlag once tested
        {
            Logger.Log($"Enemy {name} entering Engaged State");
            EnterEngagedState();
            return;
        }


        // Start the wander loop once
        if (seekNodeCoroutine == null)
        {
            seekNodeCoroutine = StartCoroutine(SeekNodeLoop());
        }
    }




    private IEnumerator SeekNodeLoop()
    {
        while (true && currentState != EnemyState.Wandering)
        {
            // Pick new scent target
            playerScentNode = GlobalEnemyPool.Instance.PlayerScentNode.GetValidPlayerScentNode(
                attackRange
            );

            Vector3 toTarget = playerScentNode - transform.position;
            toTarget.y = 0;

            SetDesiredDirection(toTarget.normalized);

            // Wait for some time
            yield return new WaitForSeconds(Random.Range(minNewPlayerScentNode, maxNewPlayerScentNode));
        }
    }


    
    protected virtual void EngagingState()
    {
        // Set look direction towards wander target
        AssignLookAtPoint(player.position, lookSpeed);


        if (isAttacking || !CanAttack) return; // prevent state change during attack, Attack State still calls this function



        if (!PlayerInAttackRange()) // added CanSeePlayerFlag once tested
        {
            Logger.Log($"Enemy {name} exiting Engaged State");
            EnterSeekingState();
            return;
        }


        Logger.Log($"Enemy {name} coroutine is null: {attackCoroutine == null} isAttacking: {isAttacking} PlayerInAttackRange: {PlayerInAttackRange()} CanAttack: {CanAttack} CanSeePlayer: {CanSeePlayer()}");
        if (attackCoroutine == null && !isAttacking && PlayerInAttackRange() && CanAttack && CanSeePlayer())
        {
            Logger.Log($"Enemy {name} starting attack");
            attackCoroutine = StartCoroutine(InitiateAttack());
        }
    }


    public IEnumerator InitiateAttack()
    {
        EnterAttackingState();
        isAttacking = true;
        CanAttack = false;

        yield return PlayAttackSequenceWithTimeout();

        EnterAttackCooldownState();
        isAttacking = false;
        yield return new WaitForSeconds(attackCooldownTime);
        CanAttack = true;
        EnterSeekingState(); // returning to seek instead of engage to ensure proper state flow
        attackCoroutine = null;
    }


    public void Attack()
    {
        FireProjectile();
        ApplyAttackRecoil();
    }

    private IEnumerator PlayAttackSequenceWithTimeout()
    {
        float elapsed = 0f;

        if (enemyAnimator != null)
        {
            IEnumerator pulse = enemyAnimator.PlayAttackPulse(() => Attack());
            while (true)
            {
                bool hasNext;
                try
                {
                    hasNext = pulse.MoveNext();
                }
                catch (Exception e)
                {
                    Logger.Log($"Attack pulse exception on {name}: {e.Message}");
                    break;
                }

                if (!hasNext)
                    break;

                yield return pulse.Current;

                elapsed += Time.deltaTime;
                if (elapsed > attackMaxDuration)
                {
                    Logger.Log($"Attack pulse timeout on {name}; forcing completion.");
                    break;
                }
            }
        }
        else
        {
            Attack();
            yield return new WaitForSeconds(attackCastTime);
        }
    }

    private void FireProjectile()
    {
        if (projectileEmitter == null)
        {
            Logger.Log($"Enemy {name} has no projectile emitter assigned.");
            return;
        }

        ProjectilePattern pattern = projectileMode == AttackProjectileMode.Single
            ? singleShotPattern
            : rowShotPattern;

        projectileEmitter.StartCoroutine(projectileEmitter.Emit(pattern));
    }

    private void ApplyAttackRecoil()
    {
        if (rb == null) return;
        rb.linearVelocity = Vector3.zero; // optional if you want pure recoil
        rb.AddForce(-transform.forward * Random.Range(attackRecoilForce * .95f, 1.1f), ForceMode.VelocityChange);
    }


    public bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (Physics.Raycast(transform.position, dirToPlayer, out RaycastHit hit, distanceToPlayer))
        {
           if (hit.transform.CompareTag("PlayerVisual")
                || hit.transform.CompareTag("Player"))
            {
                return true; // Obstacle is the player
            }
            else
            {
                return false; // Obstacle in the way
            }
        }

        return false; // Clear line of sight
    }


    public bool PlayerInAttackRange()
    {
        if (player == null) return false;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        return distanceToPlayer <= attackRange;
    }



    // ------------------------------ //
    //       MOVEMENT & AVOIDANCE
    // ------------------------------ //
    public void SetDesiredDirection(Vector3 dir)
    {
        desiredDirection = dir.normalized;
    }





   // --- Tunable Physics Parameters ---
    [Header("Movement Tuning")]
    [SerializeField] float maxAccel = 25f;          // how strong your steering forces can be
    [SerializeField] float stoppingDistance = 0.8f; // hard brake zone
    [SerializeField] float slowDownRadius = 3f;     // where we start controlling speed
    [SerializeField] float stoppingSpeed = 7f;
    public Vector3 desiredDir;
    public float moveSpeedReducerFactor = .5f;

    // --- Debug ---
    [SerializeField] bool debug;

    private void MoveRigidbody()
    {
        Vector3 toTarget = GetSteerTarget() - transform.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;

        // Stop if close enough
        if (distance < stoppingDistance)
        {
            // Actively kill velocity so we don't drift
            rb.AddForce(-rb.linearVelocity * stoppingSpeed, ForceMode.Acceleration);
            return;
        }


        if (isAttacking) return;

        desiredDir = toTarget.normalized + ComputeAvoidanceForce(); 
        desiredDir.y = 0f;
        desiredDir.Normalize();

        float desiredSpeed = moveSpeed;

        // Smoothly scale down desired speed when getting close to the target
        if (distance < slowDownRadius)
            desiredSpeed *= distance / slowDownRadius; // linear slowdown

        Vector3 desiredVelocity = desiredDir * desiredSpeed;

        Vector3 steeringForce = desiredVelocity - rb.linearVelocity;

        // Limit acceleration to prevent slippery nonsense
        steeringForce = Vector3.ClampMagnitude(steeringForce, maxAccel);

        rb.AddForce(steeringForce, ForceMode.Acceleration);


        if (debug)
        {
            Debug.DrawRay(transform.position, rb.linearVelocity, Color.blue);     // current velocity
            Debug.DrawRay(transform.position, desiredVelocity, Color.yellow); // desired velocity
            Debug.DrawRay(transform.position, steeringForce, Color.red);      // steering force
        }
    }





    public Vector3 GetSteerTarget()
    {
        if (currentState == EnemyState.Wandering)
            return currentWanderTarget;
        else
            return playerScentNode;
    }




    private Vector3 ComputeAvoidanceForce()
    {
        Vector3 totalAvoidance = Vector3.zero;
        if (rb == null) return totalAvoidance;

        Dictionary<AvoidanceLayer, Vector3> layerForces = new Dictionary<AvoidanceLayer, Vector3>();
        foreach (var layer in avoidanceLayers)
            layerForces[layer] = Vector3.zero;

        // Forward avoidance rays
        AccumulateAvoidanceCone(layerForces,
                                coneAngle,
                                raysPerCone,
                                avoidanceCastDistance,
                                avoidanceRadius,
                                lookaheadTime,
                                transform.forward,
                                0.2f,
                                debugMode,
                                Color.green,
                                Color.red);

        // Backward avoidance rays (uses its own tuning)
        AccumulateAvoidanceCone(layerForces,
                                backConeAngle,
                                backRaysPerCone,
                                backAvoidanceCastDistance,
                                backAvoidanceRadius,
                                backLookaheadTime,
                                -transform.forward,
                                -0.2f,
                                debugMode,
                                Color.cyan,
                                Color.magenta);

        foreach (var layer in avoidanceLayers)
        {
            Vector3 f = layerForces[layer];
            if (f.magnitude > layer.maxForce)
                f = f.normalized * layer.maxForce;

            totalAvoidance += f;
        }

        return totalAvoidance;
    }

    private void AccumulateAvoidanceCone(Dictionary<AvoidanceLayer, Vector3> layerForces,
                                         float coneAngleDegrees,
                                         int rays,
                                         float castDistance,
                                         float radius,
                                         float velocityLookahead,
                                         Vector3 forwardDir,
                                         float forwardOffset,
                                         bool drawDebug,
                                         Color noHitColor,
                                         Color hitColor)
    {
        float halfCone = coneAngleDegrees / 2f;

        for (int i = 0; i < rays; i++)
        {
            float t = rays == 1 ? 0.5f : (float)i / (rays - 1);
            float angle = Mathf.Lerp(-halfCone, halfCone, t);
            Vector3 rayDir = Quaternion.Euler(0, angle, 0) * forwardDir;

            float weight = halfCone > 0f ? 1f - Mathf.Abs(angle) / halfCone : 1f;
            Vector3 rayOrigin = transform.position + rb.linearVelocity * velocityLookahead + forwardDir * forwardOffset;

            bool rayHitSomething = false;
            float nearestHitDistance = castDistance;

            foreach (var layer in avoidanceLayers)
            {
                if (Physics.SphereCast(rayOrigin,
                                       radius,
                                       rayDir,
                                       out RaycastHit hit,
                                       castDistance,
                                       layer.layerMask))
                {
                    if (hit.collider.attachedRigidbody == rb)
                        continue;

                    rayHitSomething = true;
                    nearestHitDistance = Mathf.Min(nearestHitDistance, hit.distance);

                    float distanceFactor = 1f - (hit.distance / castDistance);
                    float hitStrength = layer.strength * distanceFactor * weight;
                    hitStrength /= rb.mass;

                    Vector3 force = -rayDir * hitStrength;
                    layerForces[layer] += force;
                }
            }

            if (drawDebug)
            {
                if (rayHitSomething)
                    Debug.DrawRay(rayOrigin, rayDir * nearestHitDistance, hitColor);
                else
                    Debug.DrawRay(rayOrigin, rayDir * castDistance, noHitColor);
            }
        }
    }





    // ------------------------------ //
    //    STUCK DETECTION + ESCAPE
    // ------------------------------ //

    // private void UpdateStuckDetection()
    // {
    //     float speed = rb.linearVelocity.magnitude;

    //     // Detect if stuck based on velocity
    //     bool currentlyStuck = speed < stuckSpeedThreshold;

    //     if (currentlyStuck && !forcingEscape)
    //     {
    //         // Enter forced escape mode
    //         forcingEscape = true;
    //         escapeTimer = 0f;
    //     }

    //     if (forcingEscape)
    //     {
    //         // Count up the escape timer
    //         escapeTimer += Time.fixedDeltaTime;

    //         // Stop forcing escape after duration
    //         if (escapeTimer >= escapeDuration)
    //             forcingEscape = false;

    //         IsStuck = true; // keep applying escape logic
    //     }
    //     else
    //     {
    //         IsStuck = currentlyStuck;
    //     }


    // }







    // private Vector3 ComputeEscapeForce()
    // {
    //     Vector3 escapeVel = Vector3.zero;

    //     float halfCone = 360f;
    //     for (int i = 0; i < escapeRaysPerCone; i++)
    //     {
    //         float t = escapeRaysPerCone == 1 ? 0.5f : (float)i / (escapeRaysPerCone - 1);
    //         float angle = Mathf.Lerp(-halfCone, halfCone, t);
    //         Vector3 rayDir = Quaternion.Euler(0, angle, 0) * transform.forward;
    //         Vector3 rayOrigin = transform.position + rb.linearVelocity;
    //         rayOrigin += transform.forward * 0.2f;

    //         if (!Physics.SphereCast(rayOrigin,
    //                                 avoidanceRadius,
    //                                 rayDir,
    //                                 out RaycastHit hit,
    //                                 avoidanceCastDistance,
    //                                 escapeBlockLayers))
    //         {

    //             escapeVel = rayDir * escapeForce;

    //             if (debugMode)
    //                 Debug.DrawRay(rayOrigin, rayDir * 80f, Color.blue, .1f);

    //             break; // exit on first valid escape ray
    //         }
    //         else if (debugMode)
    //         {
    //             Debug.DrawRay(rayOrigin, rayDir * avoidanceCastDistance, Color.purple);
    //         }
            
    //     }


    //     return escapeVel;
    // }

    




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

        InvokeRepeating(nameof(RandomizeStats), 0f, Random.Range(5f, 10f));
        EnterWanderingState();

        // Other initialization as needed

        // account for player personal space
        attackRange = Random.Range(attackRange * 0.8f, attackRange * 1.2f) + 10f;
    }






    public virtual void OnDespawned()
    {
        if (health != null)
            health.OnDespawn();
    }






    // ------------------------------ //
    //             DEATH
    // ------------------------------ //
    public virtual void HandleDeath()
    {
        Die();
        GlobalEnemyPool.Instance.DespawnEnemy(gameObject);
    }






    // ------------------------------ //
    //          STATE HELPERS
    // ------------------------------ //
    protected void EnterWanderingState() => currentState = EnemyState.Wandering;
    protected void EnterSeekingState() => currentState = EnemyState.Seeking;
    protected void EnterEngagedState() => currentState = EnemyState.Engaging;
    protected void EnterAttackingState() => currentState = EnemyState.Attacking;
    protected void EnterAttackCooldownState() => currentState = EnemyState.AttackCooldown;

    public void Die()
    {
        GlobalDataStore.Instance.ExplosionManager.SpawnExplosion(
            ExplosionManager.ExplosionType.Basic_Enemy,
            transform.position
        );
        gameObject.SetActive(false);
    }









    // ------------------------------ //
    //           HELPERS
    // ------------------------------ //
    public void RandomizeStats()
    {
        moveSpeed = Random.Range(averageMoveSpeed * 0.8f, averageMoveSpeed * 1.2f);
        // steerSpeed = Random.Range(averageSteerSpeed * 0.8f, averageSteerSpeed * 1.2f);
        // acceleration = Random.Range(averageAcceleration * 0.8f, averageAcceleration * 1.2f);
        avoidanceCastDistance = Random.Range(averageAvoidanceCastDistance * 0.8f, averageAvoidanceCastDistance * 1.2f);
        lookSpeed = Random.Range(averageLookSpeed * 0.8f, averageLookSpeed * 1.2f);
    }



    public void AssignLookAtPoint(Vector3 point, float lookSpeed = 2f)
    {
        if (point == null || isAttacking) return;

        // Set look direction towards wander target
        lookDir = point - transform.position;
        lookDir.y = 0;

        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            rb.MoveRotation(
                Quaternion.Slerp(rb.rotation, targetRotation, lookSpeed * Time.fixedDeltaTime)
            );
        }
    }
}




[Serializable]
public struct AvoidanceLayer
{
    public LayerMask layerMask;
    public float strength;      // max strength per ray (before distance & weight)
    public float maxForce;      // max total per layer
}

public enum AttackProjectileMode
{
    Single,
    Row
}
