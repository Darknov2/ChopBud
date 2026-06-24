using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;
    
    [Header("Input Settings")]
    [SerializeField] private bool debugMode = false;
    
    private Vector3 lastTouchPosition = Vector3.zero;
    private Vector3 lastMousePosition = Vector3.zero;
    private bool isTouching = false;
    
    public delegate void OnInputAction();
    public event OnInputAction OnTeleportInput;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        #if UNITY_ANDROID || UNITY_IOS
            HandleTouchInput();
        #else
            HandleDesktopInput();
        #endif
    }
    
    private void HandleTouchInput()
    {
        // Check for touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    isTouching = true;
                    lastTouchPosition = touch.position;
                    if (debugMode)
                        Debug.Log("Touch started at: " + touch.position);
                    break;
                
                case TouchPhase.Moved:
                    lastTouchPosition = touch.position;
                    break;
                
                case TouchPhase.Ended:
                    isTouching = false;
                    HandleTouchEnd(touch.position);
                    break;
                
                case TouchPhase.Canceled:
                    isTouching = false;
                    break;
            }
        }
    }
    
    private void HandleTouchEnd(Vector3 touchPos)
    {
        // Trigger teleport on touch release
        OnTeleportInput?.Invoke();
        if (debugMode)
            Debug.Log("Touch ended - Teleport triggered at: " + touchPos);
    }
    
    private void HandleDesktopInput()
    {
        // Check for mouse click (left button)
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;
            if (debugMode)
                Debug.Log("Mouse click at: " + Input.mousePosition);
        }
        
        // Check for mouse button up
        if (Input.GetMouseButtonUp(0))
        {
            OnTeleportInput?.Invoke();
            if (debugMode)
                Debug.Log("Mouse released - Teleport triggered");
        }
    }
    
    /// <summary>
    /// Get the current input position in screen space
    /// </summary>
    public Vector3 GetInputPosition()
    {
        #if UNITY_ANDROID || UNITY_IOS
            return lastTouchPosition;
        #else
            return Input.mousePosition;
        #endif
    }
    
    /// <summary>
    /// Check if currently touching/clicking
    /// </summary>
    public bool IsInputActive()
    {
        #if UNITY_ANDROID || UNITY_IOS
            return isTouching;
        #else
            return Input.GetMouseButton(0);
        #endif
    }
    
    /// <summary>
    /// Get input in world coordinates
    /// </summary>
    public Vector3 GetInputWorldPosition(Camera camera = null)
    {
        if (camera == null)
            camera = Camera.main;
        
        Vector3 screenPos = GetInputPosition();
        screenPos.z = 10f;
        return camera.ScreenToWorldPoint(screenPos);
    }
}
