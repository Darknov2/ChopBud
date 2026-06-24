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
    [SerializeField] private MouthAnimation mouthAnimation;
    
    [Header("Teleport Line Settings")]
    [SerializeField] private float lineFadeDuration = 0.5f;
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private Color lineColor = Color.white;
    [SerializeField] private float detectionRadius = 0.5f;
    
    [Header("Combine Settings")]
    [SerializeField] private bool enableCombineOnTeleport = true;
    [SerializeField] private List<CombineRecipe> combineRecipes = new List<CombineRecipe>();
    
    [Header("Item Pickup Settings")]
    [SerializeField] private bool pickupAllItems = true;
    [SerializeField] private string itemPickupTag = "Item";
    
    private Camera mainCamera;
    private InputManager inputManager;
    private HashSet<GameObject> pickedUpItems = new HashSet<GameObject>();
    private Dictionary<string, int> collectedItemCounts = new Dictionary<string, int>();
    
    private void Start()
    {
        if (canvas == null)
            canvas = FindObjectOfType<Canvas>();
        
        if (canvas == null)
        {
            Debug.LogError("Canvas not found! Make sure there is a Canvas in your scene.");
            return;
        }
        
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        
        // Position all UI elements on start
        PositionAllElements();
        
        Debug.Log("AutoPositionUI initialized. Canvas dimensions: " + canvas.GetComponent<RectTransform>().rect.size);
    }
    
    private void Update()
    {
        // Check for orientation changes periodically
        lastCheckTime += Time.deltaTime;
        if (lastCheckTime >= orientationCheckInterval)
        {
            lastCheckTime = 0f;
            
            // Check if screen size changed
            if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
            {
                lastScreenWidth = Screen.width;
                lastScreenHeight = Screen.height;
                
                if (debugMode)
                    Debug.Log("Screen size changed! Repositioning UI elements.");
                
                PositionAllElements();
            }
        }
    }
    
    private void TeleportToMouse()
    {
        // Check if player is dead
        if (healthManager != null && healthManager.IsDead())
            return;
        
        // Get teleport position from InputManager (works on both mobile and desktop)
        Vector3 worldPos = inputManager.GetInputWorldPosition(mainCamera);
        
        // Store initial position
        Vector3 startPos = transform.position;
        
        // Clear picked up items and collected counts from last teleport
        pickedUpItems.Clear();
        collectedItemCounts.Clear();
        
        // STEP 1: Check recipes FIRST along the line
        if (enableCombineOnTeleport)
        {
            CheckRecipesOnLine(startPos, worldPos);
        }
        
        // STEP 2: Pick up ALL items along the line
        if (pickupAllItems)
        {
            PickupAllItemsOnLine(startPos, worldPos);
        }
        
        // STEP 3: Execute recipes based on collected items
        if (enableCombineOnTeleport)
        {
            ExecuteRecipes();
        }
        
        // STEP 4: Check for enemies along the line
        CheckEnemiesOnLine(startPos, worldPos);
        
        // Teleport player to mouse position
        transform.position = new Vector3(worldPos.x, worldPos.y, transform.position.z);
        
        // Create teleport line effect
        StartCoroutine(DrawTeleportLine(startPos, worldPos));
        
        // Play mouth animation on teleport
        if (mouthAnimation != null)
        {
            mouthAnimation.PlayTeleportMouthAnimation();
        }
        
        Debug.Log("Teleported to: " + worldPos);
    }
    
    private void CheckRecipesOnLine(Vector3 startPos, Vector3 endPos)
    {
        Vector3 lineDirection = (endPos - startPos).normalized;
        float lineDistance = Vector3.Distance(startPos, endPos);
        
        // Initialize counters for each recipe item type
        foreach (CombineRecipe recipe in combineRecipes)
        {
            if (!collectedItemCounts.ContainsKey(recipe.itemName))
            {
                collectedItemCounts[recipe.itemName] = 0;
            }
        }
        
        // Method 1: Raycast along the line
        RaycastHit2D[] raycastHits = Physics2D.RaycastAll(startPos, lineDirection, lineDistance);
        
        foreach (RaycastHit2D hit in raycastHits)
        {
            if (hit.collider != null && hit.collider.CompareTag(itemPickupTag))
            {
                // Check if this item matches any recipe
                foreach (CombineRecipe recipe in combineRecipes)
                {
                    if (hit.collider.gameObject.name.Contains(recipe.itemName))
                    {
                        collectedItemCounts[recipe.itemName]++;
                        Debug.Log("Recipe check: Found " + recipe.itemName + " (Total: " + collectedItemCounts[recipe.itemName] + ")");
                        break;
                    }
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
                if (collider.CompareTag(itemPickupTag))
                {
                    // Check if this item matches any recipe
                    foreach (CombineRecipe recipe in combineRecipes)
                    {
                        if (collider.gameObject.name.Contains(recipe.itemName))
                        {
                            if (collectedItemCounts[recipe.itemName] == 0 || !HasBeenCounted(collider.gameObject))
                            {
                                collectedItemCounts[recipe.itemName]++;
                                Debug.Log("Recipe check (circle): Found " + recipe.itemName + " (Total: " + collectedItemCounts[recipe.itemName] + ")");
                            }
                            break;
                        }
                    }
                }
            }
        }
        
        Debug.Log("Recipe check complete. Item counts: " + string.Join(", ", collectedItemCounts));
    }
    
    private void PickupAllItemsOnLine(Vector3 startPos, Vector3 endPos)
    {
        Vector3 lineDirection = (endPos - startPos).normalized;
        float lineDistance = Vector3.Distance(startPos, endPos);
        
        List<GameObject> itemsToPickup = new List<GameObject>();
        
        // Method 1: Raycast along the line
        RaycastHit2D[] raycastHits = Physics2D.RaycastAll(startPos, lineDirection, lineDistance);
        
        foreach (RaycastHit2D hit in raycastHits)
        {
            if (hit.collider != null && hit.collider.CompareTag(itemPickupTag))
            {
                if (!itemsToPickup.Contains(hit.collider.gameObject))
                {
                    itemsToPickup.Add(hit.collider.gameObject);
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
                if (collider.CompareTag(itemPickupTag))
                {
                    if (!itemsToPickup.Contains(collider.gameObject))
                    {
                        itemsToPickup.Add(collider.gameObject);
                    }
                }
            }
        }
        
        // Pick up all items found
        foreach (GameObject item in itemsToPickup)
        {
            PickupItem(item);
        }
        
        if (itemsToPickup.Count > 0)
        {
            Debug.Log("Picked up " + itemsToPickup.Count + " items!");
        }
    }
    
    private void PickupItem(GameObject item)
    {
        if (pickedUpItems.Contains(item))
            return;
        
        pickedUpItems.Add(item);
        
        // Destroy the item
        Destroy(item);
        
        Debug.Log("Picked up item: " + item.name);
    }
    
    private void ExecuteRecipes()
    {
        // Check each recipe to see if we have enough items
        foreach (CombineRecipe recipe in combineRecipes)
        {
            if (!recipe.enabled || recipe.resultPrefab == null)
                continue;
            
            if (collectedItemCounts.ContainsKey(recipe.itemName) && 
                collectedItemCounts[recipe.itemName] >= recipe.requiredCount)
            {
                CombineItemsByRecipe(recipe);
            }
        }
    }
    
    private void CombineItemsByRecipe(CombineRecipe recipe)
    {
        // Find all items matching this recipe among picked up items
        List<GameObject> matchingItems = new List<GameObject>();
        
        foreach (GameObject item in pickedUpItems)
        {
            if (item != null && item.name.Contains(recipe.itemName))
            {
                matchingItems.Add(item);
                
                if (matchingItems.Count >= recipe.requiredCount)
                    break;
            }
        }
        
        // If we found enough items, combine them
        if (matchingItems.Count >= recipe.requiredCount)
        {
            // Calculate average position (use player position as fallback)
            Vector3 spawnPosition = transform.position;
            
            // Spawn result item
            GameObject resultItem = Instantiate(recipe.resultPrefab, spawnPosition, Quaternion.identity);
            
            Debug.Log("Combined " + recipe.requiredCount + "x " + recipe.itemName + " into " + recipe.resultPrefab.name);
        }
    }
    
    private bool HasBeenCounted(GameObject item)
    {
        return pickedUpItems.Contains(item);
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
