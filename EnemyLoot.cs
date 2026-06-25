using UnityEngine;
using System.Collections;

public class EnemyLoot : MonoBehaviour
{
    [Header("Loot Settings")]
    [SerializeField] private string lootItemName = "Gold"; // Name of item to drop
    [SerializeField] private int lootAmount = 5; // Amount to drop
    [SerializeField] private GameObject lootPrefab; // Prefab of item to drop
    
    [Header("Drop Physics")]
    [SerializeField] private float scatterForce = 2f; // Reduced from dropForce for slower scatter
    [SerializeField] private float scatterRadius = 3f;
    [SerializeField] private float pickupDelay = 0.2f; // Delay before items can be picked up
    
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
                
                // Apply physics immediately with proper initialization
                if (rb != null)
                {
                    // Make sure rigidbody is not kinematic
                    rb.isKinematic = false;
                    rb.gravityScale = 0f; // No gravity - items float
                    
                    // Calculate velocity: outward scatter with low speed
                    Vector2 scatterVelocity = scatterDirection * scatterForce;
                    
                    rb.velocity = scatterVelocity;
                    
                    // Add slight rotation for spinning effect
                    rb.angularVelocity = Random.Range(-180f, 180f);
                    
                    Debug.Log("Applied physics to dropped item: velocity = " + rb.velocity);
                }
                else
                {
                    Debug.LogWarning("Dropped item has no Rigidbody2D!");
                }
                
                // Disable pickup temporarily
                ItemPickup itemPickup = droppedItem.GetComponent<ItemPickup>();
                if (itemPickup != null)
                {
                    itemPickup.SetPickupEnabled(false);
                    StartCoroutine(EnablePickupAfterDelay(itemPickup));
                }
                
                Debug.Log("Dropped " + lootItemName + " at position: " + dropPosition + " with scatter movement");
            }
            Debug.Log("Total dropped: " + lootAmount + " x " + lootItemName);
        }
        else
        {
            Debug.LogWarning("Loot prefab not assigned for: " + gameObject.name);
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
