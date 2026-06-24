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
        [Tooltip("Set to 0 to auto-calculate based on children, or set custom size")]
        public Vector2 size = Vector2.zero;
    }
    
    [SerializeField] private UIElement[] uiElements;
    [SerializeField] private Canvas canvas;
    [SerializeField] private bool debugMode = true;
    [SerializeField] private float orientationCheckInterval = 0.5f;
    
    private float lastCheckTime = 0f;
    private int lastScreenWidth;
    private int lastScreenHeight;
    
    private void Start()
    {
        if (canvas == null)
            canvas = FindObjectOfType<Canvas>();
        
        if (canvas == null)
        {
            Debug.LogError("Canvas not found! Make sure there is a Canvas in your scene.");
            return;
        }
        
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        
        // Position all UI elements on start
        PositionAllElements();
        
        Debug.Log("AutoPositionUI initialized. Canvas dimensions: " + canvas.GetComponent<RectTransform>().rect.size);
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
        
        if (canvas == null)
            return;
        
        // Get canvas rect
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;
        
        if (debugMode)
            Debug.Log("Positioning: " + uiElement.elementName + " | Canvas size: " + canvasWidth + " x " + canvasHeight);
        
        // Calculate size - if 0, auto-calculate from children
        Vector2 finalSize = uiElement.size;
        if (finalSize == Vector2.zero)
        {
            finalSize = CalculateSizeFromChildren(rectTransform);
        }
        
        // Ensure minimum size
        if (finalSize.x < 1) finalSize.x = 50;
        if (finalSize.y < 1) finalSize.y = 50;
        
        Vector2 newPosition = Vector2.zero;
        Vector2 anchorMin = Vector2.zero;
        Vector2 anchorMax = Vector2.zero;
        Vector2 pivot = Vector2.zero;
        
        // Calculate position, anchor, and pivot based on direction
        switch (uiElement.position)
        {
            // Top row
            case TextAnchor.UpperLeft:
                anchorMin = new Vector2(0, 1);
                anchorMax = new Vector2(0, 1);
                pivot = new Vector2(0, 1);
                newPosition = new Vector2(uiElement.paddingX, -uiElement.paddingY);
                break;
            
            case TextAnchor.UpperCenter:
                anchorMin = new Vector2(0.5f, 1);
                anchorMax = new Vector2(0.5f, 1);
                pivot = new Vector2(0.5f, 1);
                newPosition = new Vector2(0, -uiElement.paddingY);
                break;
            
            case TextAnchor.UpperRight:
                anchorMin = new Vector2(1, 1);
                anchorMax = new Vector2(1, 1);
                pivot = new Vector2(1, 1);
                newPosition = new Vector2(-uiElement.paddingX, -uiElement.paddingY);
                break;
            
            // Middle row
            case TextAnchor.MiddleLeft:
                anchorMin = new Vector2(0, 0.5f);
                anchorMax = new Vector2(0, 0.5f);
                pivot = new Vector2(0, 0.5f);
                newPosition = new Vector2(uiElement.paddingX, 0);
                break;
            
            case TextAnchor.MiddleCenter:
                anchorMin = new Vector2(0.5f, 0.5f);
                anchorMax = new Vector2(0.5f, 0.5f);
                pivot = new Vector2(0.5f, 0.5f);
                newPosition = Vector2.zero;
                break;
            
            case TextAnchor.MiddleRight:
                anchorMin = new Vector2(1, 0.5f);
                anchorMax = new Vector2(1, 0.5f);
                pivot = new Vector2(1, 0.5f);
                newPosition = new Vector2(-uiElement.paddingX, 0);
                break;
            
            // Bottom row
            case TextAnchor.LowerLeft:
                anchorMin = new Vector2(0, 0);
                anchorMax = new Vector2(0, 0);
                pivot = new Vector2(0, 0);
                newPosition = new Vector2(uiElement.paddingX, uiElement.paddingY);
                break;
            
            case TextAnchor.LowerCenter:
                anchorMin = new Vector2(0.5f, 0);
                anchorMax = new Vector2(0.5f, 0);
                pivot = new Vector2(0.5f, 0);
                newPosition = new Vector2(0, uiElement.paddingY);
                break;
            
            case TextAnchor.LowerRight:
                anchorMin = new Vector2(1, 0);
                anchorMax = new Vector2(1, 0);
                pivot = new Vector2(1, 0);
                newPosition = new Vector2(-uiElement.paddingX, uiElement.paddingY);
                break;
        }
        
        // Set anchor and pivot
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
        
        // Apply position
        rectTransform.anchoredPosition = newPosition;
        
        // Set size
        rectTransform.sizeDelta = finalSize;
        
        if (debugMode)
            Debug.Log(uiElement.elementName + " | Anchor: " + anchorMin + " | Position: " + newPosition + " | Size: " + finalSize);
    }
    
    /// <summary>
    /// Calculate the size needed to contain all child elements
    /// </summary>
    private Vector2 CalculateSizeFromChildren(RectTransform parent)
    {
        if (parent.childCount == 0)
            return new Vector2(100, 100);
        
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        
        foreach (Transform child in parent)
        {
            RectTransform childRect = child.GetComponent<RectTransform>();
            if (childRect == null) continue;
            
            Vector2 childSize = childRect.sizeDelta;
            Vector2 childPos = childRect.anchoredPosition;
            
            float childLeft = childPos.x - (childSize.x * childRect.pivot.x);
            float childRight = childPos.x + (childSize.x * (1 - childRect.pivot.x));
            float childBottom = childPos.y - (childSize.y * childRect.pivot.y);
            float childTop = childPos.y + (childSize.y * (1 - childRect.pivot.y));
            
            minX = Mathf.Min(minX, childLeft);
            maxX = Mathf.Max(maxX, childRight);
            minY = Mathf.Min(minY, childBottom);
            maxY = Mathf.Max(maxY, childTop);
        }
        
        float width = maxX - minX;
        float height = maxY - minY;
        
        if (width < 1) width = 100;
        if (height < 1) height = 100;
        
        if (debugMode)
            Debug.Log("Auto-calculated size for " + parent.name + ": " + width + " x " + height);
        
        return new Vector2(width, height);
    }
}
