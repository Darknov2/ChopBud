using UnityEngine;

public class EnemyFollowAI : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float followSpeed = 3f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float stoppingDistance = 0.5f;
    
    [Header("Sprite Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    private Rigidbody2D rb;
    private Vector2 moveDirection = Vector2.zero;
    private bool isFollowing = false;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component not found on " + gameObject.name);
        }
        
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        if (playerTransform == null)
        {
            Debug.LogError("Player transform not assigned to " + gameObject.name);
        }
    }
    
    private void Update()
    {
        if (playerTransform == null)
            return;
        
        CheckPlayerDistance();
        UpdateAnimation();
    }
    
    private void FixedUpdate()
    {
        if (playerTransform == null || !isFollowing)
            return;
        
        Follow();
    }
    
    private void CheckPlayerDistance()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        isFollowing = distanceToPlayer < detectionRange;
    }
    
    private void Follow()
    {
        // Calculate direction to player
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        // Stop if within stopping distance
        if (distanceToPlayer > stoppingDistance)
        {
            moveDirection = directionToPlayer * followSpeed;
        }
        else
        {
            moveDirection = Vector2.zero;
        }
        
        // Apply velocity
        rb.velocity = moveDirection;
        
        // Flip sprite based on movement direction
        FlipSprite(directionToPlayer);
    }
    
    private void FlipSprite(Vector2 direction)
    {
        if (direction.x > 0)
        {
            // Moving right
            transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
        }
        else if (direction.x < 0)
        {
            // Moving left
            transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
        }
    }
    
    private void UpdateAnimation()
    {
        // You can add animation support here later if needed
    }
    
    // Public method to set player transform
    public void SetPlayerTransform(Transform player)
    {
        playerTransform = player;
    }
    
    // Public method to check if enemy is following
    public bool IsFollowing()
    {
        return isFollowing;
    }
}
