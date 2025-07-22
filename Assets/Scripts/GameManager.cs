using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main game manager that coordinates all systems and provides WebGL optimization
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private AccessoryManager accessoryManager;
    [SerializeField] private InteractionController interactionController;
    [SerializeField] private UIController uiController;
    
    [Header("WebGL Settings")]
    [SerializeField] private bool enableWebGLOptimizations = true;
    [SerializeField] private int targetFrameRate = 60;
    [SerializeField] private bool enableVSync = true;
    
    [Header("Performance Settings")]
    [SerializeField] private bool enableLODSystem = true;
    [SerializeField] private float cullingDistance = 100f;
    [SerializeField] private bool enableOcclusionCulling = true;
    
    [Header("Audio Settings")]
    [SerializeField] private AudioSource backgroundMusicSource;
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private float musicVolume = 0.5f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugMode = false;
    [SerializeField] private bool showPerformanceStats = false;
    
    // Private fields
    private bool isInitialized = false;
    private float frameRate;
    private float deltaTime;
    
    // Events
    public System.Action OnGameInitialized;
    public System.Action OnGamePaused;
    public System.Action OnGameResumed;
    
    private void Awake()
    {
        // Ensure only one GameManager exists
        if (FindObjectsOfType<GameManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);
        InitializeGame();
    }
    
    private void Start()
    {
        SetupSystemConnections();
        ApplyWebGLOptimizations();
        StartBackgroundMusic();
        
        isInitialized = true;
        OnGameInitialized?.Invoke();
    }
    
    private void Update()
    {
        UpdatePerformanceStats();
        HandleGlobalInput();
    }
    
    /// <summary>
    /// Initialize the game systems
    /// </summary>
    private void InitializeGame()
    {
        // Find system references if not assigned
        if (accessoryManager == null)
            accessoryManager = FindObjectOfType<AccessoryManager>();
            
        if (interactionController == null)
            interactionController = FindObjectOfType<InteractionController>();
            
        if (uiController == null)
            uiController = FindObjectOfType<UIController>();
    }
    
    /// <summary>
    /// Setup connections between systems
    /// </summary>
    private void SetupSystemConnections()
    {
        // Connect InteractionController events to AccessoryManager
        if (interactionController != null)
        {
            interactionController.OnFeaturePointClicked += OnFeaturePointActivated;
            interactionController.OnFeaturePointHovered += OnFeaturePointHovered;
            interactionController.OnFeaturePointExited += OnFeaturePointExited;
        }
        
        // Connect UIController events
        if (uiController != null)
        {
            uiController.OnAccessoryButtonClicked += OnAccessoryChanged;
            uiController.OnToggleButtonsChanged += OnToggleButtonsChanged;
            uiController.OnCheckoutButtonClicked += OnCheckoutButtonClicked;
            uiController.OnARViewButtonClicked += OnARViewButtonClicked;
        }
        
        // Connect AccessoryManager events
        if (accessoryManager != null)
        {
            accessoryManager.OnAccessoryChanged += OnAccessoryManagerChanged;
        }
    }
    
    /// <summary>
    /// Apply WebGL-specific optimizations
    /// </summary>
    private void ApplyWebGLOptimizations()
    {
        if (!enableWebGLOptimizations)
            return;
            
        #if UNITY_WEBGL && !UNITY_EDITOR
            // Set target frame rate
            Application.targetFrameRate = targetFrameRate;
            
            // Enable VSync
            QualitySettings.vSyncCount = enableVSync ? 1 : 0;
            
            // Optimize quality settings for WebGL
            QualitySettings.SetQualityLevel(2, true); // Medium quality
            
            // Disable unnecessary features for WebGL
            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.antiAliasing = 0;
            
            // Optimize rendering
            QualitySettings.pixelLightCount = 2;
            QualitySettings.globalTextureMipmapLimit = 1;
            
            // Enable LOD system
            if (enableLODSystem)
            {
                QualitySettings.lodBias = 0.5f;
            }
            
            // Set culling distance
            if (enableOcclusionCulling)
            {
                QualitySettings.maxQueuedFrames = 2;
            }
        #endif
    }
    
    /// <summary>
    /// Start background music
    /// </summary>
    private void StartBackgroundMusic()
    {
        if (backgroundMusicSource != null && backgroundMusic != null)
        {
            backgroundMusicSource.clip = backgroundMusic;
            backgroundMusicSource.volume = musicVolume;
            backgroundMusicSource.loop = true;
            backgroundMusicSource.Play();
        }
    }
    
    /// <summary>
    /// Update performance statistics
    /// </summary>
    private void UpdatePerformanceStats()
    {
        if (!showPerformanceStats)
            return;
            
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        frameRate = 1.0f / deltaTime;
    }
    
    /// <summary>
    /// Handle global input
    /// </summary>
    private void HandleGlobalInput()
    {
        // Pause/Resume
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
        
        // Debug mode toggle
        if (Input.GetKeyDown(KeyCode.F1))
        {
            enableDebugMode = !enableDebugMode;
        }
        
        // Performance stats toggle
        if (Input.GetKeyDown(KeyCode.F2))
        {
            showPerformanceStats = !showPerformanceStats;
        }
        
        // Reload scene
        if (Input.GetKeyDown(KeyCode.F5))
        {
            ReloadScene();
        }
    }
    
    /// <summary>
    /// Handle feature point activation
    /// </summary>
    private void OnFeaturePointActivated(InteractionController.FeaturePoint featurePoint)
    {
        if (enableDebugMode)
        {
            Debug.Log($"Feature point activated: {featurePoint.featureName}");
        }
        
        // Disable UI during feature interaction
        if (uiController != null)
        {
            uiController.SetUIActive(false);
        }
    }
    
    /// <summary>
    /// Handle feature point hover
    /// </summary>
    private void OnFeaturePointHovered(InteractionController.FeaturePoint featurePoint)
    {
        if (enableDebugMode)
        {
            Debug.Log($"Feature point hovered: {featurePoint.featureName}");
        }
    }
    
    /// <summary>
    /// Handle feature point exit
    /// </summary>
    private void OnFeaturePointExited()
    {
        // Re-enable UI when not hovering over feature points
        if (uiController != null && !interactionController.IsAnyFeatureActive())
        {
            uiController.SetUIActive(true);
        }
    }
    
    /// <summary>
    /// Handle accessory change from UI
    /// </summary>
    private void OnAccessoryChanged(AccessoryManager.AccessoryType accessoryType)
    {
        if (enableDebugMode)
        {
            Debug.Log($"Accessory changed to: {accessoryType}");
        }
    }
    
    /// <summary>
    /// Handle toggle buttons change
    /// </summary>
    private void OnToggleButtonsChanged(bool earplate, bool headsetTopper, bool both)
    {
        if (enableDebugMode)
        {
            Debug.Log($"Toggle states - Earplate: {earplate}, HeadsetTopper: {headsetTopper}, Both: {both}");
        }
    }
    
    /// <summary>
    /// Handle checkout button click
    /// </summary>
    private void OnCheckoutButtonClicked()
    {
        if (enableDebugMode)
        {
            Debug.Log("Checkout button clicked");
        }
    }
    
    /// <summary>
    /// Handle AR view button click
    /// </summary>
    private void OnARViewButtonClicked()
    {
        if (enableDebugMode)
        {
            Debug.Log("AR view button clicked");
        }
    }
    
    /// <summary>
    /// Handle accessory manager change
    /// </summary>
    private void OnAccessoryManagerChanged(AccessoryManager.AccessoryType accessoryType)
    {
        if (enableDebugMode)
        {
            Debug.Log($"Accessory manager changed to: {accessoryType}");
        }
    }
    
    /// <summary>
    /// Toggle pause state
    /// </summary>
    public void TogglePause()
    {
        if (Time.timeScale > 0)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }
    
    /// <summary>
    /// Pause the game
    /// </summary>
    public void PauseGame()
    {
        Time.timeScale = 0;
        OnGamePaused?.Invoke();
        
        if (enableDebugMode)
        {
            Debug.Log("Game paused");
        }
    }
    
    /// <summary>
    /// Resume the game
    /// </summary>
    public void ResumeGame()
    {
        Time.timeScale = 1;
        OnGameResumed?.Invoke();
        
        if (enableDebugMode)
        {
            Debug.Log("Game resumed");
        }
    }
    
    /// <summary>
    /// Reload the current scene
    /// </summary>
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    /// <summary>
    /// Get current frame rate
    /// </summary>
    public float GetFrameRate()
    {
        return frameRate;
    }
    
    /// <summary>
    /// Get current delta time
    /// </summary>
    public float GetDeltaTime()
    {
        return deltaTime;
    }
    
    /// <summary>
    /// Check if game is initialized
    /// </summary>
    public bool IsInitialized()
    {
        return isInitialized;
    }
    
    /// <summary>
    /// Check if debug mode is enabled
    /// </summary>
    public bool IsDebugModeEnabled()
    {
        return enableDebugMode;
    }
    
    /// <summary>
    /// Set debug mode
    /// </summary>
    public void SetDebugMode(bool enabled)
    {
        enableDebugMode = enabled;
    }
    
    /// <summary>
    /// Get accessory manager reference
    /// </summary>
    public AccessoryManager GetAccessoryManager()
    {
        return accessoryManager;
    }
    
    /// <summary>
    /// Get interaction controller reference
    /// </summary>
    public InteractionController GetInteractionController()
    {
        return interactionController;
    }
    
    /// <summary>
    /// Get UI controller reference
    /// </summary>
    public UIController GetUIController()
    {
        return uiController;
    }
    
    private void OnGUI()
    {
        if (!showPerformanceStats)
            return;
            
        // Display performance stats
        GUILayout.BeginArea(new Rect(10, 10, 200, 100));
        GUILayout.Label($"FPS: {frameRate:F1}");
        GUILayout.Label($"Delta Time: {deltaTime:F4}");
        GUILayout.Label($"Memory: {System.GC.GetTotalMemory(false) / 1024 / 1024:F1} MB");
        GUILayout.EndArea();
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            OnGamePaused?.Invoke();
        }
        else
        {
            OnGameResumed?.Invoke();
        }
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            OnGamePaused?.Invoke();
        }
        else
        {
            OnGameResumed?.Invoke();
        }
    }
} 