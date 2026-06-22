using UnityEngine;

public class CanvasResponsive : MonoBehaviour
{
    [Header("Canvas Settings")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private GraphicRaycaster graphicRaycaster;
    
    private void Start()
    {
        // Get Canvas component if not assigned
        if (canvas == null)
            canvas = GetComponent<Canvas>();
        
        if (canvasScaler == null)
            canvasScaler = GetComponent<CanvasScaler>();
        
        if (graphicRaycaster == null)
            graphicRaycaster = GetComponent<GraphicRaycaster>();
        
        // Configure Canvas
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
        
        // Configure CanvasScaler for responsive design
        if (canvasScaler != null)
        {
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1080, 1920); // Mobile reference (9:16 aspect ratio)
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f; // Balance between width and height
        }
        
        // Make sure GraphicRaycaster is enabled for UI interactions
        if (graphicRaycaster != null)
            graphicRaycaster.enabled = true;
        
        Debug.Log("Canvas configured for responsive mobile and desktop support");
    }
}
