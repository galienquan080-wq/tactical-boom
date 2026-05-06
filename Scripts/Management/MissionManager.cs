using UnityEngine;
using System;

/// <summary>
/// Quản lý trạng thái mission, win/loss conditions, currency
/// </summary>
public class MissionManager : MonoBehaviour
{
    public enum MissionState
    {
        Preparation,      // Player lấy vũ khí
        DefusingPhase,    // Gỡ bom + AI attack
        MissionComplete,
        MissionFailed
    }

    private MissionState currentState = MissionState.Preparation;
    private BombDefuseSystem bombDefuse;
    private AIWaveManager waveManager;
    private PlayerHealth playerHealth;
    private CurrencySystem currencySystem;

    private int rewardAmount = 2500; // Tiền thưởng khi Win

    public event Action<MissionState> OnMissionStateChanged;
    public event Action OnMissionWin;
    public event Action OnMissionLose;

    void Start()
    {
        bombDefuse = FindObjectOfType<BombDefuseSystem>();
        waveManager = FindObjectOfType<AIWaveManager>();
        playerHealth = FindObjectOfType<PlayerHealth>();
        currencySystem = FindObjectOfType<CurrencySystem>();

        // Subscribe to events
        bombDefuse.OnDefuseComplete += HandleMissionWin;
        bombDefuse.OnBombExploded += HandleMissionLose;
        playerHealth.OnPlayerDeath += HandlePlayerDeath;
        waveManager.OnAllWavesCompleted += HandleAllWavesDefeated;
    }

    void Update()
    {
        if (currentState == MissionState.Preparation && Input.GetKeyDown(KeyCode.E))
        {
            StartDefusePhase();
        }
    }

    void StartDefusePhase()
    {
        SetMissionState(MissionState.DefusingPhase);
        bombDefuse.InitializeBomb();
        Debug.Log("[MISSION] Defuse phase started!");
    }

    void HandleMissionWin()
    {
        SetMissionState(MissionState.MissionComplete);
        currencySystem.AddCurrency(rewardAmount);
        OnMissionWin?.Invoke();
        Debug.Log($"[MISSION] MISSION WIN! Reward: ${rewardAmount}");
    }

    void HandleMissionLose()
    {
        SetMissionState(MissionState.MissionFailed);
        OnMissionLose?.Invoke();
        Debug.Log("[MISSION] MISSION LOST!");
    }

    void HandlePlayerDeath()
    {
        HandleMissionLose();
    }

    void HandleAllWavesDefeated()
    {
        // Optional: Có thể thêm bonus nếu tiêu diệt tất cả AI trước khi gỡ bom xong
        Debug.Log("[MISSION] All AI waves defeated!");
    }

    void SetMissionState(MissionState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            OnMissionStateChanged?.Invoke(newState);
        }
    }

    public MissionState GetCurrentState() => currentState;
}
