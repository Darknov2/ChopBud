using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CoinUI : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private Image coinImage;
    [SerializeField] private Canvas uiCanvas;
    
    private CoinManager coinManager;
    
    private void Start()
    {
        coinManager = CoinManager.instance;
        
        if (coinManager == null)
        {
            Debug.LogError("CoinUI: CoinManager not found!");
            return;
        }
        
        // If no canvas assigned, find one in scene
        if (uiCanvas == null)
        {
            uiCanvas = FindObjectOfType<Canvas>();
        }
        
        UpdateCoinDisplay();
    }
    
    private void Update()
    {
        UpdateCoinDisplay();
    }
    
    public void UpdateCoinDisplay()
    {
        if (coinManager != null && coinText != null)
        {
            coinText.text = coinManager.GetTotalCoins().ToString();
        }
    }
}
