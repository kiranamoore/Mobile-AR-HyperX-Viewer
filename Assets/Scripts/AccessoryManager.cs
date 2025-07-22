using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages earcap accessories with animations and state control
/// Optimized for WebGL performance
/// </summary>
public class AccessoryManager : MonoBehaviour
{
    [Header("Accessory References")]
    [SerializeField] private GameObject defaultEarcap;
    [SerializeField] private GameObject natureEarcap;
    [SerializeField] private GameObject valkyrieEarcap;
    [SerializeField] private GameObject werewolfEarcap;
    [SerializeField] private GameObject defaultEarcapLeft;
    [SerializeField] private GameObject defaultEarcapRight;
    [SerializeField] private GameObject natureEarcapLeft;
    [SerializeField] private GameObject natureEarcapRight;
    [SerializeField] private GameObject valkyrieEarcapLeft;
    [SerializeField] private GameObject valkyrieEarcapRight;
    [SerializeField] private GameObject werewolfEarcapLeft;
    [SerializeField] private GameObject werewolfEarcapRight;
    [SerializeField] private GameObject natureHeadpieceLeft;
    [SerializeField] private GameObject natureHeadpieceRight;
    [SerializeField] private GameObject valkyrieHeadpieceLeft;
    [SerializeField] private GameObject valkyrieHeadpieceRight;
    [SerializeField] private GameObject werewolfHeadpieceLeft;
    [SerializeField] private GameObject werewolfHeadpieceRight;
    
    [Header("Animation Settings")]
    [SerializeField] private float swapAnimationDuration = 0.5f;
    [SerializeField] private AnimationCurve swapCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip swapSound;
    
    // Private fields
    private Dictionary<AccessoryType, GameObject> accessories;
    private AccessoryType currentAccessory = AccessoryType.Default;
    private bool isSwapping = false;
    private bool isDisabled = false;
    private Vector3 defaultLeftPosition;
    private Vector3 defaultRightPosition;
    private Vector3 natureLeftPosition;
    private Vector3 natureRightPosition;
    private Vector3 valkyrieLeftPosition;
    private Vector3 valkyrieRightPosition;
    private Vector3 werewolfLeftPosition;
    private Vector3 werewolfRightPosition;
    private Vector3 natureHeadpieceLeftPosition;
    private Vector3 natureHeadpieceRightPosition;
    private Vector3 valkyrieHeadpieceLeftPosition;
    private Vector3 valkyrieHeadpieceRightPosition;
    private Vector3 werewolfHeadpieceLeftPosition;
    private Vector3 werewolfHeadpieceRightPosition;
    
    // Events
    public System.Action<AccessoryType> OnAccessoryChanged;
    
    // Accessory types enum
    public enum AccessoryType
    {
        Default,
        Nature,
        Valkyrie,
        Werewolf
    }
    
    private void Awake()
    {
        InitializeAccessories();
        if (defaultEarcapLeft != null) defaultLeftPosition = defaultEarcapLeft.transform.localPosition;
        if (defaultEarcapRight != null) defaultRightPosition = defaultEarcapRight.transform.localPosition;
        if (natureEarcapLeft != null) natureLeftPosition = natureEarcapLeft.transform.localPosition;
        if (natureEarcapRight != null) natureRightPosition = natureEarcapRight.transform.localPosition;
        if (valkyrieEarcapLeft != null) valkyrieLeftPosition = valkyrieEarcapLeft.transform.localPosition;
        if (valkyrieEarcapRight != null) valkyrieRightPosition = valkyrieEarcapRight.transform.localPosition;
        if (werewolfEarcapLeft != null) werewolfLeftPosition = werewolfEarcapLeft.transform.localPosition;
        if (werewolfEarcapRight != null) werewolfRightPosition = werewolfEarcapRight.transform.localPosition;
        if (natureHeadpieceLeft != null) natureHeadpieceLeftPosition = natureHeadpieceLeft.transform.localPosition;
        if (natureHeadpieceRight != null) natureHeadpieceRightPosition = natureHeadpieceRight.transform.localPosition;
        if (valkyrieHeadpieceLeft != null) valkyrieHeadpieceLeftPosition = valkyrieHeadpieceLeft.transform.localPosition;
        if (valkyrieHeadpieceRight != null) valkyrieHeadpieceRightPosition = valkyrieHeadpieceRight.transform.localPosition;
        if (werewolfHeadpieceLeft != null) werewolfHeadpieceLeftPosition = werewolfHeadpieceLeft.transform.localPosition;
        if (werewolfHeadpieceRight != null) werewolfHeadpieceRightPosition = werewolfHeadpieceRight.transform.localPosition;
    }
    
    private void Start()
    {
        // Set default accessory
        SetAccessory(AccessoryType.Default, false);
    }
    
    /// <summary>
    /// Initialize the accessories dictionary
    /// </summary>
    private void InitializeAccessories()
    {
        accessories = new Dictionary<AccessoryType, GameObject>
        {
            { AccessoryType.Default, defaultEarcap },
            { AccessoryType.Nature, natureEarcap },
            { AccessoryType.Valkyrie, valkyrieEarcap },
            { AccessoryType.Werewolf, werewolfEarcap }
        };

        // Ensure all accessories are initially disabled
        foreach (var accessory in accessories.Values)
        {
            if (accessory != null)
            {
                accessory.SetActive(false);
            }
        }

        // Enable only the Default accessory (and its left/right if used)
        if (defaultEarcap != null) defaultEarcap.SetActive(true);
        if (defaultEarcapLeft != null) defaultEarcapLeft.SetActive(true);
        if (defaultEarcapRight != null) defaultEarcapRight.SetActive(true);
    }
    
    /// <summary>
    /// Set the current accessory with optional animation
    /// </summary>
    /// <param name="accessoryType">Type of accessory to set</param>
    /// <param name="animate">Whether to animate the transition</param>
    public void SetAccessory(AccessoryType accessoryType, bool animate = true)
    {
        if (isSwapping || isDisabled || !accessories.ContainsKey(accessoryType))
            return;
            
        if (animate)
        {
            StartCoroutine(SwapAccessoryCoroutine(accessoryType));
        }
        else
        {
            ApplyAccessory(accessoryType);
        }
    }
    
    /// <summary>
    /// Coroutine to handle smooth accessory swapping with animation
    /// </summary>
    private System.Collections.IEnumerator SwapAccessoryCoroutine(AccessoryType newAccessory)
    {
        isSwapping = true;
        
        // Play swap sound
        if (audioSource != null && swapSound != null)
        {
            audioSource.PlayOneShot(swapSound);
        }
        
        // Fade out current accessory
        GameObject currentAccessoryObj = accessories[currentAccessory];
        if (currentAccessoryObj != null)
        {
            yield return StartCoroutine(FadeAccessory(currentAccessoryObj, false));
        }
        
        // Apply new accessory
        ApplyAccessory(newAccessory);
        
        // Fade in new accessory
        GameObject newAccessoryObj = accessories[newAccessory];
        if (newAccessoryObj != null)
        {
            yield return StartCoroutine(FadeAccessory(newAccessoryObj, true));
        }
        
        isSwapping = false;
    }
    
    /// <summary>
    /// Apply accessory without animation
    /// </summary>
    private void ApplyAccessory(AccessoryType accessoryType)
    {
        // Disable all accessories
        foreach (var accessory in accessories.Values)
        {
            if (accessory != null)
            {
                accessory.SetActive(false);
            }
        }

        // Hide all left/right earcaps and headpieces
        if (defaultEarcapLeft != null) defaultEarcapLeft.SetActive(false);
        if (defaultEarcapRight != null) defaultEarcapRight.SetActive(false);
        if (natureEarcapLeft != null) natureEarcapLeft.SetActive(false);
        if (natureEarcapRight != null) natureEarcapRight.SetActive(false);
        if (valkyrieEarcapLeft != null) valkyrieEarcapLeft.SetActive(false);
        if (valkyrieEarcapRight != null) valkyrieEarcapRight.SetActive(false);
        if (werewolfEarcapLeft != null) werewolfEarcapLeft.SetActive(false);
        if (werewolfEarcapRight != null) werewolfEarcapRight.SetActive(false);
        if (natureHeadpieceLeft != null) natureHeadpieceLeft.SetActive(false);
        if (natureHeadpieceRight != null) natureHeadpieceRight.SetActive(false);
        if (valkyrieHeadpieceLeft != null) valkyrieHeadpieceLeft.SetActive(false);
        if (valkyrieHeadpieceRight != null) valkyrieHeadpieceRight.SetActive(false);
        if (werewolfHeadpieceLeft != null) werewolfHeadpieceLeft.SetActive(false);
        if (werewolfHeadpieceRight != null) werewolfHeadpieceRight.SetActive(false);

        // Special logic for each set
        switch (accessoryType)
        {
            case AccessoryType.Default:
                if (defaultEarcap != null) defaultEarcap.SetActive(true);
                if (defaultEarcapLeft != null) defaultEarcapLeft.SetActive(true);
                if (defaultEarcapRight != null) defaultEarcapRight.SetActive(true);
                if (defaultEarcapLeft != null && defaultEarcapRight != null)
                    StartCoroutine(AnimateEarcaps(defaultEarcapLeft, defaultEarcapRight, defaultLeftPosition, defaultRightPosition));
                break;
            case AccessoryType.Nature:
                if (accessories[accessoryType] != null) accessories[accessoryType].SetActive(true);
                if (natureEarcapLeft != null) natureEarcapLeft.SetActive(true);
                if (natureEarcapRight != null) natureEarcapRight.SetActive(true);
                if (natureEarcapLeft != null && natureEarcapRight != null)
                    StartCoroutine(AnimateEarcaps(natureEarcapLeft, natureEarcapRight, natureLeftPosition, natureRightPosition));
                if (natureHeadpieceLeft != null) natureHeadpieceLeft.SetActive(true);
                if (natureHeadpieceRight != null) natureHeadpieceRight.SetActive(true);
                if (natureHeadpieceLeft != null && natureHeadpieceRight != null)
                    StartCoroutine(AnimateEarcaps(natureHeadpieceLeft, natureHeadpieceRight, natureHeadpieceLeftPosition, natureHeadpieceRightPosition));
                break;
            case AccessoryType.Valkyrie:
                if (accessories[accessoryType] != null) accessories[accessoryType].SetActive(true);
                if (valkyrieEarcapLeft != null) valkyrieEarcapLeft.SetActive(true);
                if (valkyrieEarcapRight != null) valkyrieEarcapRight.SetActive(true);
                if (valkyrieEarcapLeft != null && valkyrieEarcapRight != null)
                    StartCoroutine(AnimateEarcaps(valkyrieEarcapLeft, valkyrieEarcapRight, valkyrieLeftPosition, valkyrieRightPosition));
                if (valkyrieHeadpieceLeft != null) valkyrieHeadpieceLeft.SetActive(true);
                if (valkyrieHeadpieceRight != null) valkyrieHeadpieceRight.SetActive(true);
                if (valkyrieHeadpieceLeft != null && valkyrieHeadpieceRight != null)
                    StartCoroutine(AnimateEarcaps(valkyrieHeadpieceLeft, valkyrieHeadpieceRight, valkyrieHeadpieceLeftPosition, valkyrieHeadpieceRightPosition));
                break;
            case AccessoryType.Werewolf:
                if (accessories[accessoryType] != null) accessories[accessoryType].SetActive(true);
                if (werewolfEarcapLeft != null) werewolfEarcapLeft.SetActive(true);
                if (werewolfEarcapRight != null) werewolfEarcapRight.SetActive(true);
                if (werewolfEarcapLeft != null && werewolfEarcapRight != null)
                    StartCoroutine(AnimateEarcaps(werewolfEarcapLeft, werewolfEarcapRight, werewolfLeftPosition, werewolfRightPosition));
                if (werewolfHeadpieceLeft != null) werewolfHeadpieceLeft.SetActive(true);
                if (werewolfHeadpieceRight != null) werewolfHeadpieceRight.SetActive(true);
                if (werewolfHeadpieceLeft != null && werewolfHeadpieceRight != null)
                    StartCoroutine(AnimateEarcaps(werewolfHeadpieceLeft, werewolfHeadpieceRight, werewolfHeadpieceLeftPosition, werewolfHeadpieceRightPosition));
                break;
        }

        currentAccessory = accessoryType;
        OnAccessoryChanged?.Invoke(accessoryType);
    }
    
    /// <summary>
    /// Fade accessory in or out
    /// </summary>
    private System.Collections.IEnumerator FadeAccessory(GameObject accessory, bool fadeIn)
    {
        Renderer[] renderers = accessory.GetComponentsInChildren<Renderer>();
        CanvasGroup[] canvasGroups = accessory.GetComponentsInChildren<CanvasGroup>();
        
        float elapsedTime = 0f;
        float startAlpha = fadeIn ? 0f : 1f;
        float targetAlpha = fadeIn ? 1f : 0f;
        
        // Set initial alpha
        SetAlpha(renderers, canvasGroups, startAlpha);
        
        while (elapsedTime < swapAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / swapAnimationDuration;
            float curveValue = swapCurve.Evaluate(normalizedTime);
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, curveValue);
            
            SetAlpha(renderers, canvasGroups, currentAlpha);
            
            yield return null;
        }
        
        // Set final alpha
        SetAlpha(renderers, canvasGroups, targetAlpha);
        
        // Disable if faded out
        if (!fadeIn)
        {
            accessory.SetActive(false);
        }
    }
    
    /// <summary>
    /// Set alpha for renderers and canvas groups
    /// </summary>
    private void SetAlpha(Renderer[] renderers, CanvasGroup[] canvasGroups, float alpha)
    {
        // Set material alpha
        foreach (var renderer in renderers)
        {
            if (renderer.material.HasProperty("_Color"))
            {
                Color color = renderer.material.color;
                color.a = alpha;
                renderer.material.color = color;
            }
        }
        
        // Set canvas group alpha
        foreach (var canvasGroup in canvasGroups)
        {
            canvasGroup.alpha = alpha;
        }
    }
    
    /// <summary>
    /// Enable or disable accessories (used during feature interactions)
    /// </summary>
    /// <param name="enabled">Whether accessories should be enabled</param>
    public void SetAccessoriesEnabled(bool enabled)
    {
        isDisabled = !enabled;
        
        foreach (var accessory in accessories.Values)
        {
            if (accessory != null)
            {
                accessory.SetActive(enabled && accessory == accessories[currentAccessory]);
            }
        }
    }
    
    /// <summary>
    /// Get current accessory type
    /// </summary>
    public AccessoryType GetCurrentAccessory()
    {
        return currentAccessory;
    }
    
    /// <summary>
    /// Check if accessories are currently disabled
    /// </summary>
    public bool AreAccessoriesDisabled()
    {
        return isDisabled;
    }
    
    /// <summary>
    /// Check if currently swapping accessories
    /// </summary>
    public bool IsSwapping()
    {
        return isSwapping;
    }
    
    /// <summary>
    /// Get all available accessory types
    /// </summary>
    public AccessoryType[] GetAvailableAccessories()
    {
        return System.Enum.GetValues(typeof(AccessoryType)) as AccessoryType[];
    }

    private System.Collections.IEnumerator AnimateEarcaps(GameObject left, GameObject right, Vector3 leftTarget, Vector3 rightTarget)
    {
        // Set start positions (displacement: +0.1 for left, -0.1 for right)
        left.transform.localPosition = leftTarget + new Vector3(0.1f, 0, 0);
        right.transform.localPosition = rightTarget + new Vector3(-0.1f, 0, 0);

        float elapsed = 0f;
        float duration = 0.1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            left.transform.localPosition = Vector3.Lerp(
                leftTarget + new Vector3(0.1f, 0, 0),
                leftTarget,
                t
            );
            right.transform.localPosition = Vector3.Lerp(
                rightTarget + new Vector3(-0.1f, 0, 0),
                rightTarget,
                t
            );

            yield return null;
        }

        // Ensure final positions
        left.transform.localPosition = leftTarget;
        right.transform.localPosition = rightTarget;
    }
} 