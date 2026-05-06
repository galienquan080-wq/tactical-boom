using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Quản lý 3-5 đợt tấn công AI (Wave-based defense)
/// </summary>
public class AIWaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public int waveNumber;
        public int enemyCount;
        public float spawnDelay;
        public Vector3[] spawnPoints;
    }

    [SerializeField] private List<Wave> waves = new List<Wave>();
    [SerializeField] private Vector3 bombPosition;

    private int currentWaveIndex = 0;
    private int totalEnemiesSpawned = 0;
    private int totalEnemiesDefeated = 0;
    private bool waveInProgress = false;

    private List<EnemyController> activeEnemies = new List<EnemyController>();

    public event Action<int> OnWaveStarted;
    public event Action<int> OnWaveCompleted;
    public event Action OnAllWavesCompleted;

    void Start()
    {
        InitializeWaves();
    }

    void InitializeWaves()
    {
        waves = new List<Wave>();

        // Wave 1: 3 enemies
        waves.Add(new Wave
        {
            waveNumber = 1,
            enemyCount = 3,
            spawnDelay = 2f,
            spawnPoints = new[] 
            { 
                new Vector3(-10, 1, 10), 
                new Vector3(-15, 1, 8), 
                new Vector3(-12, 1, 15) 
            }
        });

        // Wave 2: 4 enemies
        waves.Add(new Wave
        {
            waveNumber = 2,
            enemyCount = 4,
            spawnDelay = 1.5f,
            spawnPoints = new[] 
            { 
                new Vector3(-8, 1, 12), 
                new Vector3(-14, 1, 10), 
                new Vector3(-11, 1, 16),
                new Vector3(-9, 1, 14)
            }
        });

        // Wave 3: 5 enemies (heavy)
        waves.Add(new Wave
        {
            waveNumber = 3,
            enemyCount = 5,
            spawnDelay = 1f,
            spawnPoints = new[] 
            { 
                new Vector3(-7, 1, 11), 
                new Vector3(-13, 1, 9), 
                new Vector3(-10, 1, 17),
                new Vector3(-16, 1, 12),
                new Vector3(-11, 1, 13)
            }
        });
    }

    public void StartWaveDefense()
    {
        if (waveInProgress) return;

        SpawnNextWave();
    }

    void SpawnNextWave()
    {
        if (currentWaveIndex >= waves.Count)
        {
            Debug.Log("[WAVES] All waves completed!");
            OnAllWavesCompleted?.Invoke();
            return;
        }

        Wave currentWave = waves[currentWaveIndex];
        waveInProgress = true;
        currentWaveIndex++;

        OnWaveStarted?.Invoke(currentWave.waveNumber);
        Debug.Log($"[WAVES] Wave {currentWave.waveNumber} started! Enemy count: {currentWave.enemyCount}");

        // Spawn enemies
        for (int i = 0; i < currentWave.enemyCount; i++)
        {
            StartCoroutine(SpawnEnemyDelayed(currentWave.spawnPoints[i], currentWave.spawnDelay * i));
        }
    }

    System.Collections.IEnumerator SpawnEnemyDelayed(Vector3 spawnPos, float delay)
    {
        yield return new WaitForSeconds(delay);

        GameObject enemyPrefab = Resources.Load<GameObject>("Prefabs/AI_Terrorist");
        if (enemyPrefab != null)
        {
            GameObject enemyGO = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            EnemyController enemy = enemyGO.GetComponent<EnemyController>();
            
            if (enemy != null)
            {
                enemy.SetTargetBomb(bombPosition);
                enemy.OnDeath += HandleEnemyDeath;
                activeEnemies.Add(enemy);
                totalEnemiesSpawned++;
            }
        }
    }

    void HandleEnemyDeath(EnemyController enemy)
    {
        activeEnemies.Remove(enemy);
        totalEnemiesDefeated++;

        // Kiểm tra wave complete
        if (activeEnemies.Count == 0 && waveInProgress)
        {
            waveInProgress = false;
            OnWaveCompleted?.Invoke(currentWaveIndex);
            
            // Spawn next wave sau 3 giây
            Invoke(nameof(SpawnNextWave), 3f);
        }
    }

    // Getters
    public int GetCurrentWave() => currentWaveIndex;
    public int GetTotalWaves() => waves.Count;
    public int GetActiveEnemiesCount() => activeEnemies.Count;
    public int GetTotalSpawned() => totalEnemiesSpawned;
    public int GetTotalDefeated() => totalEnemiesDefeated;
}
