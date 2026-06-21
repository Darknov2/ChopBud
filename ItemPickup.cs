using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float pickupDelay = 0.1f;
    
    [Header("Sound Settings")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float soundVolume = 1f;
    [SerializeField] private bool playSound = true;
    
    [Header("Coin Settings")]
    [SerializeField] private int coinAmount = 10;
    [SerializeField] private bool givesCoins = true;
    
    private bool canPickup = true;
    private Collider2D itemCollider;
    private AudioSource audioSource;
    
    private void Start()
    {
        itemCollider = GetComponent<Collider2D>();
        
        if (itemCollider == null)
        {
            Debug.LogWarning("ItemPickup: No Collider2D found on " + gameObject.name);
        }
        
        // Create AudioSource if playSound is enabled
        if (playSound && pickupSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = pickupSound;
            audioSource.volume = soundVolume;
            audioSource.playOnAwake = false;
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
        
        // Add coins to player
        if (givesCoins && CoinManager.instance != null)
        {
            CoinManager.instance.AddCoins(coinAmount);
        }
        
        // Play pickup sound
        if (playSound && audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound, soundVolume);
        }
        
        // Add your inventory logic here
        // Example: player.GetComponent<Inventory>().AddItem(this);
        
        // Destroy the item after sound finishes (if sound is playing)
        if (playSound && pickupSound != null)
        {
            Destroy(gameObject, pickupSound.length);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void SetPickupEnabled(bool enabled)
    {
        canPickup = enabled;
    }
    
    public void SetPickupSound(AudioClip clip)
    {
        pickupSound = clip;
    }
    
    public void SetSoundVolume(float volume)
    {
        soundVolume = Mathf.Clamp01(volume);
        if (audioSource != null)
        {
            audioSource.volume = soundVolume;
        }
    }
    
    public void SetCoinAmount(int amount)
    {
        coinAmount = amount;
    }
}
