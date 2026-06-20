using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float damageCooldown = 1f;
    
    private HealthManager playerHealth;
    private float lastDamageTime = 0f;
    
    private void Start()
    {
        // Find player by tag
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerHealth = playerObject.GetComponent<HealthManager>();
        }
        else
        {
            Debug.LogError("Player with tag 'Player' not found!");
        }
    }
    
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Check if enough time has passed since last damage
            if (Time.time - lastDamageTime >= damageCooldown)
            {
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damageAmount);
                    lastDamageTime = Time.time;
                    Debug.Log("Player hit! Health: " + playerHealth.GetCurrentHealth());
                }
            }
        }
    }
}
