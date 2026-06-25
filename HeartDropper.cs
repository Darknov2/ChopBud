using UnityEngine;

public class HeartDropper : MonoBehaviour
{
    [Header("Heart Drop Settings")]
    [SerializeField] private GameObject heartPrefab; // Heart item prefab to drop
    [SerializeField] private float dropChance = 0.15f; // 15% chance to drop (0-1)
    [SerializeField] private float dropForce = 3f; // Force to scatter the heart
    [SerializeField] private float dropRadius = 2f; // Radius around enemy to scatter heart
    
    [Header("Drop Physics")]
    [SerializeField] private float groundDrag = 2f; // Friction on ground
    [SerializeField] private float pickupDelay = 0.2f; // Delay before item can be picked up
    
    public void TryDropHeart()
    {
        Debug.Log("HeartDropper.TryDropHeart() called on " + gameObject.name);
        
        // Check if heart prefab is assigned
        if (heartPrefab == null)
        {
            Debug.LogWarning("HeartDropper: Heart prefab not assigned on " + gameObject.name + "!");
            return;
        }
        
        // Roll for drop chance
        float randomChance = Random.Range(0f, 1f);
        
        Debug.Log("Heart drop roll: " + randomChance + " vs chance: " + dropChance);
        
        if (randomChance > dropChance)
        {
            // Drop failed
            Debug.Log("Heart drop chance failed.");
            return;
        }
        
        // Chance succeeded - drop heart
        Debug.Log("Heart drop SUCCESSFUL!");
        DropHeart();
    }
    
    private void DropHeart()
    {
        if (heartPrefab == null)
        {
            Debug.LogError("HeartDropper: Heart prefab is null in DropHeart!");
            return;
        }
        
        // Random angle for scatter effect
        float randomAngle = Random.Range(0f, 360f);
        Vector2 scatterDirection = new Vector2(
            Mathf.Cos(randomAngle * Mathf.Deg2Rad),
            Mathf.Sin(randomAngle * Mathf.Deg2Rad)
        ).normalized;
        
        // Random distance within scatter radius
        float randomDistance = Random.Range(0.2f, dropRadius);
        Vector3 dropPosition = transform.position + (Vector3)scatterDirection * randomDistance;
        
        Debug.Log("Attempting to instantiate heart at: " + dropPosition);
        
        // Instantiate the heart
        GameObject droppedHeart = Instantiate(heartPrefab, dropPosition, Quaternion.identity);
        
        Debug.Log("Heart instantiated: " + droppedHeart.name);
        
        // Apply physics to the heart
        Rigidbody2D rb = droppedHeart.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.gravityScale = 1f; // Normal gravity
            rb.drag = groundDrag; // Friction
            
            // Apply scatter velocity
            Vector2 scatterVelocity = scatterDirection * dropForce;
            rb.velocity = scatterVelocity;
            
            Debug.Log("Heart physics applied: velocity = " + rb.velocity);
        }
        else
        {
            Debug.LogWarning("Heart prefab " + heartPrefab.name + " has no Rigidbody2D component!");
        }
        
        // Disable pickup temporarily
        HeartItem heartItem = droppedHeart.GetComponent<HeartItem>();
        if (heartItem != null)
        {
            heartItem.SetPickupEnabled(false);
            StartCoroutine(EnablePickupAfterDelay(heartItem));
            Debug.Log("Heart item found and pickup disabled temporarily");
        }
        else
        {
            Debug.LogWarning("Heart prefab " + heartPrefab.name + " has no HeartItem component!");
        }
        
        Debug.Log("Heart successfully dropped at: " + dropPosition);
    }
    
    private System.Collections.IEnumerator EnablePickupAfterDelay(HeartItem heartItem)
    {
        yield return new UnityEngine.WaitForSeconds(pickupDelay);
        
        if (heartItem != null)
        {
            heartItem.SetPickupEnabled(true);
            Debug.Log("Heart pickup enabled");
        }
    }
}
