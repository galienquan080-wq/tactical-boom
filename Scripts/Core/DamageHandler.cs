using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Hệ thống xử lý sát thương dựa trên Hitbox (vị trí trúng đạn)
/// </summary>
public class DamageHandler : MonoBehaviour
{
    [System.Serializable]
    public enum HitboxType
    {
        HeadTop,      // Nửa trên đầu (1 shot = tử vong)
        HeadBottom,   // Nửa dưới đầu (2 shots = tử vong)
        CenterMass,   // Trung tâm ngực (1 shot = tử vong)
        LeftArm,      // Tay trái
        RightArm,     // Tay phải
        LeftLeg,      // Chân trái
        RightLeg,     // Chân phải
        Torso         // Thân mình (áo giáp)
    }

    [System.Serializable]
    public class HitboxData
    {
        public HitboxType type;
        public Collider collider;
        public float damageMultiplier;
        public bool isCritical;
    }

    [System.Serializable]
    public class DamageProfile
    {
        public HitboxType hitboxType;
        public int damageValue;
        public float ragdollForce;
        public string effectName; // Particle effect khi trúng
    }

    private Dictionary<HitboxType, DamageProfile> damageTable;
    private PlayerHealth playerHealth;
    private List<HitboxData> hitboxes;

    void Start()
    {
        InitializeDamageTable();
        CollectHitboxes();
        playerHealth = GetComponent<PlayerHealth>();
    }

    /// <summary>
    /// Khởi tạo bảng dữ liệu sát thương cho từng vị trí
    /// </summary>
    void InitializeDamageTable()
    {
        damageTable = new Dictionary<HitboxType, DamageProfile>
        {
            { HitboxType.HeadTop, new DamageProfile 
            { 
                hitboxType = HitboxType.HeadTop,
                damageValue = 9999, // Instant Kill
                ragdollForce = 500f,
                effectName = "headshot_explosion"
            }},
            { HitboxType.HeadBottom, new DamageProfile 
            { 
                hitboxType = HitboxType.HeadBottom,
                damageValue = 100, // 2 shots = death (50 each)
                ragdollForce = 300f,
                effectName = "headshot_blood"
            }},
            { HitboxType.CenterMass, new DamageProfile 
            { 
                hitboxType = HitboxType.CenterMass,
                damageValue = 9999, // Instant Kill
                ragdollForce = 250f,
                effectName = "chest_impact"
            }},
            { HitboxType.LeftArm, new DamageProfile 
            { 
                hitboxType = HitboxType.LeftArm,
                damageValue = 30,
                ragdollForce = 100f,
                effectName = "arm_hit"
            }},
            { HitboxType.RightArm, new DamageProfile 
            { 
                hitboxType = HitboxType.RightArm,
                damageValue = 30,
                ragdollForce = 100f,
                effectName = "arm_hit"
            }},
            { HitboxType.LeftLeg, new DamageProfile 
            { 
                hitboxType = HitboxType.LeftLeg,
                damageValue = 35,
                ragdollForce = 150f,
                effectName = "leg_hit"
            }},
            { HitboxType.RightLeg, new DamageProfile 
            { 
                hitboxType = HitboxType.RightLeg,
                damageValue = 35,
                ragdollForce = 150f,
                effectName = "leg_hit"
            }},
            { HitboxType.Torso, new DamageProfile 
            { 
                hitboxType = HitboxType.Torso,
                damageValue = 50,
                ragdollForce = 120f,
                effectName = "torso_hit"
            }}
        };
    }

    /// <summary>
    /// Thu thập tất cả Hitbox từ nhân vật
    /// </summary>
    void CollectHitboxes()
    {
        hitboxes = new List<HitboxData>();
        Collider[] allColliders = GetComponentsInChildren<Collider>();

        foreach (Collider col in allColliders)
        {
            string boneName = col.gameObject.name.ToLower();
            HitboxType type = DetermineHitboxType(boneName);
            
            hitboxes.Add(new HitboxData
            {
                type = type,
                collider = col,
                damageMultiplier = GetDamageMultiplier(type),
                isCritical = (type == HitboxType.HeadTop || type == HitboxType.CenterMass)
            });
        }
    }

    /// <summary>
    /// Xác định loại Hitbox dựa trên tên bone
    /// </summary>
    HitboxType DetermineHitboxType(string boneName)
    {
        if (boneName.Contains("head_top") || boneName.Contains("brain"))
            return HitboxType.HeadTop;
        if (boneName.Contains("head_bottom") || boneName.Contains("jaw"))
            return HitboxType.HeadBottom;
        if (boneName.Contains("chest") || boneName.Contains("torso"))
            return HitboxType.CenterMass;
        if (boneName.Contains("arm_left") || boneName.Contains("shoulder_l"))
            return HitboxType.LeftArm;
        if (boneName.Contains("arm_right") || boneName.Contains("shoulder_r"))
            return HitboxType.RightArm;
        if (boneName.Contains("leg_left") || boneName.Contains("thigh_l"))
            return HitboxType.LeftLeg;
        if (boneName.Contains("leg_right") || boneName.Contains("thigh_r"))
            return HitboxType.RightLeg;
        
        return HitboxType.Torso; // Default
    }

    float GetDamageMultiplier(HitboxType type)
    {
        return type switch
        {
            HitboxType.HeadTop => 5.0f,      // 5x multiplier
            HitboxType.HeadBottom => 2.0f,   // 2x multiplier
            HitboxType.CenterMass => 3.0f,   // 3x multiplier
            HitboxType.Torso => 1.0f,        // Normal
            _ => 0.8f                         // Arms & Legs
        };
    }

    /// <summary>
    /// Hàm chính tính toán sát thương
    /// hitPoint: Vị trí bullet hit từ world space
    /// </summary>
    public DamageResult CalculateDamage(Vector3 hitPoint, Collider hitCollider, int baseBulletDamage = 50)
    {
        DamageResult result = new DamageResult();

        // Tìm Hitbox nào bị trúng
        HitboxData hitBox = FindHitbox(hitCollider);
        if (hitBox == null)
        {
            Debug.LogWarning("Hitbox not found!");
            return result;
        }

        // Lấy thông tin sát thương từ bảng
        if (!damageTable.TryGetValue(hitBox.type, out DamageProfile damageProfile))
        {
            return result;
        }

        // Tính toán sát thương cuối cùng
        int finalDamage = CalculateFinalDamage(
            baseBulletDamage, 
            hitBox.damageMultiplier, 
            damageProfile
        );

        result.damage = finalDamage;
        result.hitboxType = hitBox.type;
        result.isCritical = hitBox.isCritical;
        result.hitPoint = hitPoint;
        result.effectName = damageProfile.effectName;
        result.ragdollForce = damageProfile.ragdollForce;

        // Kiểm tra instant kill
        if (hitBox.type == HitboxType.HeadTop || hitBox.type == HitboxType.CenterMass)
        {
            result.isInstantKill = true;
        }

        // Debug
        Debug.Log($"[DAMAGE] Hit: {hitBox.type} | Damage: {finalDamage} | Critical: {hitBox.isCritical}");

        return result;
    }

    HitboxData FindHitbox(Collider hitCollider)
    {
        foreach (HitboxData hitbox in hitboxes)
        {
            if (hitbox.collider == hitCollider)
                return hitbox;
        }
        return null;
    }

    int CalculateFinalDamage(int baseDamage, float multiplier, DamageProfile profile)
    {
        int calculatedDamage = (int)(baseDamage * multiplier);
        return Mathf.Max(calculatedDamage, profile.damageValue);
    }

    /// <summary>
    /// Xử lý sát thương cho Player
    /// </summary>
    public void ApplyDamage(DamageResult damageResult)
    {
        if (playerHealth == null) return;

        // Nếu là instant kill
        if (damageResult.isInstantKill)
        {
            playerHealth.Die(damageResult);
            return;
        }

        // Xử lý trúng tay: Tăng Recoil
        if (damageResult.hitboxType == HitboxType.LeftArm || 
            damageResult.hitboxType == HitboxType.RightArm)
        {
            playerHealth.ApplyArmDamage(damageResult.hitboxType);
        }

        // Xử lý trúng chân: Giảm tốc độ hoặc Crawl
        if (damageResult.hitboxType == HitboxType.LeftLeg || 
            damageResult.hitboxType == HitboxType.RightLeg)
        {
            playerHealth.ApplyLegDamage(damageResult.hitboxType, damageResult.damage);
        }

        playerHealth.TakeDamage(damageResult.damage);
    }

    public struct DamageResult
    {
        public int damage;
        public HitboxType hitboxType;
        public bool isCritical;
        public bool isInstantKill;
        public Vector3 hitPoint;
        public string effectName;
        public float ragdollForce;
    }
}
