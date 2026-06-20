using UnityEngine;
using System.Collections;

public class TeleportSystem : MonoBehaviour
{
    [Header("Teleport Settings")]
    [SerializeField] private HealthManager healthManager;
    
    [Header("Teleport Line Settings")]
    [SerializeField] private float lineFadeDuration = 0.5f;
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private Color lineColor = Color.white;
    
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
        
        // Store initial position
        Vector3 startPos = transform.position;
        
        // Teleport player to mouse position
        transform.position = new Vector3(worldPos.x, worldPos.y, transform.position.z);
        
        // Create teleport line effect
        StartCoroutine(DrawTeleportLine(startPos, worldPos));
        
        Debug.Log("Teleported to: " + worldPos);
    }
    
    private IEnumerator DrawTeleportLine(Vector3 startPos, Vector3 endPos)
    {
        // Create a new GameObject for the line
        GameObject lineObject = new GameObject("TeleportLine");
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
        
        // Setup LineRenderer
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        
        // Create material for the line
        Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material = lineMaterial;
        
        // Set initial color
        Color startColor = lineColor;
        lineRenderer.startColor = startColor;
        lineRenderer.endColor = startColor;
        
        // Fade out over time
        float elapsedTime = 0f;
        while (elapsedTime < lineFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float fadeProgress = elapsedTime / lineFadeDuration;
            
            // Lerp the alpha from 1 to 0
            Color fadeColor = startColor;
            fadeColor.a = Mathf.Lerp(1f, 0f, fadeProgress);
            
            lineRenderer.startColor = fadeColor;
            lineRenderer.endColor = fadeColor;
            
            yield return null;
        }
        
        // Destroy the line object
        Destroy(lineObject);
    }
}
