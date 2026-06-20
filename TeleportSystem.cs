using UnityEngine;

public class TeleportSystem : MonoBehaviour
{
    [Header("Teleport Settings")]
    [SerializeField] private HealthManager healthManager;
    
    private Camera mainCamera;
    
    private void Start()
    {
        mainCamera = Camera.main;
        
        if (healthManager == null)
        {
            healthManager = GetComponent<HealthManager>();
        }
        
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
        }
    }
    
    private void Update()
    {
        // Check if player is dead
        if (healthManager != null && healthManager.IsDead())
            return;
        
        // Check for mouse click
        if (Input.GetMouseButtonDown(0))
        {
            TeleportToMouse();
        }
    }
    
    private void TeleportToMouse()
    {
        // Get mouse position in world coordinates
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f; // Distance from camera
        
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        
        // Teleport player to mouse position
        transform.position = new Vector3(worldPos.x, worldPos.y, transform.position.z);
        
        Debug.Log("Teleported to: " + worldPos);
    }
}
