using UnityEngine;

public sealed class RtsEntityViewAnimator : MonoBehaviour
{
    public float IdleScaleAmplitude = 0.025f;
    public float IdleRotationDegrees = 1.5f;
    public float IdleSpeed = 2.5f;
    public float AttackKick = 0.12f;
    public float HitShakeDegrees = 7f;

    private Vector3 baseScale;
    private Quaternion baseRotation;
    private float attackTimer;
    private float hitTimer;
    private bool initialized;

    public void PlayAttack()
    {
        attackTimer = 0.16f;
    }

    public void PlayHit()
    {
        hitTimer = 0.18f;
    }

    private void Start()
    {
        CaptureBaseTransform();
    }

    private void Update()
    {
        if (!initialized)
        {
            CaptureBaseTransform();
        }

        attackTimer = Mathf.Max(0f, attackTimer - Time.deltaTime);
        hitTimer = Mathf.Max(0f, hitTimer - Time.deltaTime);

        float idleWave = Mathf.Sin(Time.time * IdleSpeed);
        float scale = 1f + idleWave * IdleScaleAmplitude;

        if (attackTimer > 0f)
        {
            scale += Mathf.Sin(attackTimer / 0.16f * Mathf.PI) * AttackKick;
        }

        float rotation = idleWave * IdleRotationDegrees;

        if (hitTimer > 0f)
        {
            rotation += Mathf.Sin(hitTimer * 120f) * HitShakeDegrees * (hitTimer / 0.18f);
        }

        transform.localScale = baseScale * scale;
        transform.localRotation = baseRotation * Quaternion.Euler(0f, 0f, rotation);
    }

    private void OnDisable()
    {
        if (!initialized)
        {
            return;
        }

        transform.localScale = baseScale;
        transform.localRotation = baseRotation;
    }

    private void CaptureBaseTransform()
    {
        baseScale = transform.localScale;
        baseRotation = transform.localRotation;
        initialized = true;
    }
}
