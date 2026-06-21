using UnityEngine;
using System.Collections;

public class MouthAnimation : MonoBehaviour
{
    [Header("Mouth Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private float mouthAnimationDuration = 0.5f;
    [SerializeField] private bool debugMode = false;
    
    private bool isAnimating = false;
    
    private void Start()
    {
        // Find animator if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        if (animator == null)
        {
            Debug.LogError("Animator not found! Please assign it in the inspector or ensure the player has an Animator component.");
        }
    }
    
    /// <summary>
    /// Call this when the player teleports
    /// </summary>
    public void PlayTeleportMouthAnimation()
    {
        if (animator == null)
            return;
        
        if (debugMode)
            Debug.Log("Playing teleport mouth animation");
        
        StartCoroutine(MouthAnimationCoroutine());
    }
    
    /// <summary>
    /// Call this when the player picks up an item
    /// </summary>
    public void PlayPickupMouthAnimation()
    {
        if (animator == null)
            return;
        
        if (debugMode)
            Debug.Log("Playing pickup mouth animation");
        
        StartCoroutine(MouthAnimationCoroutine());
    }
    
    private IEnumerator MouthAnimationCoroutine()
    {
        if (isAnimating)
            yield break;
        
        isAnimating = true;
        
        // Play mouth open animation
        animator.SetTrigger("MouthOpen");
        
        // Wait for half the duration
        yield return new WaitForSeconds(mouthAnimationDuration / 2f);
        
        // Play mouth close animation
        animator.SetTrigger("MouthClose");
        
        // Wait for the remaining duration
        yield return new WaitForSeconds(mouthAnimationDuration / 2f);
        
        isAnimating = false;
    }
}
