using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Hệ thống vũ khí chính với Recoil, ammo management
/// </summary>
public class WeaponSystem : MonoBehaviour
{
    [System.Serializable]
    public enum WeaponType
    {
        M4A1,
        AK47,
        G13
    }

    [System.Serializable]
    public class WeaponStats
    {
        public WeaponType type;
        public string weaponName;
        public int damage;
        public int maxAmmo;
        public int currentAmmo;
        public float fireRate;
        public float recoilX;      // Recoil trong frame (Y)
        public float recoilY;      // Recoil ngang (X)
        public float accuracy;
        public float weaponPrice;
    }

    [System.Serializable]
    public class Attachment
    {
        public string attachmentName;
        public float accuracyBonus;
        public float recoilReduction;
        public float fireRateBonus;
        public int price;
    }

    private WeaponStats currentWeapon;
    private Dictionary<WeaponType, WeaponStats> weaponDatabase;
    private List<Attachment> attachments = new List<Attachment>();
    
    private float recoilAccumulationX = 0f;
    private float recoilAccumulationY = 0f;
    private float lastShotTime = 0f;

    private DamageHandler damageHandler;
    private PlayerHealth playerHealth;

    public event Action<int> OnAmmoChanged;
    public event Action OnReload;

    void Start()
    {
        InitializeWeapons();
        damageHandler = GetComponent<DamageHandler>();
        playerHealth = GetComponent<PlayerHealth>();
        
        // Bắt đầu với M4A1
        EquipWeapon(WeaponType.M4A1);
    }

    /// <summary>
    /// Khởi tạo cơ sở dữ liệu vũ khí
    /// </summary>
    void InitializeWeapons()
    {
        weaponDatabase = new Dictionary<WeaponType, WeaponStats>
        {
            { WeaponType.M4A1, new WeaponStats
            {
                type = WeaponType.M4A1,
                weaponName = "M4A1 Carbine",
                damage = 50,
                maxAmmo = 120,
                currentAmmo = 120,
                fireRate = 0.1f,     // 10 shots/sec
                recoilX = 1.5f,      // Vertical recoil
                recoilY = 0.8f,      // Horizontal recoil
                accuracy = 0.85f,
                weaponPrice = 0
            }},
            { WeaponType.AK47, new WeaponStats
            {
                type = WeaponType.AK47,
                weaponName = "AK-47",
                damage = 60,
                maxAmmo = 120,
                currentAmmo = 120,
                fireRate = 0.15f,    // ~6.7 shots/sec
                recoilX = 2.5f,      // More recoil
                recoilY = 1.5f,
                accuracy = 0.75f,    // Less accurate
                weaponPrice = 2500
            }},
            { WeaponType.G13, new WeaponStats
            {
                type = WeaponType.G13,
                weaponName = "Glock 18",
                damage = 30,
                maxAmmo = 60,
                currentAmmo = 60,
                fireRate = 0.08f,    // Fast pistol
                recoilX = 0.8f,
                recoilY = 0.4f,
                accuracy = 0.6f,
                weaponPrice = 500
            }}
        };
    }

    /// <summary>
    /// Trang bị vũ khí
    /// </summary>
    public void EquipWeapon(WeaponType weaponType)
    {
        currentWeapon = weaponDatabase[weaponType];
        recoilAccumulationX = 0f;
        recoilAccumulationY = 0f;
        Debug.Log($"[WEAPON] Equipped: {currentWeapon.weaponName}");
    }

    /// <summary>
    /// Bắn súng với tính toán Recoil
    /// </summary>
    public void Fire(Vector3 shootDirection)
    {
        // Kiểm tra fire rate
        if (Time.time - lastShotTime < currentWeapon.fireRate)
            return;

        // Kiểm tra ammo
        if (currentWeapon.currentAmmo <= 0)
        {
            Debug.Log("[WEAPON] Out of ammo!");
            return;
        }

        lastShotTime = Time.time;
        currentWeapon.currentAmmo--;
        OnAmmoChanged?.Invoke(currentWeapon.currentAmmo);

        // Tính Recoil
        Vector3 recoil = CalculateRecoil();
        
        // Áp dụng Recoil lên camera/crosshair
        ApplyRecoilToCamera(recoil);

        // Raycast để kiểm tra hit
        ProcessBullet(shootDirection, recoil);

        Debug.Log($"[WEAPON] {currentWeapon.weaponName} fired | Ammo: {currentWeapon.currentAmmo}");
    }

    /// <summary>
    /// Tính toán Recoil dựa vào vũ khí và trạng thái Player
    /// </summary>
    Vector3 CalculateRecoil()
    {
        float recoilX = currentWeapon.recoilX;
        float recoilY = currentWeapon.recoilY;

        // Nếu tay bị thương: tăng recoil
        if (playerHealth != null)
        {
            float armInjuryMultiplier = playerHealth.GetRecoilMultiplier();
            recoilX *= armInjuryMultiplier;
            recoilY *= armInjuryMultiplier;
        }

        // Thêm randomness
        recoilX += Random.Range(-0.2f, 0.2f);
        recoilY += Random.Range(-0.3f, 0.3f);

        // Accumulate recoil
        recoilAccumulationX += recoilX;
        recoilAccumulationY += recoilY;

        return new Vector3(recoilY, recoilAccumulationX, 0);
    }

    void ApplyRecoilToCamera(Vector3 recoil)
    {
        // Sẽ được xử lý bởi PlayerController
        // Rotate camera dựa vào recoil pattern
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transform.Rotate(recoil.y * 0.1f, recoil.x * 0.1f, 0);
        }
    }

    /// <summary>
    /// Xử lý bullet: Raycast và detect hit
    /// </summary>
    void ProcessBullet(Vector3 shootDirection, Vector3 recoil)
    {
        Vector3 firePoint = Camera.main.transform.position;
        Vector3 adjustedDirection = shootDirection + recoil * 0.01f;

        if (Physics.Raycast(firePoint, adjustedDirection, out RaycastHit hit, 1000f))
        {
            // Kiểm tra có phải AI/Enemy không
            EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
            
            if (enemyHealth != null)
            {
                DamageHandler.DamageResult damageResult = 
                    damageHandler.CalculateDamage(hit.point, hit.collider, currentWeapon.damage);
                
                enemyHealth.TakeDamage(damageResult);

                // VFX
                InstantiateImpactEffect(hit.point, damageResult.effectName);
            }
        }
    }

    void InstantiateImpactEffect(Vector3 position, string effectName)
    {
        // TODO: Load từ Resources/Effects/
        Debug.Log($"[VFX] Impact effect: {effectName} at {position}");
    }

    /// <summary>
    /// Reload vũ khí
    /// </summary>
    public void Reload()
    {
        currentWeapon.currentAmmo = currentWeapon.maxAmmo;
        OnAmmoChanged?.Invoke(currentWeapon.currentAmmo);
        OnReload?.Invoke();
        Debug.Log($"[WEAPON] {currentWeapon.weaponName} reloaded!");
    }

    /// <summary>
    /// Thêm phụ kiện vũ khí
    /// </summary>
    public void AttachAttachment(Attachment attachment)
    {
        attachments.Add(attachment);
        
        currentWeapon.accuracy += attachment.accuracyBonus;
        currentWeapon.recoilX *= (1f - attachment.recoilReduction);
        currentWeapon.recoilY *= (1f - attachment.recoilReduction);
        currentWeapon.fireRate *= (1f - attachment.fireRateBonus);

        Debug.Log($"[ATTACHMENT] Added: {attachment.attachmentName}");
    }

    // Getters
    public WeaponStats GetCurrentWeapon() => currentWeapon;
    public int GetCurrentAmmo() => currentWeapon.currentAmmo;
    public int GetMaxAmmo() => currentWeapon.maxAmmo;
    public List<Attachment> GetAttachments() => attachments;
}
