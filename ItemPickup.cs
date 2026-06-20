using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float pickupDelay = 0.1f;
    
    private bool canPickup = true;
    private Collider2D itemCollider;
    
    private void Start()
    {
        itemCollider = GetComponent<Collider2D>();
        
        if (itemCollider == null)
        {
            Debug.LogWarning("ItemPickup: No Collider2D found on " + gameObject.name);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (canPickup && collision.CompareTag(playerTag))
        {
            PickupItem(collision.gameObject);
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (canPickup && collision.gameObject.CompareTag(playerTag))
        {
            PickupItem(collision.gameObject);
        }
    }
    
    private void PickupItem(GameObject player)
    {
        Debug.Log("Item picked up by: " + player.name);
        
        // Add your inventory logic here
        // Example: player.GetComponent<Inventory>().AddItem(this);
        
        // Destroy the item
        Destroy(gameObject);
    }
    
    public void SetPickupEnabled(bool enabled)
    {
        canPickup = enabled;
    }
}
