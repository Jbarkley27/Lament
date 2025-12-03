using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    [Header("Tilt Sensitivity")]
    public float tiltSensitivityX = 1f;  // how strong X tilt reacts to velocity
    public float tiltSensitivityY = 1f;  // how strong Y tilt reacts to velocity

    [Header("Tilt Limits")]
    [Range(0, 1)] public float maxTiltX = 1f; // -1 to 1 range for BlendTree
    [Range(0, 1)] public float maxTiltY = 1f;

    [Header("Blend Transition Speed")]
    public float tiltDampSpeed = 0.1f; // lower = smoother, higher = snappier

    [Header("Idle Deadzone")]
    [Tooltip("Minimum planar speed before the blend tree moves off idle.")]
    public float movementThreshold = 0.05f;

    [Header("Attack Pulse")]
    public float attackExpandMultiplier = 1.1f;
    public float attackShrinkMultiplier = 0.9f;
    public float attackExpandDuration = 0.25f;
    public float attackPauseDuration = 0.3f;
    public float attackShrinkDuration = 0.15f;
    public float attackReturnDelay = 0.1f;
    public float attackReturnDuration = 0.2f;
    public Ease attackExpandEase = Ease.OutBack;
    public Ease attackShrinkEase = Ease.InOutSine;
    public Ease attackReturnEase = Ease.OutSine;

    public Rigidbody rb;
    private Animator anim;

    private readonly int xRotHash = Animator.StringToHash("X_Rot");
    private readonly int yRotHash = Animator.StringToHash("Y_Rot");
    public bool debugUseDesiredDir = false;

    public EnemyBase enemyBase;
    private Sequence attackTween;
    private Vector3 baseScale;

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = rb ?? GetComponent<Rigidbody>();
        enemyBase = enemyBase ?? GetComponent<EnemyBase>();
        baseScale = transform.localScale;
    }

    void OnDisable()
    {
        if (attackTween != null && attackTween.IsActive())
            attackTween.Kill();
        transform.localScale = baseScale;
        attackTween = null;
    }

    void Update()
    {
        if (rb == null) return;

        Vector3 vel = rb.linearVelocity;
        Vector3 localVel = debugUseDesiredDir && enemyBase != null ? enemyBase.desiredDir : vel;

        // Convert world velocity to local movement
        localVel = transform.InverseTransformDirection(localVel);

        // Scale by sensitivity
        float rawX = localVel.x * tiltSensitivityX;
        float rawY = localVel.z * tiltSensitivityY;

        // Keep idle when movement is below the threshold to avoid small jitters leaving idle
        float planarSpeedSqr = rb.linearVelocity.sqrMagnitude;
        if (planarSpeedSqr < movementThreshold
            || enemyBase.currentState == EnemyBase.EnemyState.Engaging
            || enemyBase.currentState == EnemyBase.EnemyState.Seeking)
        {
            anim.SetFloat(xRotHash, 0f, tiltDampSpeed, Time.deltaTime);
            anim.SetFloat(yRotHash, 0f, tiltDampSpeed, Time.deltaTime);
            return;
        }

        // Clamp by max tilt per axis
        float xTilt = Mathf.Clamp(rawX, -maxTiltX, maxTiltX);
        float yTilt = Mathf.Clamp(rawY, -maxTiltY, maxTiltY);

        // Smooth blend
        anim.SetFloat(xRotHash, xTilt, tiltDampSpeed, Time.deltaTime);
        anim.SetFloat(yRotHash, yTilt, tiltDampSpeed, Time.deltaTime);
    }

    public IEnumerator PlayAttackPulse(Action onFire)
    {
        if (attackTween != null && attackTween.IsActive())
            attackTween.Kill();

        baseScale = transform.localScale;
        Vector3 originalScale = baseScale;

        attackTween = DOTween.Sequence();
        attackTween.Append(transform.DOScale(originalScale * attackExpandMultiplier, attackExpandDuration).SetEase(attackExpandEase));
        attackTween.AppendInterval(attackPauseDuration);
        attackTween.AppendCallback(() => onFire?.Invoke());
        attackTween.Append(transform.DOScale(originalScale * attackShrinkMultiplier, attackShrinkDuration).SetEase(attackShrinkEase));
        attackTween.AppendInterval(Random.Range(attackReturnDelay * 0.8f, attackReturnDelay * 1.2f));
        attackTween.Append(transform.DOScale(originalScale, attackReturnDuration).SetEase(attackReturnEase));
        attackTween.OnComplete(() => attackTween = null);
        attackTween.OnKill(() => transform.localScale = originalScale);

        yield return attackTween.WaitForCompletion();
    }
}
