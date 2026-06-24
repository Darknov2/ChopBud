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
    private Dictionary<string, List<GameObject>> collectedItemsByType = new Dictionary<string, List<GameObject>>();
    
    private void Start()
    {
        mainCamera = Camera.main;
        
        if (healthManager == null)
        {
            healthManager = GetComponent<HealthManager>();
        }
        
        if (mouthAnimation == null)
        {
            mouthAnimation = GetComponent<MouthAnimation>();
        }
        
        // Get InputManager instance
        if (InputManager.instance != null)
        {
            inputManager = InputManager.instance;
            // Subscribe to teleport input event
            inputManager.OnTeleportInput += TeleportToMouse;
        }
        else
        {
            Debug.LogWarning("InputManager not found in scene! Teleport input will not work.");
        }
        
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from input event
        if (inputManager != null)
        {
            inputManager.OnTeleportInput -= TeleportToMouse;
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
        
        // Clear collected items from last teleport
        pickedUpItems.Clear();
        collectedItemsByType.Clear();
        
        // STEP 1: Check recipes first - collect all items matching recipes
        if (enableCombineOnTeleport)
        {
            CheckAndCollectRecipeItems(startPos, worldPos);
        }
        
        // STEP 2: Execute recipes based on collected items
        if (enableCombineOnTeleport)
        {
            ExecuteRecipes();
        }
        
        // STEP 3: Pick up ALL remaining items along the line (not used in recipes)
        if (pickupAllItems)
        {
            PickupAllItemsOnLine(startPos, worldPos);
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
    
    private void CheckAndCollectRecipeItems(Vector3 startPos, Vector3 endPos)
    {
        Vector3 lineDirection = (endPos - startPos).normalized;
        float lineDistance = Vector3.Distance(startPos, endPos);
        
        // Initialize dictionaries for each recipe item type
        foreach (CombineRecipe recipe in combineRecipes)
        {
            if (!collectedItemsByType.ContainsKey(recipe.itemName))
            {
                collectedItemsByType[recipe.itemName] = new List<GameObject>();
            }
        }
        
        List<GameObject> allItemsOnLine = new List<GameObject>();
        
        // Method 1: Raycast along the line
        RaycastHit2D[] raycastHits = Physics2D.RaycastAll(startPos, lineDirection, lineDistance);
        
        foreach (RaycastHit2D hit in raycastHits)
        {
            if (hit.collider != null && hit.collider.CompareTag(itemPickupTag))
            {
                if (!allItemsOnLine.Contains(hit.collider.gameObject))
                {
                    allItemsOnLine.Add(hit.collider.gameObject);
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
                    if (!allItemsOnLine.Contains(collider.gameObject))
                    {
                        allItemsOnLine.Add(collider.gameObject);
                    }
                }
            }
        }
        
        // Sort items by recipe type
        foreach (GameObject item in allItemsOnLine)
        {
            foreach (CombineRecipe recipe in combineRecipes)
            {
                if (item.name.Contains(recipe.itemName))
                {
                    collectedItemsByType[recipe.itemName].Add(item);
                    pickedUpItems.Add(item);
                    Debug.Log("Recipe check: Found " + recipe.itemName + " (Total: " + collectedItemsByType[recipe.itemName].Count + ")");
                    break;
                }
            }
        }
    }
    
    private void ExecuteRecipes()
    {
        foreach (CombineRecipe recipe in combineRecipes)
        {
            if (!recipe.enabled || recipe.resultPrefab == null)
                continue;
            
            if (collectedItemsByType.ContainsKey(recipe.itemName))
            {
                List<GameObject> items = collectedItemsByType[recipe.itemName];
                
                // Check if we have enough items for this recipe
                while (items.Count >= recipe.requiredCount)
                {
                    // Get the required number of items
                    List<GameObject> itemsToUse = new List<GameObject>();
                    for (int i = 0; i < recipe.requiredCount; i++)
                    {
                        itemsToUse.Add(items[i]);
                    }
                    
                    // Combine them
                    CombineItems(itemsToUse, recipe);
                    
                    // Remove used items from the list
                    for (int i = 0; i < recipe.requiredCount; i++)
                    {
                        items.RemoveAt(0);
                    }
                }
            }
        }
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
                if (!itemsToPickup.Contains(hit.collider.gameObject) && !pickedUpItems.Contains(hit.collider.gameObject))
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
                    if (!itemsToPickup.Contains(collider.gameObject) && !pickedUpItems.Contains(collider.gameObject))
                    {
                        itemsToPickup.Add(collider.gameObject);
                    }
                }
            }
        }
        
        // Pick up all remaining items found
        foreach (GameObject item in itemsToPickup)
        {
            PickupItem(item);
        }
        
        if (itemsToPickup.Count > 0)
        {
            Debug.Log("Picked up " + itemsToPickup.Count + " remaining items!");
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
            if (items[i] != null)
            {
                Destroy(items[i]);
            }
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
