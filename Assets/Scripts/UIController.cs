using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Manages the UI system including accessory buttons, toggle buttons, and navigation
/// Optimized for WebGL performance
/// </summary>
public class UIController : MonoBehaviour
{
    [Header("Accessory Buttons")]
    [SerializeField] private Button defaultAccessoryButton;
    [SerializeField] private Button natureAccessoryButton;
    [SerializeField] private Button valkyrieAccessoryButton;
    [SerializeField] private Button werewolfAccessoryButton;
    
    [Header("Toggle Buttons")]
    [SerializeField] private Toggle earplateToggle;
    [SerializeField] private Toggle headsetTopperToggle;
    [SerializeField] private Toggle bothToggle;
    
    [Header("Action Buttons")]
    [SerializeField] private Button checkoutButton;
    [SerializeField] private Button arViewButton;
    
    [Header("UI Panels")]
    [SerializeField] private GameObject topBarPanel;
    [SerializeField] private GameObject bottomBarPanel;
    [SerializeField] private GameObject loadingPanel;
    
    [Header("Button Styling")]
    [SerializeField] private Color selectedButtonColor = Color.blue;
    [SerializeField] private Color normalButtonColor = Color.white;
    [SerializeField] private Color disabledButtonColor = Color.gray;
    
    [Header("Animation Settings")]
    [SerializeField] private float buttonAnimationDuration = 0.2f;
    [SerializeField] private AnimationCurve buttonAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip toggleSound;
    
    [Header("External Links")]
    [SerializeField] private string checkoutURL = "checkout.html";
    [SerializeField] private string arViewURL = "ARWebViewer/index.html";
    
    [Header("References")]
    [SerializeField] private AccessoryManager accessoryManager;
    [SerializeField] private InteractionController interactionController;
    
    // Private fields
    private Button[] accessoryButtons;
    private Toggle[] toggleButtons;
    private AccessoryManager.AccessoryType currentAccessory = AccessoryManager.AccessoryType.Default;
    private bool isUIActive = true;
    
    // Events
    public System.Action<AccessoryManager.AccessoryType> OnAccessoryButtonClicked;
    public System.Action<bool, bool, bool> OnToggleButtonsChanged;
    public System.Action OnCheckoutButtonClicked;
    public System.Action OnARViewButtonClicked;
    
    private void Start()
    {
        InitializeUI();
        SetupEventListeners();
        
        if (accessoryManager == null)
            accessoryManager = FindObjectOfType<AccessoryManager>();
            
        if (interactionController == null)
            interactionController = FindObjectOfType<InteractionController>();
    }
    
    private void Update()
    {
        HandleKeyboardInput();
    }
    
    /// <summary>
    /// Initialize UI components and arrays
    /// </summary>
    private void InitializeUI()
    {
        // Initialize accessory buttons array
        accessoryButtons = new Button[]
        {
            defaultAccessoryButton,
            natureAccessoryButton,
            valkyrieAccessoryButton,
            werewolfAccessoryButton
        };
        
        // Initialize toggle buttons array
        toggleButtons = new Toggle[]
        {
            earplateToggle,
            headsetTopperToggle,
            bothToggle
        };
        
        // Set initial states
        UpdateAccessoryButtonStates();
        UpdateToggleButtonStates();
    }
    
    /// <summary>
    /// Setup event listeners for all UI elements
    /// </summary>
    private void SetupEventListeners()
    {
        // Accessory button listeners
        if (defaultAccessoryButton != null)
            defaultAccessoryButton.onClick.AddListener(() => OnAccessoryButtonPressed(AccessoryManager.AccessoryType.Default));
            
        if (natureAccessoryButton != null)
            natureAccessoryButton.onClick.AddListener(() => OnAccessoryButtonPressed(AccessoryManager.AccessoryType.Nature));
            
        if (valkyrieAccessoryButton != null)
            valkyrieAccessoryButton.onClick.AddListener(() => OnAccessoryButtonPressed(AccessoryManager.AccessoryType.Valkyrie));
            
        if (werewolfAccessoryButton != null)
            werewolfAccessoryButton.onClick.AddListener(() => OnAccessoryButtonPressed(AccessoryManager.AccessoryType.Werewolf));
        
        // Toggle button listeners
        if (earplateToggle != null)
            earplateToggle.onValueChanged.AddListener((value) => OnToggleButtonChanged(0, value));
            
        if (headsetTopperToggle != null)
            headsetTopperToggle.onValueChanged.AddListener((value) => OnToggleButtonChanged(1, value));
            
        if (bothToggle != null)
            bothToggle.onValueChanged.AddListener((value) => OnToggleButtonChanged(2, value));
        
        // Action button listeners
        if (checkoutButton != null)
            checkoutButton.onClick.AddListener(OnCheckoutButtonPressed);
            
        if (arViewButton != null)
            arViewButton.onClick.AddListener(OnARViewButtonPressed);
    }
    
    /// <summary>
    /// Handle keyboard input for shortcuts
    /// </summary>
    private void HandleKeyboardInput()
    {
        if (!isUIActive)
            return;
            
        // Number keys for accessory selection
        if (Input.GetKeyDown(KeyCode.Alpha1))
            OnAccessoryButtonPressed(AccessoryManager.AccessoryType.Default);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            OnAccessoryButtonPressed(AccessoryManager.AccessoryType.Nature);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            OnAccessoryButtonPressed(AccessoryManager.AccessoryType.Valkyrie);
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            OnAccessoryButtonPressed(AccessoryManager.AccessoryType.Werewolf);
        
        // Toggle shortcuts
        if (Input.GetKeyDown(KeyCode.E))
            ToggleEarplate();
        else if (Input.GetKeyDown(KeyCode.H))
            ToggleHeadsetTopper();
        else if (Input.GetKeyDown(KeyCode.B))
            ToggleBoth();
        
        // Action shortcuts
        if (Input.GetKeyDown(KeyCode.C))
            OnCheckoutButtonPressed();
        else if (Input.GetKeyDown(KeyCode.A))
            OnARViewButtonPressed();
    }
    
    /// <summary>
    /// Handle accessory button press
    /// </summary>
    private void OnAccessoryButtonPressed(AccessoryManager.AccessoryType accessoryType)
    {
        if (!isUIActive || accessoryManager == null)
            return;
            
        // Play button sound
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
        
        // Set accessory
        accessoryManager.SetAccessory(accessoryType);
        currentAccessory = accessoryType;
        
        // Update button states
        UpdateAccessoryButtonStates();
        
        // Trigger event
        OnAccessoryButtonClicked?.Invoke(accessoryType);
        
        // Animate button
        AnimateButton(GetAccessoryButton(accessoryType));
    }
    
    /// <summary>
    /// Handle toggle button change
    /// </summary>
    private void OnToggleButtonChanged(int toggleIndex, bool value)
    {
        if (!isUIActive)
            return;
            
        // Play toggle sound
        if (audioSource != null && toggleSound != null)
        {
            audioSource.PlayOneShot(toggleSound);
        }
        
        // Handle "both" toggle logic
        if (toggleIndex == 2 && value) // Both toggle
        {
            // Uncheck other toggles
            if (earplateToggle != null) earplateToggle.isOn = false;
            if (headsetTopperToggle != null) headsetTopperToggle.isOn = false;
        }
        else if (toggleIndex != 2 && value) // Individual toggle
        {
            // Uncheck "both" toggle
            if (bothToggle != null) bothToggle.isOn = false;
        }
        
        // Update toggle states
        UpdateToggleButtonStates();
        
        // Trigger event
        OnToggleButtonsChanged?.Invoke(
            earplateToggle != null ? earplateToggle.isOn : false,
            headsetTopperToggle != null ? headsetTopperToggle.isOn : false,
            bothToggle != null ? bothToggle.isOn : false
        );
        
        // Animate toggle
        AnimateToggle(toggleButtons[toggleIndex]);
    }
    
    /// <summary>
    /// Handle checkout button press
    /// </summary>
    private void OnCheckoutButtonPressed()
    {
        if (!isUIActive)
            return;
            
        // Play button sound
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
        
        // Show loading
        ShowLoading(true);
        
        // Open checkout page
        OpenExternalURL(checkoutURL);
        
        // Trigger event
        OnCheckoutButtonClicked?.Invoke();
        
        // Animate button
        AnimateButton(checkoutButton);
        
        // Hide loading after delay
        Invoke(nameof(HideLoading), 1f);
    }
    
    /// <summary>
    /// Handle AR view button press
    /// </summary>
    private void OnARViewButtonPressed()
    {
        if (!isUIActive)
            return;
            
        // Play button sound
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
        
        // Show loading
        ShowLoading(true);
        
        // Open AR view page
        OpenExternalURL(arViewURL);
        
        // Trigger event
        OnARViewButtonClicked?.Invoke();
        
        // Animate button
        AnimateButton(arViewButton);
        
        // Hide loading after delay
        Invoke(nameof(HideLoading), 1f);
    }
    
    /// <summary>
    /// Update accessory button visual states
    /// </summary>
    private void UpdateAccessoryButtonStates()
    {
        for (int i = 0; i < accessoryButtons.Length; i++)
        {
            if (accessoryButtons[i] != null)
            {
                AccessoryManager.AccessoryType buttonType = (AccessoryManager.AccessoryType)i;
                bool isSelected = buttonType == currentAccessory;
                bool isDisabled = accessoryManager != null && accessoryManager.AreAccessoriesDisabled();
                
                SetButtonColor(accessoryButtons[i], isSelected ? selectedButtonColor : normalButtonColor);
                accessoryButtons[i].interactable = !isDisabled;
            }
        }
    }
    
    /// <summary>
    /// Update toggle button visual states
    /// </summary>
    private void UpdateToggleButtonStates()
    {
        // This can be expanded to show visual feedback for toggle states
        // For now, we'll just ensure the toggles are properly configured
    }
    
    /// <summary>
    /// Set button color with smooth transition
    /// </summary>
    private void SetButtonColor(Button button, Color color)
    {
        if (button == null) return;
        
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = color;
        }
    }
    
    /// <summary>
    /// Animate button press
    /// </summary>
    private void AnimateButton(Button button)
    {
        if (button == null) return;
        
        StartCoroutine(ButtonAnimationCoroutine(button));
    }
    
    /// <summary>
    /// Animate toggle press
    /// </summary>
    private void AnimateToggle(Toggle toggle)
    {
        if (toggle == null) return;
        
        StartCoroutine(ToggleAnimationCoroutine(toggle));
    }
    
    /// <summary>
    /// Button animation coroutine
    /// </summary>
    private System.Collections.IEnumerator ButtonAnimationCoroutine(Button button)
    {
        Transform buttonTransform = button.transform;
        Vector3 originalScale = buttonTransform.localScale;
        Vector3 targetScale = originalScale * 0.95f;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < buttonAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / buttonAnimationDuration;
            float curveValue = buttonAnimationCurve.Evaluate(normalizedTime);
            
            buttonTransform.localScale = Vector3.Lerp(originalScale, targetScale, curveValue);
            
            yield return null;
        }
        
        // Return to original scale
        elapsedTime = 0f;
        while (elapsedTime < buttonAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / buttonAnimationDuration;
            float curveValue = buttonAnimationCurve.Evaluate(normalizedTime);
            
            buttonTransform.localScale = Vector3.Lerp(targetScale, originalScale, curveValue);
            
            yield return null;
        }
        
        buttonTransform.localScale = originalScale;
    }
    
    /// <summary>
    /// Toggle animation coroutine
    /// </summary>
    private System.Collections.IEnumerator ToggleAnimationCoroutine(Toggle toggle)
    {
        Transform toggleTransform = toggle.transform;
        Vector3 originalScale = toggleTransform.localScale;
        Vector3 targetScale = originalScale * 1.1f;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < buttonAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / buttonAnimationDuration;
            float curveValue = buttonAnimationCurve.Evaluate(normalizedTime);
            
            toggleTransform.localScale = Vector3.Lerp(originalScale, targetScale, curveValue);
            
            yield return null;
        }
        
        // Return to original scale
        elapsedTime = 0f;
        while (elapsedTime < buttonAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / buttonAnimationDuration;
            float curveValue = buttonAnimationCurve.Evaluate(normalizedTime);
            
            toggleTransform.localScale = Vector3.Lerp(targetScale, originalScale, curveValue);
            
            yield return null;
        }
        
        toggleTransform.localScale = originalScale;
    }
    
    /// <summary>
    /// Open external URL (WebGL compatible)
    /// </summary>
    private void OpenExternalURL(string url)
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL specific URL opening
            Application.OpenURL(url);
        #else
            // Editor and other platforms
            Application.OpenURL(url);
        #endif
    }
    
    /// <summary>
    /// Show loading panel
    /// </summary>
    private void ShowLoading(bool show)
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(show);
        }
    }
    
    /// <summary>
    /// Hide loading panel
    /// </summary>
    private void HideLoading()
    {
        ShowLoading(false);
    }
    
    /// <summary>
    /// Get accessory button by type
    /// </summary>
    private Button GetAccessoryButton(AccessoryManager.AccessoryType accessoryType)
    {
        int index = (int)accessoryType;
        if (index >= 0 && index < accessoryButtons.Length)
        {
            return accessoryButtons[index];
        }
        return null;
    }
    
    /// <summary>
    /// Toggle earplate programmatically
    /// </summary>
    public void ToggleEarplate()
    {
        if (earplateToggle != null)
        {
            earplateToggle.isOn = !earplateToggle.isOn;
        }
    }
    
    /// <summary>
    /// Toggle headset topper programmatically
    /// </summary>
    public void ToggleHeadsetTopper()
    {
        if (headsetTopperToggle != null)
        {
            headsetTopperToggle.isOn = !headsetTopperToggle.isOn;
        }
    }
    
    /// <summary>
    /// Toggle both programmatically
    /// </summary>
    public void ToggleBoth()
    {
        if (bothToggle != null)
        {
            bothToggle.isOn = !bothToggle.isOn;
        }
    }
    
    /// <summary>
    /// Set UI active state (used during feature interactions)
    /// </summary>
    public void SetUIActive(bool active)
    {
        isUIActive = active;
        
        // Update button interactability
        foreach (var button in accessoryButtons)
        {
            if (button != null)
            {
                button.interactable = active;
            }
        }
        
        foreach (var toggle in toggleButtons)
        {
            if (toggle != null)
            {
                toggle.interactable = active;
            }
        }
        
        if (checkoutButton != null)
            checkoutButton.interactable = active;
            
        if (arViewButton != null)
            arViewButton.interactable = active;
    }
    
    /// <summary>
    /// Get current accessory type
    /// </summary>
    public AccessoryManager.AccessoryType GetCurrentAccessory()
    {
        return currentAccessory;
    }
    
    /// <summary>
    /// Get current toggle states
    /// </summary>
    public (bool earplate, bool headsetTopper, bool both) GetToggleStates()
    {
        return (
            earplateToggle != null ? earplateToggle.isOn : false,
            headsetTopperToggle != null ? headsetTopperToggle.isOn : false,
            bothToggle != null ? bothToggle.isOn : false
        );
    }
    
    /// <summary>
    /// Check if UI is currently active
    /// </summary>
    public bool IsUIActive()
    {
        return isUIActive;
    }
} 