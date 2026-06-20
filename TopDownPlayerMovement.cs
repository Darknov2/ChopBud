using UnityEngine;

public class TopDownPlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 15f;
    
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private bool useAnimations = true;
    
    private Rigidbody2D rb;
    private Vector2 moveDirection = Vector2.zero;
    private Vector2 currentVelocity = Vector2.zero;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component not found on " + gameObject.name);
        }
        
        if (useAnimations && animator == null)
        {
            animator = GetComponent<Animator>();
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
        if (moveDirection.magnitude > 0)
        {
            // Accelerate towards target speed
            currentVelocity = Vector2.Lerp(
                currentVelocity,
                moveDirection * moveSpeed,
                acceleration * Time.fixedDeltaTime
            );
        }
        else
        {
            // Decelerate to stop
            currentVelocity = Vector2.Lerp(
                currentVelocity,
                Vector2.zero,
                deceleration * Time.fixedDeltaTime
            );
        }
        
        // Apply velocity to Rigidbody2D
        rb.velocity = currentVelocity;
        
        // Update animations if enabled
        if (useAnimations && animator != null)
        {
            UpdateAnimations();
        }
    }
    
    private void UpdateAnimations()
    {
        // Set movement parameters for animation
        animator.SetFloat("moveX", currentVelocity.x);
        animator.SetFloat("moveY", currentVelocity.y);
        animator.SetFloat("speed", currentVelocity.magnitude);
        
        // Optional: Set last facing direction when idle
        if (currentVelocity.magnitude > 0.01f)
        {
            animator.SetFloat("lastX", currentVelocity.x);
            animator.SetFloat("lastY", currentVelocity.y);
        }
    }
    
    // Public method to get current velocity
    public Vector2 GetCurrentVelocity()
    {
        return currentVelocity;
    }
    
    // Public method to get move direction
    public Vector2 GetMoveDirection()
    {
        return moveDirection;
    }
    
    // Public method to check if character is moving
    public bool IsMoving()
    {
        return currentVelocity.magnitude > 0.01f;
    }
}
