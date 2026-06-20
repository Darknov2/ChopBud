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
    
    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHeartDisplay();
    }
    
    public void TakeDamage(int damage = 1)
    {
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
        Debug.Log("Player Died!");
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
}
