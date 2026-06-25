using UnityEngine;
using System.Collections;

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
    [SerializeField] private float pickupDelay = 0.2f; // Delay before items can be picked up
    [SerializeField] private float freezeAfterTime = 2f; // Time before items freeze
    
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
                
                // Disable pickup temporarily, then enable after delay
                ItemPickup itemPickup = droppedItem.GetComponent<ItemPickup>();
                if (itemPickup != null)
                {
                    itemPickup.SetPickupEnabled(false);
                    StartCoroutine(EnablePickupAfterDelay(itemPickup));
                }
                
                // Start applying physics with a delay to ensure rigidbody is ready
                StartCoroutine(ApplyPhysicsWithDelay(droppedItem, scatterDirection));
                
                // Start timer to freeze item after set time
                StartCoroutine(FreezeItemAfterTime(droppedItem));
                
                Debug.Log("Dropped " + lootItemName + " at position: " + dropPosition + " with physics movement");
            }
            Debug.Log("Total dropped: " + lootAmount + " x " + lootItemName);
        }
        else
        {
            Debug.LogWarning("Loot prefab not assigned for: " + gameObject.name);
        }
    }
    
    private IEnumerator ApplyPhysicsWithDelay(GameObject item, Vector2 scatterDirection)
    {
        // Wait a frame to ensure Rigidbody2D is fully initialized
        yield return new WaitForEndOfFrame();
        
        if (item == null)
            yield break;
        
        // Get Rigidbody2D component
        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
        
        if (rb != null)
        {
            // Make sure rigidbody is not kinematic
            rb.isKinematic = false;
            
            // Calculate velocity: outward scatter + upward force
            Vector2 scatterVelocity = scatterDirection * dropForce;
            Vector2 upwardVelocity = Vector2.up * upwardForce;
            
            rb.velocity = scatterVelocity + upwardVelocity;
            
            // Add rotation for spinning effect
            rb.angularVelocity = Random.Range(-360f, 360f);
            
            Debug.Log("Applied physics to dropped item: velocity = " + rb.velocity);
        }
        else
        {
            Debug.LogWarning("Dropped item has no Rigidbody2D!");
        }
    }
    
    private IEnumerator FreezeItemAfterTime(GameObject item)
    {
        // Wait for the specified freeze time
        yield return new WaitForSeconds(freezeAfterTime);
        
        if (item == null)
            yield break;
        
        // Get Rigidbody2D component
        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
        
        if (rb != null)
        {
            // Stop the item from falling
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true;
            
            Debug.Log("Item frozen after " + freezeAfterTime + " seconds at position: " + item.transform.position);
        }
    }
    
    private IEnumerator EnablePickupAfterDelay(ItemPickup itemPickup)
    {
        yield return new WaitForSeconds(pickupDelay);
        
        if (itemPickup != null)
        {
            itemPickup.SetPickupEnabled(true);
            Debug.Log("Item pickup enabled");
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
