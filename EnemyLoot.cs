using UnityEngine;

public class EnemyLoot : MonoBehaviour
{
    [Header("Loot Settings")]
    [SerializeField] private string lootItemName = "Gold"; // Name of item to drop
    [SerializeField] private int lootAmount = 5; // Amount to drop
    [SerializeField] private GameObject lootPrefab; // Prefab of item to drop
    [SerializeField] private float dropForce = 2f;
    
    public void DropLoot()
    {
        if (lootPrefab != null)
        {
            for (int i = 0; i < lootAmount; i++)
            {
                GameObject droppedItem = Instantiate(lootPrefab, transform.position, Quaternion.identity);
                
                // Add some randomness to drop direction
                Vector2 dropDirection = Random.insideUnitCircle.normalized;
                Rigidbody2D rb = droppedItem.GetComponent<Rigidbody2D>();
                
                if (rb != null)
                {
                    rb.velocity = dropDirection * dropForce;
                }
                
                Debug.Log("Dropped " + lootItemName + " x" + lootAmount);
            }
        }
        else
        {
            Debug.LogWarning("Loot prefab not assigned for: " + gameObject.name);
        }
    }
    
    public string GetLootName()
    {
        return lootItemName;
    }
    
    public int GetLootAmount()
    {
        return lootAmount;
    }
}
