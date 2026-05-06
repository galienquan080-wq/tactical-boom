using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Hệ thống gỡ bom với wave-based AI defense
/// </summary>
public class BombDefuseSystem : MonoBehaviour
{
    [System.Serializable]
    public class BombSite
    {
        public Vector3 position;
        public float defuseRadius = 3f;
        public float baseBombTimer = 45f;  // Bom phát nổ trong 45 giây
    }

    [System.Serializable]
    public class DefuseConfig
    {
        public float baseDefuseTime = 5f;   // 5 giây gỡ bom mặc định
        public float defuseInterruptDistance = 1f;
    }

    private BombSite bombSite;
    private DefuseConfig defuseConfig;
    
    private bool isDefusing = false;
    private float defuseProgress = 0f;
    private float defuseTimeRequired = 0f;
    private float defuseTimer = 0f;

    private float bombTimer = 0f;
    private bool bombActive = false;

    private PlayerHealth playerHealth;
    private PlayerController playerController;
    private AIWaveManager waveManager;

    public event Action<float> OnDefuseProgressChanged; // 0-1
    public event Action OnDefuseComplete;
    public event Action OnDefuseInterrupted;
    public event Action<float> OnBombTimerTick;     // Countdown
    public event Action OnBombExploded;

    void Start()
    {
        bombSite = new BombSite { position = transform.position };
        defuseConfig = new DefuseConfig();

        playerHealth = FindObjectOfType<PlayerHealth>();
        playerController = FindObjectOfType<PlayerController>();
        waveManager = FindObjectOfType<AIWaveManager>();
    }

    void Update()
    {
        if (!bombActive) return;

        // Cập nhật bomb timer
        bombTimer -= Time.deltaTime;
        OnBombTimerTick?.Invoke(bombTimer);

        if (bombTimer <= 0)
        {
            ExplodeBomb();
            return;
        }

        // Kiểm tra defuse
        UpdateDefuse();
    }

    /// <summary>
    /// Bắt đầu quy trình gỡ bom
    /// </summary>
    public void StartDefuse()
    {
        if (!playerHealth.IsAlive()) return;

        // Kiểm tra khoảng cách
        float distanceToBomb = Vector3.Distance(playerController.transform.position, bombSite.position);
        if (distanceToBomb > bombSite.defuseRadius)
        {
            Debug.Log("[BOMB] Out of defuse range!");
            return;
        }

        isDefusing = true;
        
        // Tính thời gian gỡ bom (có thể bị ảnh hưởng bởi thương chân)
        defuseTimeRequired = defuseConfig.baseDefuseTime * playerHealth.GetDefuseTimeMultiplier();
        
        // Kích hoạt Wave-based AI Defense
        if (waveManager != null)
        {
            waveManager.StartWaveDefense();
        }

        Debug.Log($"[BOMB] Defuse started! Time required: {defuseTimeRequired}s");
    }

    void UpdateDefuse()
    {
        if (!isDefusing || playerHealth == null || !playerHealth.IsAlive())
        {
            InterruptDefuse();
            return;
        }

        // Kiểm tra khoảng cách
        float distanceToBomb = Vector3.Distance(playerController.transform.position, bombSite.position);
        if (distanceToBomb > bombSite.defuseRadius + defuseConfig.defuseInterruptDistance)
        {
            InterruptDefuse();
            return;
        }

        // Cập nhật tiến độ gỡ bom
        defuseTimer += Time.deltaTime;
        defuseProgress = Mathf.Clamp01(defuseTimer / defuseTimeRequired);
        OnDefuseProgressChanged?.Invoke(defuseProgress);

        // Hoàn thành gỡ bom
        if (defuseProgress >= 1f)
        {
            CompleteDefuse();
        }
    }

    public void InterruptDefuse()
    {
        if (!isDefusing) return;

        isDefusing = false;
        defuseTimer = 0f;
        defuseProgress = 0f;
        OnDefuseInterrupted?.Invoke();
        Debug.Log("[BOMB] Defuse interrupted!");
    }

    public void CompleteDefuse()
    {
        isDefusing = false;
        bombActive = false;
        OnDefuseComplete?.Invoke();
        
        // Win condition
        Debug.Log("[BOMB] Defuse successful! MISSION COMPLETE");
    }

    public void ExplodeBomb()
    {
        bombActive = false;
        OnBombExploded?.Invoke();
        
        // Tất cả Player trong radius bị damage
        Collider[] playersInRange = Physics.OverlapSphere(bombSite.position, 30f);
        foreach (Collider col in playersInRange)
        {
            PlayerHealth ph = col.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(150);
            }
        }

        Debug.Log("[BOMB] BOMB EXPLODED! Mission failed!");
    }

    /// <summary>
    /// Bắt đầu hệ thống bomb (từ MissionManager)
    /// </summary>
    public void InitializeBomb()
    {
        bombActive = true;
        bombTimer = bombSite.baseBombTimer;
        Debug.Log($"[BOMB] Bomb planted! Explodes in {bombSite.baseBombTimer}s");
    }

    // Getters
    public bool IsDefusing() => isDefusing;
    public float GetDefuseProgress() => defuseProgress;
    public float GetBombTimer() => bombTimer;
    public bool IsBombActive() => bombActive;
    public Vector3 GetBombPosition() => bombSite.position;
}
