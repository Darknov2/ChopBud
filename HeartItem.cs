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
        
        Debug.Log("========== HEART PICKUP DEBUG START ==========");
        Debug.Log("HeartItem: PickupItem called on player: " + playerObj.name);
        
        canPickup = false;
        isPulling = false;
        
        // Get player health manager
        HealthManager healthManager = playerObj.GetComponent<HealthManager>();
        
        Debug.Log("Step 1 - HealthManager found: " + (healthManager != null ? "YES ✓" : "NO ✗"));
        
        if (healthManager == null)
        {
            Debug.LogError("FAILED: HealthManager is NULL!");
            Component[] components = playerObj.GetComponents<Component>();
            foreach (Component comp in components)
            {
                Debug.Log("  - Component: " + comp.GetType().Name);
            }
            return;
        }
        
        // Get health values BEFORE healing
        int currentHealthBefore = healthManager.GetCurrentHealth();
        int maxHealth = healthManager.GetMaxHealth();
        bool isDead = healthManager.IsDead();
        
        Debug.Log("Step 2 - Health Status BEFORE:");
        Debug.Log("  - Current Health: " + currentHealthBefore);
        Debug.Log("  - Max Health: " + maxHealth);
        Debug.Log("  - Is Dead: " + isDead);
        
        // Check if already at full health
        if (currentHealthBefore >= maxHealth)
        {
            Debug.Log("SKIPPED: Player already at full health!");
            canPickup = true;
            return;
        }
        
        // Check if player is dead
        if (isDead)
        {
            Debug.Log("SKIPPED: Player is dead!");
            canPickup = true;
            return;
        }
        
        // HEALING
        Debug.Log("Step 3 - Calling Heal(" + healAmount + ")...");
        healthManager.Heal(healAmount);
        
        // Get health values AFTER healing
        int currentHealthAfter = healthManager.GetCurrentHealth();
        Debug.Log("Step 4 - Health Status AFTER:");
        Debug.Log("  - Current Health: " + currentHealthAfter);
        Debug.Log("  - Health increased by: " + (currentHealthAfter - currentHealthBefore));
        
        if (currentHealthAfter > currentHealthBefore)
        {
            Debug.Log("SUCCESS: Player healed! ✓");
        }
        else
        {
            Debug.LogError("FAILED: Health did not increase!");
        }
        
        // Play animations and sounds
        MouthAnimation mouthAnimation = playerObj.GetComponent<MouthAnimation>();
        if (mouthAnimation != null)
        {
            mouthAnimation.PlayPickupMouthAnimation();
            Debug.Log("Mouth animation played");
        }
        
        PlayPickupSound();
        
        // Destroy item
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
        
        Debug.Log("========== HEART PICKUP DEBUG END ==========");
    }
    
    public void PlayPickupSound()
    {
        if (audioSource == null)
        {
            InitializeAudioSource();
        }
        
        if (playSound && audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound, soundVolume);
            Debug.Log("Playing heart pickup sound");
        }
    }
    
    public void SetPickupEnabled(bool enabled)
    {
        canPickup = enabled;
        Debug.Log("HeartItem: Pickup " + (enabled ? "ENABLED ✓" : "DISABLED ✗"));
    }
}
