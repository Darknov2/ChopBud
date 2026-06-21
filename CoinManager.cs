using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager instance;
    
    private int totalCoins = 0;
    
    private void Awake()
    {
        // Singleton pattern
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public void AddCoins(int amount)
    {
        totalCoins += amount;
        Debug.Log("Coins added: " + amount + " | Total: " + totalCoins);
    }
    
    public int GetTotalCoins()
    {
        return totalCoins;
    }
    
    public void ResetCoins()
    {
        totalCoins = 0;
    }
}
