using UnityEngine;

public class TopDownPlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    
    [Header("Sprite Sorting")]
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private SpriteRenderer legRenderer;
    [SerializeField] private int bodySortingOrder = 1;
    [SerializeField] private int legsSortingOrder = 0;
    
    [Header("Animation")]
    [SerializeField] private Animator legAnimator;
    [SerializeField] private bool useAnimations = true;
    
    private Rigidbody2D rb;
    private Vector2 moveDirection = Vector2.zero;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component not found on " + gameObject.name);
        }
        
        // Set up sprite sorting order
        if (bodyRenderer != null)
        {
            bodyRenderer.sortingOrder = bodySortingOrder;
        }
        
        if (legRenderer != null)
        {
            legRenderer.sortingOrder = legsSortingOrder;
        }
        
        if (useAnimations && legAnimator == null)
        {
            // Try to find leg animator in children
            legAnimator = GetComponentInChildren<Animator>();
        }
    }
    
    private void Update()
    {
        HandleInput();
    }
    
    private void FixedUpdate()
    {
        Move();
    }
    
    private void HandleInput()
    {
        // Get input from arrow keys or WASD
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        
        // Create movement direction
        moveDirection = new Vector2(horizontalInput, verticalInput).normalized;
    }
    
    private void Move()
    {
        // Apply velocity directly based on input
        Vector2 velocity = moveDirection * moveSpeed;
        rb.velocity = velocity;
        
        // Update animations if enabled
        if (useAnimations && legAnimator != null)
        {
            UpdateAnimations();
        }
    }
    
    private void UpdateAnimations()
    {
        bool isMoving = moveDirection.magnitude > 0.01f;
        
        // Trigger leg walking animation
        legAnimator.SetBool("isMoving", isMoving);
        
        // Set direction for directional animations
        legAnimator.SetFloat("moveX", moveDirection.x);
        legAnimator.SetFloat("moveY", moveDirection.y);
        
        legAnimator.SetFloat("speed", moveDirection.magnitude * moveSpeed);
    }
    
    // Public method to get move direction
    public Vector2 GetMoveDirection()
    {
        return moveDirection;
    }
    
    // Public method to check if character is moving
    public bool IsMoving()
    {
        return moveDirection.magnitude > 0.01f;
    }
}
