using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Handles 3D model feature point interactions with animations and info windows
/// Optimized for WebGL performance
/// </summary>
public class InteractionController : MonoBehaviour
{
    [Header("Feature Points")]
    [SerializeField] private FeaturePoint[] featurePoints;
    
    [Header("Info Windows")]
    [SerializeField] private GameObject infoWindowPrefab;
    [SerializeField] private Transform infoWindowParent;
    [SerializeField] private float infoWindowOffset = 2f;
    
    [Header("Animation Settings")]
    [SerializeField] private float highlightAnimationDuration = 0.3f;
    [SerializeField] private float infoWindowFadeDuration = 0.5f;
    [SerializeField] private AnimationCurve highlightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;
    
    [Header("References")]
    [SerializeField] private AccessoryManager accessoryManager;
    [SerializeField] private Camera mainCamera;
    
    // Private fields
    private FeaturePoint currentHighlightedPoint;
    private GameObject currentInfoWindow;
    private bool isInteractionActive = false;
    
    // Events
    public System.Action<FeaturePoint> OnFeaturePointClicked;
    public System.Action<FeaturePoint> OnFeaturePointHovered;
    public System.Action OnFeaturePointExited;
    
    [System.Serializable]
    public class FeaturePoint
    {
        [Header("Feature Settings")]
        public string featureName;
        public Transform featureTransform;
        public Animator featureAnimator;
        public string animationTriggerName = "Activate";
        
        [Header("Info Content")]
        [TextArea(3, 5)]
        public string featureDescription;
        public Sprite featureIcon;
        
        [Header("Visual Feedback")]
        public Renderer[] highlightRenderers;
        public Color highlightColor = Color.yellow;
        public float highlightIntensity = 1.5f;
        
        [Header("Audio")]
        public AudioClip featureAudio;
        
        // Runtime state
        [HideInInspector] public bool isActive = false;
        [HideInInspector] public Color originalColor;
        [HideInInspector] public Material[] originalMaterials;
    }
    
    private void Start()
    {
        InitializeFeaturePoints();
        
        if (mainCamera == null)
            mainCamera = Camera.main;
            
        if (accessoryManager == null)
            accessoryManager = FindObjectOfType<AccessoryManager>();
    }
    
    private void Update()
    {
        HandleInput();
    }
    
    /// <summary>
    /// Initialize feature points and their materials
    /// </summary>
    private void InitializeFeaturePoints()
    {
        foreach (var point in featurePoints)
        {
            if (point.highlightRenderers != null)
            {
                point.originalMaterials = new Material[point.highlightRenderers.Length];
                for (int i = 0; i < point.highlightRenderers.Length; i++)
                {
                    if (point.highlightRenderers[i] != null)
                    {
                        point.originalMaterials[i] = point.highlightRenderers[i].material;
                        point.originalColor = point.originalMaterials[i].color;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Handle mouse input for feature point interactions
    /// </summary>
    private void HandleInput()
    {
        if (isInteractionActive)
            return;
            
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            FeaturePoint hitPoint = GetFeaturePointFromCollider(hit.collider);
            
            if (hitPoint != null)
            {
                if (currentHighlightedPoint != hitPoint)
                {
                    // Exit previous point
                    if (currentHighlightedPoint != null)
                    {
                        ExitFeaturePoint(currentHighlightedPoint);
                    }
                    
                    // Enter new point
                    EnterFeaturePoint(hitPoint);
                }
                
                // Handle click
                if (Input.GetMouseButtonDown(0))
                {
                    ActivateFeaturePoint(hitPoint);
                }
            }
            else
            {
                // Exit current point if not hovering over any feature point
                if (currentHighlightedPoint != null)
                {
                    ExitFeaturePoint(currentHighlightedPoint);
                }
            }
        }
        else
        {
            // Exit current point if raycast misses
            if (currentHighlightedPoint != null)
            {
                ExitFeaturePoint(currentHighlightedPoint);
            }
        }
    }
    
    /// <summary>
    /// Get feature point from collider
    /// </summary>
    private FeaturePoint GetFeaturePointFromCollider(Collider collider)
    {
        foreach (var point in featurePoints)
        {
            if (point.featureTransform != null && 
                point.featureTransform.GetComponent<Collider>() == collider)
            {
                return point;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Enter feature point (hover)
    /// </summary>
    private void EnterFeaturePoint(FeaturePoint point)
    {
        currentHighlightedPoint = point;
        
        // Play hover sound
        if (audioSource != null && hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
        
        // Start highlight animation
        StartCoroutine(HighlightFeaturePoint(point, true));
        
        OnFeaturePointHovered?.Invoke(point);
    }
    
    /// <summary>
    /// Exit feature point
    /// </summary>
    private void ExitFeaturePoint(FeaturePoint point)
    {
        if (currentHighlightedPoint == point)
        {
            currentHighlightedPoint = null;
        }
        
        // Stop highlight animation
        StartCoroutine(HighlightFeaturePoint(point, false));
        
        OnFeaturePointExited?.Invoke();
    }
    
    /// <summary>
    /// Activate feature point (click)
    /// </summary>
    private void ActivateFeaturePoint(FeaturePoint point)
    {
        if (point.isActive)
            return;
            
        isInteractionActive = true;
        point.isActive = true;
        
        // Play click sound
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
        
        // Play feature audio
        if (audioSource != null && point.featureAudio != null)
        {
            audioSource.PlayOneShot(point.featureAudio);
        }
        
        // Disable accessories during interaction
        if (accessoryManager != null)
        {
            accessoryManager.SetAccessoriesEnabled(false);
        }
        
        // Trigger animation
        if (point.featureAnimator != null)
        {
            point.featureAnimator.SetTrigger(point.animationTriggerName);
        }
        
        // Show info window
        ShowInfoWindow(point);
        
        OnFeaturePointClicked?.Invoke(point);
        
        // Re-enable accessories after animation
        StartCoroutine(ReEnableAccessoriesAfterDelay(point));
    }
    
    /// <summary>
    /// Show info window for feature point
    /// </summary>
    private void ShowInfoWindow(FeaturePoint point)
    {
        if (infoWindowPrefab == null || infoWindowParent == null)
            return;
            
        // Destroy previous info window
        if (currentInfoWindow != null)
        {
            Destroy(currentInfoWindow);
        }
        
        // Create new info window
        currentInfoWindow = Instantiate(infoWindowPrefab, infoWindowParent);
        
        // Position info window
        Vector3 position = point.featureTransform.position + Vector3.up * infoWindowOffset;
        currentInfoWindow.transform.position = position;
        
        // Make info window face camera
        currentInfoWindow.transform.LookAt(mainCamera.transform);
        currentInfoWindow.transform.Rotate(0, 180, 0);
        
        // Set info window content
        InfoWindow infoWindowComponent = currentInfoWindow.GetComponent<InfoWindow>();
        if (infoWindowComponent != null)
        {
            infoWindowComponent.SetContent(point.featureName, point.featureDescription, point.featureIcon);
        }
        
        // Fade in info window
        StartCoroutine(FadeInfoWindow(currentInfoWindow, true));
    }
    
    /// <summary>
    /// Hide current info window
    /// </summary>
    private void HideInfoWindow()
    {
        if (currentInfoWindow != null)
        {
            StartCoroutine(FadeInfoWindow(currentInfoWindow, false));
        }
    }
    
    /// <summary>
    /// Highlight feature point with animation
    /// </summary>
    private System.Collections.IEnumerator HighlightFeaturePoint(FeaturePoint point, bool highlight)
    {
        if (point.highlightRenderers == null)
            yield break;
            
        float elapsedTime = 0f;
        Color startColor = point.originalColor;
        Color targetColor = highlight ? point.highlightColor * point.highlightIntensity : point.originalColor;
        
        while (elapsedTime < highlightAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / highlightAnimationDuration;
            float curveValue = highlightCurve.Evaluate(normalizedTime);
            Color currentColor = Color.Lerp(startColor, targetColor, curveValue);
            
            for (int i = 0; i < point.highlightRenderers.Length; i++)
            {
                if (point.highlightRenderers[i] != null && point.originalMaterials[i] != null)
                {
                    point.highlightRenderers[i].material.color = currentColor;
                }
            }
            
            yield return null;
        }
        
        // Set final color
        for (int i = 0; i < point.highlightRenderers.Length; i++)
        {
            if (point.highlightRenderers[i] != null && point.originalMaterials[i] != null)
            {
                point.highlightRenderers[i].material.color = targetColor;
            }
        }
    }
    
    /// <summary>
    /// Fade info window in or out
    /// </summary>
    private System.Collections.IEnumerator FadeInfoWindow(GameObject infoWindow, bool fadeIn)
    {
        CanvasGroup canvasGroup = infoWindow.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = infoWindow.AddComponent<CanvasGroup>();
            
        float elapsedTime = 0f;
        float startAlpha = fadeIn ? 0f : 1f;
        float targetAlpha = fadeIn ? 1f : 0f;
        
        canvasGroup.alpha = startAlpha;
        
        while (elapsedTime < infoWindowFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / infoWindowFadeDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, normalizedTime);
            
            yield return null;
        }
        
        canvasGroup.alpha = targetAlpha;
        
        if (!fadeIn)
        {
            Destroy(infoWindow);
        }
    }
    
    /// <summary>
    /// Re-enable accessories after feature interaction
    /// </summary>
    private System.Collections.IEnumerator ReEnableAccessoriesAfterDelay(FeaturePoint point)
    {
        // Wait for animation to complete (you can adjust this based on your animation length)
        yield return new WaitForSeconds(3f);
        
        // Hide info window
        HideInfoWindow();
        
        // Re-enable accessories
        if (accessoryManager != null)
        {
            accessoryManager.SetAccessoriesEnabled(true);
        }
        
        point.isActive = false;
        isInteractionActive = false;
    }
    
    /// <summary>
    /// Get all feature points
    /// </summary>
    public FeaturePoint[] GetFeaturePoints()
    {
        return featurePoints;
    }
    
    /// <summary>
    /// Check if any feature point is currently active
    /// </summary>
    public bool IsAnyFeatureActive()
    {
        return isInteractionActive;
    }
    
    /// <summary>
    /// Force stop all feature interactions
    /// </summary>
    public void StopAllInteractions()
    {
        isInteractionActive = false;
        
        foreach (var point in featurePoints)
        {
            point.isActive = false;
        }
        
        HideInfoWindow();
        
        if (accessoryManager != null)
        {
            accessoryManager.SetAccessoriesEnabled(true);
        }
    }
}

/// <summary>
/// Info window component for displaying feature information
/// </summary>
public class InfoWindow : MonoBehaviour
{
    [SerializeField] private Text titleText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Image iconImage;
    
    public void SetContent(string title, string description, Sprite icon)
    {
        if (titleText != null)
            titleText.text = title;
            
        if (descriptionText != null)
            descriptionText.text = description;
            
        if (iconImage != null && icon != null)
            iconImage.sprite = icon;
    }
} 