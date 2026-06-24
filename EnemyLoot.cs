using UnityEngine;

public class EnemyLoot : MonoBehaviour
{
    [Header("Loot Settings")]
    [SerializeField] private string lootItemName = "Gold"; // Name of item to drop
    [SerializeField] private int lootAmount = 5; // Amount to drop
    [SerializeField] private GameObject lootPrefab; // Prefab of item to drop
    
    [Header("Drop Physics")]
    [SerializeField] private float dropForce = 8f;
    [SerializeField] private float scatterRadius = 3f;
    [SerializeField] private float upwardForce = 3f; // Initial upward velocity
    [SerializeField] private float gravityScale = 1f; // How heavy the items feel
    [SerializeField] private float angularVelocity = 180f; // Spin effect
    
    [Header("Drop Animation")]
    [SerializeField] private float pickupDelay = 0.2f; // Delay before items can be picked up
    [SerializeField] private AnimationCurve dropFadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);
    
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
                
                // Get Rigidbody2D component
                Rigidbody2D rb = droppedItem.GetComponent<Rigidbody2D>();
                
                if (rb != null)
                {
                    // Set gravity scale for natural falling motion
                    rb.gravityScale = gravityScale;
                    
                    // Calculate velocity: outward scatter + upward force
                    Vector2 scatterVelocity = scatterDirection * dropForce;
                    Vector2 upwardVelocity = Vector2.up * upwardForce;
                    
                    rb.velocity = scatterVelocity + upwardVelocity;
                    
                    // Add rotation for spinning effect
                    rb.angularVelocity = Random.Range(-angularVelocity, angularVelocity);
                }
                
                // Disable pickup temporarily
                ItemPickup itemPickup = droppedItem.GetComponent<ItemPickup>();
                if (itemPickup != null)
                {
                    itemPickup.SetPickupEnabled(false);
                    // Enable pickup after delay
                    Invoke(nameof(EnablePickup), pickupDelay);
                }
                
                Debug.Log("Dropped " + lootItemName + " at position: " + dropPosition + " with physics movement");
            }
            Debug.Log("Total dropped: " + lootAmount + " x " + lootItemName);
        }
        else
        {
            Debug.LogWarning("Loot prefab not assigned for: " + gameObject.name);
        }
    }
    
    private void EnablePickup()
    {
        // This will be called on a delay to enable pickup
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
