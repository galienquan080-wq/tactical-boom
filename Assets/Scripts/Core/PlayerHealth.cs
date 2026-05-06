using UnityEngine;
using System;

/// <summary>
/// Quản lý sức khỏe và các trạng thái thương (Arm/Leg damage)
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [System.Serializable]
    public enum InjuryState
    {
        Healthy,
        LeftArmInjured,
        RightArmInjured,
        LeftLegInjured,
        RightLegInjured,
        BothLegsInjured,  // Crawl state
        Dead
    }

    [SerializeField] private int maxHealth = 200;
    private int currentHealth;
    
    private InjuryState currentInjuryState = InjuryState.Healthy;
    private int leftArmHits = 0;
    private int rightArmHits = 0;
    private int leftLegHits = 0;
    private int rightLegHits = 0;

    private float recoilMultiplier = 1.0f;
    private float defuseTimeMultiplier = 1.0f;
    private float moveSpeedMultiplier = 1.0f;

    // Events
    public event Action<int> OnHealthChanged;
    public event Action<InjuryState> OnInjuryStateChanged;
    public event Action OnPlayerDeath;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Xử lý thương tay: Tăng độ giật (Recoil)
    /// </summary>
    public void ApplyArmDamage(DamageHandler.HitboxType armType)
    {
        if (armType == DamageHandler.HitboxType.LeftArm)
        {
            leftArmHits++;
            SetInjuryState(InjuryState.LeftArmInjured);
        }
        else if (armType == DamageHandler.HitboxType.RightArm)
        {
            rightArmHits++;
            SetInjuryState(InjuryState.RightArmInjured);
        }

        // Tăng Recoil multiplier dựa vào số lần bị trúng
        recoilMultiplier = 1.0f + (leftArmHits + rightArmHits) * 0.3f;
        Debug.Log($"[ARM INJURY] Recoil Multiplier: {recoilMultiplier}x");
    }

    /// <summary>
    /// Xử lý thương chân: Giảm tốc độ hoặc chuyển sang Crawl
    /// </summary>
    public void ApplyLegDamage(DamageHandler.HitboxType legType, int damage)
    {
        if (legType == DamageHandler.HitboxType.LeftLeg)
        {
            leftLegHits++;
            if (rightLegHits > 0) // Cả 2 chân bị thương
            {
                SetInjuryState(InjuryState.BothLegsInjured);
                moveSpeedMultiplier = 0.25f; // Crawl state = 25% tốc độ
                defuseTimeMultiplier = 1.5f; // Gỡ bom chậm hơn
                Debug.Log("[LEG INJURY] Both legs injured - CRAWL MODE ACTIVATED");
            }
            else
            {
                SetInjuryState(InjuryState.LeftLegInjured);
                moveSpeedMultiplier = 0.7f; // Giảm 30% tốc độ
            }
        }
        else if (legType == DamageHandler.HitboxType.RightLeg)
        {
            rightLegHits++;
            if (leftLegHits > 0) // Cả 2 chân bị thương
            {
                SetInjuryState(InjuryState.BothLegsInjured);
                moveSpeedMultiplier = 0.25f; // Crawl state
                defuseTimeMultiplier = 1.5f;
                Debug.Log("[LEG INJURY] Both legs injured - CRAWL MODE ACTIVATED");
            }
            else
            {
                SetInjuryState(InjuryState.RightLegInjured);
                moveSpeedMultiplier = 0.7f;
            }
        }
    }

    void SetInjuryState(InjuryState newState)
    {
        if (currentInjuryState != newState)
        {
            currentInjuryState = newState;
            OnInjuryStateChanged?.Invoke(newState);
        }
    }

    public void Die(DamageHandler.DamageResult? damageResult = null)
    {
        SetInjuryState(InjuryState.Dead);
        currentHealth = 0;

        // Trigger ragdoll hoặc death animation
        if (damageResult.HasValue)
        {
            ApplyRagdoll(damageResult.Value);
        }

        OnPlayerDeath?.Invoke();
        Debug.Log("[PLAYER] Player has been eliminated!");
    }

    void ApplyRagdoll(DamageHandler.DamageResult result)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 forceDirection = (transform.position - result.hitPoint).normalized;
            rb.AddForce(forceDirection * result.ragdollForce, ForceMode.Impulse);
        }
    }

    // Getters
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public InjuryState GetInjuryState() => currentInjuryState;
    public float GetRecoilMultiplier() => recoilMultiplier;
    public float GetDefuseTimeMultiplier() => defuseTimeMultiplier;
    public float GetMoveSpeedMultiplier() => moveSpeedMultiplier;
    public bool IsCrawling() => currentInjuryState == InjuryState.BothLegsInjured;
    public bool IsAlive() => currentHealth > 0 && currentInjuryState != InjuryState.Dead;
}
