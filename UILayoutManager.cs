using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class UIElementPosition
{
    public string elementName;
    public RectTransform element;
    
    [Header("Portrait Mode")]
    public Vector2 portraitPosition;
    public Vector2 portraitScale = Vector2.one;
    
    [Header("Landscape Mode")]
    public Vector2 landscapePosition;
    public Vector2 landscapeScale = Vector2.one;
}

public class UILayoutManager : MonoBehaviour
{
    [SerializeField] private UIElementPosition[] uiElements;
    [SerializeField] private bool debugMode = false;
    [SerializeField] private float orientationCheckInterval = 0.5f;
    
    private float lastCheckTime = 0f;
    private bool isPortrait = true;
    private int lastScreenWidth;
    private int lastScreenHeight;
    
    private void Start()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        isPortrait = Screen.height > Screen.width;
        
        // Apply initial layout
        ApplyLayout();
    }
    
    private void Update()
    {
        // Check for orientation changes periodically
        lastCheckTime += Time.deltaTime;
        if (lastCheckTime >= orientationCheckInterval)
        {
            lastCheckTime = 0f;
            
            // Check if screen size changed
            if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
            {
                lastScreenWidth = Screen.width;
                lastScreenHeight = Screen.height;
                
                bool wasPortrait = isPortrait;
                isPortrait = Screen.height > Screen.width;
                
                if (wasPortrait != isPortrait)
                {
                    if (debugMode)
                        Debug.Log("Orientation changed! Portrait: " + isPortrait);
                    
                    ApplyLayout();
                }
            }
        }
    }
    
    private void ApplyLayout()
    {
        foreach (UIElementPosition uiElement in uiElements)
        {
            if (uiElement.element == null)
            {
                Debug.LogWarning("UI Element '" + uiElement.elementName + "' is not assigned!");
                continue;
            }
            
            if (isPortrait)
            {
                // Apply portrait settings
                uiElement.element.anchoredPosition = uiElement.portraitPosition;
                uiElement.element.localScale = uiElement.portraitScale;
                
                if (debugMode)
                    Debug.Log("Applied portrait layout to: " + uiElement.elementName);
            }
            else
            {
                // Apply landscape settings
                uiElement.element.anchoredPosition = uiElement.landscapePosition;
                uiElement.element.localScale = uiElement.landscapeScale;
                
                if (debugMode)
                    Debug.Log("Applied landscape layout to: " + uiElement.elementName);
            }
        }
    }
    
    /// <summary>
    /// Get current orientation
    /// </summary>
    public bool IsPortrait()
    {
        return isPortrait;
    }
    
    /// <summary>
    /// Manually refresh layout
    /// </summary>
    public void RefreshLayout()
    {
        ApplyLayout();
    }
}
