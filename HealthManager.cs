using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private Image[] heartImages;
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;
    
    private int currentHealth;
    private bool isDead = false;
    
    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHeartDisplay();
        Debug.Log("HealthManager initialized: Max Health = " + maxHealth + ", Heart Images count = " + (heartImages != null ? heartImages.Length : 0));
    }
    
    public void TakeDamage(int damage = 1)
    {
        if (isDead)
            return;
        
        currentHealth -= damage;
        
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
        
        UpdateHeartDisplay();
        Debug.Log("Player took " + damage + " damage. Current health: " + currentHealth);
        
        if (currentHealth <= 0)
        {
            PlayerDied();
        }
    }
    
    public void Heal(int healAmount = 1)
    {
        Debug.Log("HealthManager.Heal() called with healAmount: " + healAmount);
        Debug.Log("  Before: currentHealth = " + currentHealth + ", maxHealth = " + maxHealth);
        
        if (isDead)
        {
            Debug.Log("  Heal blocked: Player is dead");
            return;
        }
        
        currentHealth += healAmount;
        
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        
        Debug.Log("  After: currentHealth = " + currentHealth);
        
        UpdateHeartDisplay();
        Debug.Log("  Heart display updated");
    }
    
    private void UpdateHeartDisplay()
    {
        if (heartImages == null || heartImages.Length == 0)
        {
            Debug.LogWarning("HealthManager: heartImages array is empty or null! Health UI will not update.");
            return;
        }
        
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < currentHealth)
            {
                heartImages[i].sprite = fullHeartSprite;
            }
            else
            {
                heartImages[i].sprite = emptyHeartSprite;
            }
        }
        
        Debug.Log("Heart display updated to show: " + currentHealth + "/" + maxHealth);
    }
    
    private void PlayerDied()
    {
        isDead = true;
        Debug.Log("Player Died!");
        
        // Disable TeleportSystem to prevent further teleporting
        TeleportSystem teleportSystem = GetComponent<TeleportSystem>();
        if (teleportSystem != null)
        {
            teleportSystem.enabled = false;
        }
        
        // Stop player movement by freezing rigidbody
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true;
        }
        
        // Add death logic here (restart level, game over screen, etc.)
    }
    
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public int GetMaxHealth()
    {
        return maxHealth;
    }
    
    public bool IsDead()
    {
        return isDead;
    }
}
