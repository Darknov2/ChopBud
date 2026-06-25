using UnityEngine;

public class HeartItem : MonoBehaviour
{
    [Header("Heart Settings")]
    [SerializeField] private int healAmount = 1;
    [SerializeField] private bool destroyAfterPickup = true;
    
    [Header("Sound Settings")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float soundVolume = 1f;
    [SerializeField] private bool playSound = true;
    
    [Header("Magnet Settings")]
    [SerializeField] private float pullDistance = 3f; // Distance at which items start being pulled
    [SerializeField] private float pullSpeed = 5f; // Speed at which items move toward player
    [SerializeField] private float pickupDistance = 0.5f; // Distance at which item is picked up
    
    private bool canPickup = true;
    private Collider2D itemCollider;
    private AudioSource audioSource;
    private Rigidbody2D rb;
    private GameObject player;
    private bool isPulling = false;
    
    private void Start()
    {
        itemCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        
        if (itemCollider == null)
        {
            Debug.LogWarning("HeartItem: No Collider2D found on " + gameObject.name);
        }
        
        // Ensure collider is a trigger so teleport can detect it
        if (itemCollider != null)
        {
            itemCollider.isTrigger = true;
        }
        
        // Create AudioSource if playSound is enabled
        InitializeAudioSource();
        
        Debug.Log("HeartItem initialized: " + gameObject.name + ", Collider trigger: " + (itemCollider != null ? itemCollider.isTrigger : false));
    }
    
    private void Update()
    {
        // Find player if not already found
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
            {
                player = foundPlayer;
            }
        }
        
        // Check if player is within pull distance
        if (player != null && canPickup && !isPulling)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            
            if (distanceToPlayer <= pullDistance)
            {
                isPulling = true;
                Debug.Log("Heart " + gameObject.name + " started pulling toward player");
            }
        }
        
        // Pull item toward player if pulling
        if (isPulling && player != null)
        {
            PullTowardPlayer();
        }
    }
    
    private void PullTowardPlayer()
    {
        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        
        // Check if item is close enough to pick up
        if (distanceToPlayer <= pickupDistance)
        {
            PickupItem(player);
            return;
        }
        
        // Move item toward player
        if (rb != null && !rb.isKinematic)
        {
            // Use velocity to pull item
            rb.velocity = directionToPlayer * pullSpeed;
        }
        else
        {
            // Fallback to transform movement if no rigidbody
            transform.position += directionToPlayer * pullSpeed * Time.deltaTime;
        }
    }
    
    private void InitializeAudioSource()
    {
        if (playSound && pickupSound != null)
        {
            audioSource = GetComponent<AudioSource>();
            
            // Create AudioSource if it doesn't exist
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            audioSource.clip = pickupSound;
            audioSource.volume = soundVolume;
            audioSource.playOnAwake = false;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (canPickup && collision.CompareTag("Player"))
        {
            Debug.Log("HeartItem: OnTriggerEnter2D with player! " + collision.gameObject.name);
            PickupItem(collision.gameObject);
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (canPickup && collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("HeartItem: OnCollisionEnter2D with player!");
            PickupItem(collision.gameObject);
        }
    }
    
    private void PickupItem(GameObject playerObj)
    {
        if (!canPickup)
        {
            Debug.Log("HeartItem: Pickup disabled, ignoring pickup attempt");
            return;
        }
        
        Debug.Log("HeartItem: PickupItem called on player: " + playerObj.name);
        
        canPickup = false;
        isPulling = false;
        
        // Get player health manager - search recursively through all components
        HealthManager healthManager = playerObj.GetComponent<HealthManager>();
        
        Debug.Log("HeartItem: HealthManager found: " + (healthManager != null ? "YES" : "NO"));
        
        if (healthManager == null)
        {
            Debug.LogError("HeartItem: HealthManager is NULL on player " + playerObj.name + "! Searching all components...");
            
            // Debug: list all components
            Component[] components = playerObj.GetComponents<Component>();
            foreach (Component comp in components)
            {
                Debug.Log("  - Component found: " + comp.GetType().Name);
            }
            
            Debug.LogWarning("HeartItem: HealthManager not found on player!");
            return;
        }
        
        // Check if player is already at full health
        int currentHealth = healthManager.GetCurrentHealth();
        int maxHealth = healthManager.GetMaxHealth();
        
        Debug.Log("HeartItem: Current Health: " + currentHealth + " / Max Health: " + maxHealth);
        
        if (currentHealth >= maxHealth)
        {
            Debug.Log("Player health is already full! Heart not used.");
            canPickup = true; // Re-enable pickup
            return;
        }
        
        // Heal player
        Debug.Log("HeartItem: Calling Heal(" + healAmount + ")");
        healthManager.Heal(healAmount);
        
        int newHealth = healthManager.GetCurrentHealth();
        Debug.Log("Player healed by: " + healAmount + " HP. New health: " + newHealth);
        
        // Play pickup mouth animation
        MouthAnimation mouthAnimation = playerObj.GetComponent<MouthAnimation>();
        if (mouthAnimation != null)
        {
            mouthAnimation.PlayPickupMouthAnimation();
        }
        
        // Play pickup sound
        PlayPickupSound();
        
        // Destroy the item after sound finishes (if sound is playing)
        if (destroyAfterPickup)
        {
            if (playSound && pickupSound != null)
            {
                Destroy(gameObject, pickupSound.length);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
    
    public void PlayPickupSound()
    {
        // Ensure AudioSource is initialized
        if (audioSource == null)
        {
            InitializeAudioSource();
        }
        
        // Play pickup sound
        if (playSound && audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound, soundVolume);
            Debug.Log("Playing heart pickup sound: " + pickupSound.name);
        }
    }
    
    public void SetPickupEnabled(bool enabled)
    {
        canPickup = enabled;
        Debug.Log("HeartItem: Pickup " + (enabled ? "enabled" : "disabled"));
    }
}
