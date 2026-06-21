using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CombineRecipe
{
    public string itemName = "pancake";
    public int requiredCount = 5;
    public GameObject resultPrefab;
    public bool enabled = true;
}

public class ItemCombiner : MonoBehaviour
{
    [Header("Combine Settings")]
    [SerializeField] private List<CombineRecipe> recipes = new List<CombineRecipe>();
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float checkInterval = 0.5f;
    
    private Collider2D combineZone;
    private float checkTimer = 0f;
    private List<GameObject> itemsInZone = new List<GameObject>();
    
    private void Start()
    {
        combineZone = GetComponent<Collider2D>();
        
        if (combineZone == null)
        {
            Debug.LogError("ItemCombiner: No Collider2D found! Add a trigger collider to this object.");
            return;
        }
        
        if (!combineZone.isTrigger)
        {
            Debug.LogWarning("ItemCombiner: Collider is not set to trigger!");
        }
        
        if (recipes.Count == 0)
        {
            Debug.LogWarning("ItemCombiner: No combine recipes assigned!");
        }
    }
    
    private void Update()
    {
        checkTimer += Time.deltaTime;
        
        if (checkTimer >= checkInterval)
        {
            CheckForCombinations();
            checkTimer = 0f;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!itemsInZone.Contains(collision.gameObject))
        {
            itemsInZone.Add(collision.gameObject);
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        itemsInZone.Remove(collision.gameObject);
    }
    
    private void CheckForCombinations()
    {
        // Remove null references
        itemsInZone.RemoveAll(item => item == null);
        
        // Check each recipe
        foreach (CombineRecipe recipe in recipes)
        {
            if (!recipe.enabled || recipe.resultPrefab == null)
                continue;
            
            // Count items matching recipe
            int count = 0;
            List<GameObject> matchingItems = new List<GameObject>();
            
            foreach (GameObject item in itemsInZone)
            {
                if (item.name.Contains(recipe.itemName))
                {
                    count++;
                    matchingItems.Add(item);
                }
            }
            
            // Check if we have enough items to combine
            if (count >= recipe.requiredCount)
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
            itemsInZone.Remove(items[i]);
            Destroy(items[i]);
        }
        
        // Spawn result item
        GameObject resultItem = Instantiate(recipe.resultPrefab, spawnPosition, Quaternion.identity);
        
        Debug.Log("Combined " + recipe.requiredCount + "x " + recipe.itemName + " into " + recipe.resultPrefab.name);
    }
    
    public void AddRecipe(string itemName, int count, GameObject resultPrefab)
    {
        CombineRecipe newRecipe = new CombineRecipe
        {
            itemName = itemName,
            requiredCount = count,
            resultPrefab = resultPrefab,
            enabled = true
        };
        
        recipes.Add(newRecipe);
    }
    
    public int GetItemCountInZone(string itemName)
    {
        int count = 0;
        foreach (GameObject item in itemsInZone)
        {
            if (item.name.Contains(itemName))
                count++;
        }
        return count;
    }
    
    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (combineZone != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(combineZone.bounds.center, combineZone.bounds.size);
        }
    }
    #endif
}
