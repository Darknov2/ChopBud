using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private Image[] heartImages;
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;
    
    [Header("Death Settings")]
    [SerializeField] private TopDownPlayerMovement playerMovement;
    
    private int currentHealth;
    private bool isDead = false;
    
    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHeartDisplay();
        
        if (playerMovement == null)
        {
            playerMovement = GetComponent<TopDownPlayerMovement>();
        }
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
        
        if (currentHealth <= 0)
        {
            PlayerDied();
        }
    }
    
    public void Heal(int healAmount = 1)
    {
        if (isDead)
            return;
        
        currentHealth += healAmount;
        
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        
        UpdateHeartDisplay();
    }
    
    private void UpdateHeartDisplay()
    {
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
    }
    
    private void PlayerDied()
    {
        isDead = true;
        Debug.Log("Player Died!");
        
        // Disable player movement
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
        
        // Disable player rigidbody velocity
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
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
