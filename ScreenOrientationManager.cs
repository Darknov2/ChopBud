using UnityEngine;
using UnityEngine.UI;

public class ScreenOrientationManager : MonoBehaviour
{
    [Header("Orientation Settings")]
    [SerializeField] private ScreenOrientation allowedOrientation = ScreenOrientation.AutoRotation;
    [SerializeField] private bool allowPortrait = true;
    [SerializeField] private bool allowLandscape = false;
    [SerializeField] private bool autoRotate = true;
    [SerializeField] private float rotationCheckInterval = 0.5f;
    
    [Header("Canvas Settings")]
    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private Canvas canvas;
    
    private float lastRotationCheckTime = 0f;
    private ScreenOrientation lastOrientation;
    private int lastScreenWidth;
    private int lastScreenHeight;
    
    private void Start()
    {
        // Get components if not assigned
        if (canvasScaler == null)
            canvasScaler = FindObjectOfType<CanvasScaler>();
        
        if (canvas == null)
            canvas = FindObjectOfType<Canvas>();
        
        // Set allowed orientations
        SetOrientationMode();
        
        // Initialize screen tracking
        lastOrientation = Screen.orientation;
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        
        Debug.Log("Screen Orientation Manager initialized");
    }
    
    private void Update()
    {
        // Check for orientation changes periodically
        lastRotationCheckTime += Time.deltaTime;
        if (lastRotationCheckTime >= rotationCheckInterval)
        {
            lastRotationCheckTime = 0f;
            CheckOrientationChange();
        }
    }
    
    private void SetOrientationMode()
    {
        if (autoRotate)
        {
            // Allow auto rotation
            Screen.orientation = ScreenOrientation.AutoRotation;
            
            // Set which orientations are allowed
            if (allowPortrait && allowLandscape)
            {
                Screen.autorotateToPortrait = true;
                Screen.autorotateToPortraitUpsideDown = true;
                Screen.autorotateToLandscapeLeft = true;
                Screen.autorotateToLandscapeRight = true;
            }
            else if (allowPortrait)
            {
                Screen.autorotateToPortrait = true;
                Screen.autorotateToPortraitUpsideDown = true;
                Screen.autorotateToLandscapeLeft = false;
                Screen.autorotateToLandscapeRight = false;
            }
            else if (allowLandscape)
            {
                Screen.autorotateToPortrait = false;
                Screen.autorotateToPortraitUpsideDown = false;
                Screen.autorotateToLandscapeLeft = true;
                Screen.autorotateToLandscapeRight = true;
            }
        }
        else
        {
            // Lock orientation
            Screen.orientation = allowedOrientation;
        }
    }
    
    private void CheckOrientationChange()
    {
        // Check if screen size changed (indicates rotation)
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            
            OnOrientationChanged();
        }
    }
    
    private void OnOrientationChanged()
    {
        Debug.Log("Orientation changed! Width: " + Screen.width + " Height: " + Screen.height);
        
        // Update canvas scaler if assigned
        if (canvasScaler != null)
        {
            // Force layout rebuild
            LayoutRebuilder.ForceRebuildLayoutHierarchy(canvas.GetComponent<RectTransform>());
        }
        
        // Notify other scripts of orientation change
        SendMessage("OnScreenRotated", SendMessageOptions.DontRequireReceiver);
    }
    
    /// <summary>
    /// Enable/disable auto rotation at runtime
    /// </summary>
    public void SetAutoRotation(bool enabled)
    {
        autoRotate = enabled;
        SetOrientationMode();
    }
    
    /// <summary>
    /// Get current screen orientation
    /// </summary>
    public bool IsPortrait()
    {
        return Screen.height > Screen.width;
    }
    
    public bool IsLandscape()
    {
        return Screen.width > Screen.height;
    }
    
    /// <summary>
    /// Get aspect ratio
    /// </summary>
    public float GetAspectRatio()
    {
        return (float)Screen.width / Screen.height;
    }
}
