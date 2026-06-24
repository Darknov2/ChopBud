using UnityEngine;
using UnityEngine.UI;

public class AutoPositionUI : MonoBehaviour
{
    [System.Serializable]
    public class UIElement
    {
        public string elementName;
        public RectTransform element;
        [Tooltip("Top-Left, Top-Center, Top-Right, Center-Left, Center, Center-Right, Bottom-Left, Bottom-Center, Bottom-Right")]
        public TextAnchor position = TextAnchor.MiddleCenter;
        
        [Header("Padding")]
        [Tooltip("Distance from screen edge")]
        public float paddingX = 20f;
        public float paddingY = 20f;
        
        [Header("Size")]
        public Vector2 size = new Vector2(100, 100);
    }
    
    [SerializeField] private UIElement[] uiElements;
    [SerializeField] private Canvas canvas;
    [SerializeField] private bool debugMode = false;
    [SerializeField] private float orientationCheckInterval = 0.5f;
    
    private float lastCheckTime = 0f;
    private int lastScreenWidth;
    private int lastScreenHeight;
    
    private void Start()
    {
        if (canvas == null)
            canvas = FindObjectOfType<Canvas>();
        
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        
        // Position all UI elements on start
        PositionAllElements();
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
                
                if (debugMode)
                    Debug.Log("Screen size changed! Repositioning UI elements.");
                
                PositionAllElements();
            }
        }
    }
    
    private void PositionAllElements()
    {
        foreach (UIElement uiElement in uiElements)
        {
            if (uiElement.element == null)
            {
                Debug.LogWarning("UI Element '" + uiElement.elementName + "' is not assigned!");
                continue;
            }
            
            PositionElement(uiElement);
        }
    }
    
    private void PositionElement(UIElement uiElement)
    {
        RectTransform rectTransform = uiElement.element;
        
        // Get canvas rect
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;
        
        Vector2 newPosition = Vector2.zero;
        
        // Calculate position based on anchor
        switch (uiElement.position)
        {
            // Top row
            case TextAnchor.UpperLeft:
                newPosition = new Vector2(-canvasWidth / 2 + uiElement.paddingX + uiElement.size.x / 2, 
                                         canvasHeight / 2 - uiElement.paddingY - uiElement.size.y / 2);
                break;
            
            case TextAnchor.UpperCenter:
                newPosition = new Vector2(0, 
                                         canvasHeight / 2 - uiElement.paddingY - uiElement.size.y / 2);
                break;
            
            case TextAnchor.UpperRight:
                newPosition = new Vector2(canvasWidth / 2 - uiElement.paddingX - uiElement.size.x / 2, 
                                         canvasHeight / 2 - uiElement.paddingY - uiElement.size.y / 2);
                break;
            
            // Middle row
            case TextAnchor.MiddleLeft:
                newPosition = new Vector2(-canvasWidth / 2 + uiElement.paddingX + uiElement.size.x / 2, 0);
                break;
            
            case TextAnchor.MiddleCenter:
                newPosition = Vector2.zero;
                break;
            
            case TextAnchor.MiddleRight:
                newPosition = new Vector2(canvasWidth / 2 - uiElement.paddingX - uiElement.size.x / 2, 0);
                break;
            
            // Bottom row
            case TextAnchor.LowerLeft:
                newPosition = new Vector2(-canvasWidth / 2 + uiElement.paddingX + uiElement.size.x / 2, 
                                         -canvasHeight / 2 + uiElement.paddingY + uiElement.size.y / 2);
                break;
            
            case TextAnchor.LowerCenter:
                newPosition = new Vector2(0, 
                                         -canvasHeight / 2 + uiElement.paddingY + uiElement.size.y / 2);
                break;
            
            case TextAnchor.LowerRight:
                newPosition = new Vector2(canvasWidth / 2 - uiElement.paddingX - uiElement.size.x / 2, 
                                         -canvasHeight / 2 + uiElement.paddingY + uiElement.size.y / 2);
                break;
        }
        
        // Apply position
        rectTransform.anchoredPosition = newPosition;
        
        // Set size if it has a LayoutElement, otherwise set directly
        LayoutElement layoutElement = rectTransform.GetComponent<LayoutElement>();
        if (layoutElement != null)
        {
            layoutElement.preferredWidth = uiElement.size.x;
            layoutElement.preferredHeight = uiElement.size.y;
        }
        else
        {
            rectTransform.sizeDelta = uiElement.size;
        }
        
        if (debugMode)
            Debug.Log(uiElement.elementName + " positioned at: " + newPosition);
    }
}
