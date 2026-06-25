using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class Wave
{
    public string waveName = "Wave 1";
    public int maxEnemies = 10;
    public List<GameObject> enemyPrefabs = new List<GameObject>();
    public float spawnRate = 2f; // Seconds between spawns
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private bool autoStart = true;
    
    [Header("Wave Settings")]
    [SerializeField] private List<Wave> waves = new List<Wave>();
    [SerializeField] private float breakBetweenWaves = 15f; // 15 seconds between waves
    
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI waveText; // Display wave start message
    
    private float spawnTimer = 0f;
    private int enemiesSpawnedInWave = 0; // Track how many enemies we've spawned
    private int enemiesKilledInWave = 0; // Track how many enemies have been killed
    private int currentWaveIndex = 0;
    private bool isSpawning = false;
    private bool isWaveActive = false;
    private bool isBetweenWaves = false;
    private float breakTimer = 0f;
    private bool waveEnded = false; // Flag to prevent multiple wave end calls
    
    private static EnemySpawner instance;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (playerTransform == null)
        {
            // Try to find player automatically
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogError("EnemySpawner: Player not found! Assign player transform manually.");
                return;
            }
        }
        
        if (waves.Count == 0)
        {
            Debug.LogWarning("EnemySpawner: No waves configured!");
            return;
        }
        
        if (autoStart)
        {
            StartWaveSystem();
        }
    }
    
    private void Update()
    {
        if (!isSpawning || playerTransform == null)
            return;
        
        // Handle wave break countdown
        if (isBetweenWaves)
        {
            breakTimer -= Time.deltaTime;
            
            if (waveText != null)
            {
                waveText.text = "Next Wave in: " + Mathf.Max(0, Mathf.Ceil(breakTimer)).ToString() + "s";
            }
            
            if (breakTimer <= 0f)
            {
                isBetweenWaves = false;
                StartNextWave();
            }
            return;
        }
        
        // Handle spawning during wave
        if (isWaveActive && currentWaveIndex < waves.Count)
        {
            Wave currentWave = waves[currentWaveIndex];
            
            spawnTimer += Time.deltaTime;
            
            if (spawnTimer >= currentWave.spawnRate)
            {
                if (enemiesSpawnedInWave < currentWave.maxEnemies)
                {
                    SpawnEnemy(currentWave);
                }
                spawnTimer = 0f;
            }
        }
        
        // Check if wave is complete (all enemies spawned and all killed)
        // This happens OUTSIDE the spawning block so it checks even after spawning stops
        if (isWaveActive && !waveEnded && enemiesSpawnedInWave > 0 && enemiesKilledInWave >= enemiesSpawnedInWave)
        {
            waveEnded = true;
            
            if (currentWaveIndex < waves.Count - 1)
            {
                EndWave();
            }
            else
            {
                AllWavesComplete();
            }
        }
    }
    
    private void StartWaveSystem()
    {
        isSpawning = true;
        currentWaveIndex = 0;
        StartNextWave();
        Debug.Log("Wave system started!");
    }
    
    private void StartNextWave()
    {
        if (currentWaveIndex >= waves.Count)
        {
            AllWavesComplete();
            return;
        }
        
        enemiesSpawnedInWave = 0;
        enemiesKilledInWave = 0;
        spawnTimer = 0f;
        isWaveActive = true;
        waveEnded = false; // Reset flag for new wave
        
        Wave currentWave = waves[currentWaveIndex];
        
        // Display wave start message
        if (waveText != null)
        {
            waveText.text = currentWave.waveName + " Started!";
            StartCoroutine(FadeWaveText());
        }
        
        Debug.Log(currentWave.waveName + " started! Max enemies: " + currentWave.maxEnemies);
    }
    
    private void SpawnEnemy(Wave wave)
    {
        if (wave.enemyPrefabs.Count == 0)
        {
            Debug.LogWarning("Wave " + (currentWaveIndex + 1) + " has no enemy prefabs assigned!");
            return;
        }
        
        // Get random position around player
        float randomAngle = Random.Range(0f, 360f);
        float randomDistance = Random.Range(spawnRadius * 0.5f, spawnRadius);
        
        Vector2 spawnOffset = new Vector2(
            Mathf.Cos(randomAngle * Mathf.Deg2Rad),
            Mathf.Sin(randomAngle * Mathf.Deg2Rad)
        ) * randomDistance;
        
        Vector3 spawnPosition = playerTransform.position + (Vector3)spawnOffset;
        
        // Get random enemy prefab from wave
        GameObject randomEnemyPrefab = wave.enemyPrefabs[Random.Range(0, wave.enemyPrefabs.Count)];
        
        // Instantiate enemy
        GameObject newEnemy = Instantiate(randomEnemyPrefab, spawnPosition, Quaternion.identity);
        
        enemiesSpawnedInWave++;
        
        Debug.Log("Enemy spawned at: " + spawnPosition + " (Spawned: " + enemiesSpawnedInWave + "/" + wave.maxEnemies + ", Killed: " + enemiesKilledInWave + ")");
    }
    
    private void EndWave()
    {
        isWaveActive = false;
        currentWaveIndex++;
        isBetweenWaves = true;
        breakTimer = breakBetweenWaves;
        
        Debug.Log("Wave ended! " + enemiesKilledInWave + " enemies killed. Starting " + breakBetweenWaves + "s break before next wave...");
    }
    
    private void AllWavesComplete()
    {
        isSpawning = false;
        isWaveActive = false;
        
        if (waveText != null)
        {
            waveText.text = "All Waves Complete!";
        }
        
        Debug.Log("All waves completed!");
    }
    
    private IEnumerator FadeWaveText()
    {
        // Display wave text for 3 seconds then fade
        yield return new WaitForSeconds(3f);
        
        if (waveText != null && !isBetweenWaves)
        {
            waveText.text = "";
        }
    }
    
    public void OnEnemyDestroyed()
    {
        if (isWaveActive)
        {
            enemiesKilledInWave++;
            Debug.Log("Enemy defeated! Wave Progress: " + enemiesKilledInWave + "/" + enemiesSpawnedInWave);
        }
    }
    
    public void StopSpawning()
    {
        isSpawning = false;
        isWaveActive = false;
        isBetweenWaves = false;
        Debug.Log("Wave system stopped!");
    }
    
    public int GetEnemiesSpawnedInWave()
    {
        return enemiesSpawnedInWave;
    }
    
    public int GetEnemiesKilledInWave()
    {
        return enemiesKilledInWave;
    }
    
    public int GetCurrentWaveIndex()
    {
        return currentWaveIndex + 1; // Return 1-indexed wave number
    }
    
    public static EnemySpawner GetInstance()
    {
        return instance;
    }
    
    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, spawnRadius);
        }
    }
    #endif
}
