using UnityEngine;

public class EnemyLoot : MonoBehaviour
{
    [Header("Loot Settings")]
    [SerializeField] private string lootItemName = "Gold"; // Name of item to drop
    [SerializeField] private int lootAmount = 5; // Amount to drop
    [SerializeField] private GameObject lootPrefab; // Prefab of item to drop
    [SerializeField] private float dropForce = 5f;
    [SerializeField] private float scatterRadius = 3f;
    
    public void DropLoot()
    {
        if (lootPrefab != null)
        {
            for (int i = 0; i < lootAmount; i++)
            {
                // Random angle for scatter effect (360 degrees)
                float randomAngle = Random.Range(0f, 360f);
                Vector2 scatterDirection = new Vector2(
                    Mathf.Cos(randomAngle * Mathf.Deg2Rad),
                    Mathf.Sin(randomAngle * Mathf.Deg2Rad)
                ).normalized;
                
                // Random distance within scatter radius
                float randomDistance = Random.Range(0.2f, scatterRadius);
                Vector3 dropPosition = transform.position + (Vector3)scatterDirection * randomDistance;
                
                // Instantiate the loot item
                GameObject droppedItem = Instantiate(lootPrefab, dropPosition, Quaternion.identity);
                
                // Add randomness to velocity
                Vector2 velocity = scatterDirection * dropForce;
                Rigidbody2D rb = droppedItem.GetComponent<Rigidbody2D>();
                
                if (rb != null)
                {
                    rb.velocity = velocity;
                }
                
                Debug.Log("Dropped " + lootItemName + " at position: " + dropPosition);
            }
            Debug.Log("Total dropped: " + lootAmount + " x " + lootItemName);
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
