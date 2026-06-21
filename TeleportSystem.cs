using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CombineRecipe
{
    public string itemName = "pancake";
    public int requiredCount = 5;
    public GameObject resultPrefab;
    public bool enabled = true;
}

public class TeleportSystem : MonoBehaviour
{
    [Header("Teleport Settings")]
    [SerializeField] private HealthManager healthManager;
    
    [Header("Teleport Line Settings")]
    [SerializeField] private float lineFadeDuration = 0.5f;
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private Color lineColor = Color.white;
    [SerializeField] private float detectionRadius = 0.5f;
    
    [Header("Combine Settings")]
    [SerializeField] private bool enableCombineOnTeleport = true;
    [SerializeField] private List<CombineRecipe> combineRecipes = new List<CombineRecipe>();
    
    private Camera mainCamera;
    
    private void Start()
    {
        mainCamera = Camera.main;
        
        if (healthManager == null)
        {
            healthManager = GetComponent<HealthManager>();
        }
        
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
        }
    }
    
    private void Update()
    {
        // Check if player is dead
        if (healthManager != null && healthManager.IsDead())
            return;
        
        // Check for mouse click
        if (Input.GetMouseButtonDown(0))
        {
            TeleportToMouse();
        }
    }
    
    private void TeleportToMouse()
    {
        // Get mouse position in world coordinates
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f; // Distance from camera
        
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        
        // Store initial position
        Vector3 startPos = transform.position;
        
        // Check for enemies along the line BEFORE teleporting
        CheckEnemiesOnLine(startPos, worldPos);
        
        // Check for items to combine on the line
        if (enableCombineOnTeleport)
        {
            CombineItemsOnLine(startPos, worldPos);
        }
        
        // Teleport player to mouse position
        transform.position = new Vector3(worldPos.x, worldPos.y, transform.position.z);
        
        // Create teleport line effect
        StartCoroutine(DrawTeleportLine(startPos, worldPos));
        
        Debug.Log("Teleported to: " + worldPos);
    }
    
    private IEnumerator DrawTeleportLine(Vector3 startPos, Vector3 endPos)
    {
        // Create a new GameObject for the line
        GameObject lineObject = new GameObject("TeleportLine");
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
        
        // Setup LineRenderer
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        
        // Create material for the line
        Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material = lineMaterial;
        
        // Set initial color
        Color startColor = lineColor;
        lineRenderer.startColor = startColor;
        lineRenderer.endColor = startColor;
        
        // Fade out over time
        float elapsedTime = 0f;
        while (elapsedTime < lineFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float fadeProgress = elapsedTime / lineFadeDuration;
            
            // Lerp the alpha from 1 to 0
            Color fadeColor = startColor;
            fadeColor.a = Mathf.Lerp(1f, 0f, fadeProgress);
            
            lineRenderer.startColor = fadeColor;
            lineRenderer.endColor = fadeColor;
            
            yield return null;
        }
        
        // Destroy the line object
        Destroy(lineObject);
    }
    
    private void CombineItemsOnLine(Vector3 startPos, Vector3 endPos)
    {
        if (combineRecipes.Count == 0)
            return;
        
        Vector3 lineDirection = (endPos - startPos).normalized;
        float lineDistance = Vector3.Distance(startPos, endPos);
        
        // Check each recipe
        foreach (CombineRecipe recipe in combineRecipes)
        {
            if (!recipe.enabled || recipe.resultPrefab == null)
                continue;
            
            // Find all items matching this recipe on the line
            List<GameObject> matchingItems = new List<GameObject>();
            
            // Method 1: Raycast along the line
            RaycastHit2D[] raycastHits = Physics2D.RaycastAll(startPos, lineDirection, lineDistance);
            
            foreach (RaycastHit2D hit in raycastHits)
            {
                if (hit.collider != null && hit.collider.gameObject.name.Contains(recipe.itemName))
                {
                    if (!matchingItems.Contains(hit.collider.gameObject))
                    {
                        matchingItems.Add(hit.collider.gameObject);
                    }
                }
            }
            
            // Method 2: Circle cast along the line to catch nearby items
            int steps = Mathf.Max(5, (int)(lineDistance / 0.5f));
            for (int i = 0; i < steps; i++)
            {
                float t = (float)i / steps;
                Vector3 checkPos = Vector3.Lerp(startPos, endPos, t);
                
                Collider2D[] colliders = Physics2D.OverlapCircleAll(checkPos, detectionRadius);
                
                foreach (Collider2D collider in colliders)
                {
                    if (collider.gameObject.name.Contains(recipe.itemName))
                    {
                        if (!matchingItems.Contains(collider.gameObject))
                        {
                            matchingItems.Add(collider.gameObject);
                        }
                    }
                }
            }
            
            // Check if we have enough items to combine
            if (matchingItems.Count >= recipe.requiredCount)
            {
                CombineItems(matchingItems, recipe);
            }
        }
    }
    
    private void CombineItems(List<GameObject> items, CombineRecipe recipe)
    {
        // Calculate average position
        Vector3 spawnPosition = Vector3.zero;
        
        for (int i = 0; i < recipe.requiredCount && i < items.Count; i++)
        {
            spawnPosition += items[i].transform.position;
        }
        
        spawnPosition /= recipe.requiredCount;
        
        // Destroy original items
        for (int i = 0; i < recipe.requiredCount && i < items.Count; i++)
        {
            Destroy(items[i]);
        }
        
        // Spawn result item
        GameObject resultItem = Instantiate(recipe.resultPrefab, spawnPosition, Quaternion.identity);
        
        Debug.Log("Combined " + recipe.requiredCount + "x " + recipe.itemName + " into " + recipe.resultPrefab.name);
    }
    
    private void CheckEnemiesOnLine(Vector3 startPos, Vector3 endPos)
    {
        Vector3 lineDirection = (endPos - startPos).normalized;
        float lineDistance = Vector3.Distance(startPos, endPos);
        
        // Method 1: Raycast along the line
        RaycastHit2D[] raycastHits = Physics2D.RaycastAll(startPos, lineDirection, lineDistance);
        
        Debug.Log("Raycast hits: " + raycastHits.Length);
        
        foreach (RaycastHit2D hit in raycastHits)
        {
            if (hit.collider != null)
            {
                Debug.Log("Hit: " + hit.collider.gameObject.name + " Tag: " + hit.collider.tag);
                
                if (hit.collider.CompareTag("Enemy"))
                {
                    DestroyEnemy(hit.collider.gameObject);
                }
            }
        }
        
        // Method 2: Circle cast along the line to catch nearby enemies
        int steps = Mathf.Max(5, (int)(lineDistance / 0.5f));
        for (int i = 0; i < steps; i++)
        {
            float t = (float)i / steps;
            Vector3 checkPos = Vector3.Lerp(startPos, endPos, t);
            
            Collider2D[] colliders = Physics2D.OverlapCircleAll(checkPos, detectionRadius);
            
            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag("Enemy"))
                {
                    DestroyEnemy(collider.gameObject);
                }
            }
        }
    }
    
    private void DestroyEnemy(GameObject enemy)
    {
        EnemyLoot enemyLoot = enemy.GetComponent<EnemyLoot>();
        
        if (enemyLoot != null)
        {
            // Drop loot before destroying
            enemyLoot.DropLoot();
        }
        
        // Destroy the enemy
        Destroy(enemy);
        Debug.Log("Enemy destroyed by teleport line!");
    }
}
