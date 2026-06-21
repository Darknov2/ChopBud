using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0, 2, -10);
    [SerializeField] private bool smoothFollow = true;
    
    private Vector3 targetPosition;
    private Camera cameraComponent;
    
    private void Start()
    {
        // Find player if not assigned
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }
        
        if (playerTransform == null)
        {
            Debug.LogError("Player not found! Make sure player has 'Player' tag or assign it manually.");
            return;
        }
        
        // Get camera component
        cameraComponent = GetComponent<Camera>();
        
        // Set initial position
        targetPosition = playerTransform.position + offset;
        transform.position = targetPosition;
        
        // Set camera rotation to be perfectly straight
        transform.rotation = Quaternion.identity;
    }
    
    private void LateUpdate()
    {
        if (playerTransform == null)
            return;
        
        // Calculate target position
        targetPosition = playerTransform.position + offset;
        
        // Smooth follow
        if (smoothFollow)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = targetPosition;
        }
        
        // Keep rotation fixed at identity (straight)
        transform.rotation = Quaternion.identity;
    }
}
