using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private float spawnRate = 2f; // Seconds between spawns
    [SerializeField] private int maxEnemies = 10;
    [SerializeField] private bool autoStart = true;
    
    [Header("Enemy Prefabs")]
    [SerializeField] private List<GameObject> enemyPrefabs = new List<GameObject>();
    
    private float spawnTimer = 0f;
    private int currentEnemyCount = 0;
    private bool isSpawning = false;
    
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
        
        if (enemyPrefabs.Count == 0)
        {
            Debug.LogWarning("EnemySpawner: No enemy prefabs assigned!");
            return;
        }
        
        if (autoStart)
        {
            StartSpawning();
        }
    }
    
    private void Update()
    {
        if (!isSpawning || playerTransform == null)
            return;
        
        spawnTimer += Time.deltaTime;
        
        if (spawnTimer >= spawnRate)
        {
            if (currentEnemyCount < maxEnemies)
            {
                SpawnEnemy();
            }
            spawnTimer = 0f;
        }
    }
    
    private void SpawnEnemy()
    {
        // Get random position around player
        float randomAngle = Random.Range(0f, 360f);
        float randomDistance = Random.Range(spawnRadius * 0.5f, spawnRadius);
        
        Vector2 spawnOffset = new Vector2(
            Mathf.Cos(randomAngle * Mathf.Deg2Rad),
            Mathf.Sin(randomAngle * Mathf.Deg2Rad)
        ) * randomDistance;
        
        Vector3 spawnPosition = playerTransform.position + (Vector3)spawnOffset;
        
        // Get random enemy prefab
        GameObject randomEnemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
        
        // Instantiate enemy
        GameObject newEnemy = Instantiate(randomEnemyPrefab, spawnPosition, Quaternion.identity);
        
        currentEnemyCount++;
        
        Debug.Log("Enemy spawned at: " + spawnPosition + " (Total: " + currentEnemyCount + ")");
    }
    
    public void StartSpawning()
    {
        isSpawning = true;
        Debug.Log("Enemy spawner started!");
    }
    
    public void StopSpawning()
    {
        isSpawning = false;
        Debug.Log("Enemy spawner stopped!");
    }
    
    public void OnEnemyDestroyed()
    {
        if (currentEnemyCount > 0)
        {
            currentEnemyCount--;
            Debug.Log("Enemy destroyed. Remaining: " + currentEnemyCount);
        }
    }
    
    public int GetCurrentEnemyCount()
    {
        return currentEnemyCount;
    }
    
    public void SetMaxEnemies(int max)
    {
        maxEnemies = max;
    }
    
    public void SetSpawnRate(float rate)
    {
        spawnRate = rate;
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
