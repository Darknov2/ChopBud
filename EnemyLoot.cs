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
    [SerializeField] private float stopFallHeight = -5f; // Y position where items stop falling
    
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
                
                // Disable pickup temporarily, then enable after delay
                ItemPickup itemPickup = droppedItem.GetComponent<ItemPickup>();
                if (itemPickup != null)
                {
                    itemPickup.SetPickupEnabled(false);
                    StartCoroutine(EnablePickupAfterDelay(itemPickup));
                }
                
                // Start monitoring for when item reaches stop fall height
                StartCoroutine(MonitorItemFall(droppedItem, rb));
                
                Debug.Log("Dropped " + lootItemName + " at position: " + dropPosition + " with physics movement");
            }
            Debug.Log("Total dropped: " + lootAmount + " x " + lootItemName);
        }
        else
        {
            Debug.LogWarning("Loot prefab not assigned for: " + gameObject.name);
        }
    }
    
    private IEnumerator MonitorItemFall(GameObject item, Rigidbody2D rb)
    {
        while (item != null && rb != null)
        {
            // Check if item has reached the stop fall height
            if (item.transform.position.y <= stopFallHeight)
            {
                // Stop the item from falling
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.isKinematic = true;
                
                Debug.Log("Item stopped falling at height: " + item.transform.position.y);
                break;
            }
            
            yield return null;
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
