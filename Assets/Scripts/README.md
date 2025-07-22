# Unity WebGL 3D Experience Script Structure

This Unity project provides a complete script structure for a WebGL-based 3D experience featuring interactive accessories, feature points, and a comprehensive UI system.

## Script Overview

### Core Scripts

#### 1. **AccessoryManager.cs**
Manages earcap accessories with smooth animations and state control.

**Features:**
- 4 accessory types: Default, Nature, Valkyrie, Werewolf
- Smooth fade animations with customizable curves
- Audio feedback for accessory swaps
- Enable/disable functionality during feature interactions
- WebGL-optimized performance

**Key Methods:**
- `SetAccessory(AccessoryType, bool animate)` - Change accessories with optional animation
- `SetAccessoriesEnabled(bool)` - Enable/disable accessories during interactions
- `GetCurrentAccessory()` - Get currently active accessory

#### 2. **InteractionController.cs**
Handles 3D model feature point interactions with animations and info windows.

**Features:**
- 3 clickable feature points on 3D model
- Hover highlighting with customizable colors
- Click animations using Unity Animator
- Dynamic info windows with fade effects
- Audio feedback for interactions
- Automatic accessory disabling during interactions

**Key Methods:**
- `ActivateFeaturePoint(FeaturePoint)` - Trigger feature point interaction
- `StopAllInteractions()` - Force stop all active interactions
- `IsAnyFeatureActive()` - Check if any feature is currently active

#### 3. **UIController.cs**
Manages the complete UI system with buttons, toggles, and navigation.

**Features:**
- Top bar with 4 accessory buttons
- Bottom bar with 3 toggle buttons (Earplate, Headset Topper, Both)
- Checkout button linking to local HTML page
- AR view button for AR experience
- Keyboard shortcuts for all functions
- Button animations and audio feedback
- WebGL-compatible external URL handling

**Key Methods:**
- `SetUIActive(bool)` - Enable/disable UI during interactions
- `GetToggleStates()` - Get current toggle button states
- `ToggleEarplate()`, `ToggleHeadsetTopper()`, `ToggleBoth()` - Programmatic toggle control

#### 4. **GameManager.cs**
Main coordinator that manages all systems and provides WebGL optimization.

**Features:**
- System coordination and event management
- WebGL-specific performance optimizations
- Background music management
- Debug mode and performance statistics
- Global input handling (pause, debug toggles)
- Scene management

**Key Methods:**
- `TogglePause()` - Pause/resume game
- `SetDebugMode(bool)` - Enable/disable debug features
- `GetFrameRate()` - Get current performance metrics

## Setup Instructions

### 1. Scene Setup

1. **Create an empty GameObject** and attach the `GameManager` script
2. **Create accessory GameObjects** for each earcap type and attach them to your 3D model
3. **Set up feature points** by creating colliders on your 3D model and configuring them in the InteractionController
4. **Create UI elements** following the structure below

### 2. UI Hierarchy Structure

```
Canvas (Screen Space - Overlay)
├── TopBarPanel
│   ├── DefaultAccessoryButton
│   ├── NatureAccessoryButton
│   ├── ValkyrieAccessoryButton
│   └── WerewolfAccessoryButton
├── BottomBarPanel
│   ├── EarplateToggle
│   ├── HeadsetTopperToggle
│   ├── BothToggle
│   ├── CheckoutButton
│   └── ARViewButton
├── LoadingPanel
└── InfoWindowParent
```

### 3. Component Configuration

#### AccessoryManager Setup
- Assign each accessory GameObject to the corresponding field
- Configure animation duration and curve
- Set up AudioSource and swap sound clip

#### InteractionController Setup
- Create 3 feature points with colliders
- Assign Animator components for animations
- Configure highlight renderers and colors
- Set up info window prefab and parent

#### UIController Setup
- Assign all UI buttons and toggles
- Configure button colors and animation settings
- Set up AudioSource for UI sounds
- Configure external URLs for checkout and AR view

#### GameManager Setup
- Assign references to all other managers
- Configure WebGL optimization settings
- Set up background music
- Enable debug mode as needed

### 4. WebGL Build Settings

1. **Player Settings:**
   - Set WebGL Template to "Minimal"
   - Enable "Development Build" for debugging
   - Set Compression Format to "Disabled" for faster loading

2. **Quality Settings:**
   - Use Medium quality level
   - Disable shadows and anti-aliasing
   - Set pixel light count to 2
   - Enable LOD system

3. **Build Settings:**
   - Add your scene to build
   - Set platform to WebGL
   - Configure compression settings

## Keyboard Shortcuts

- **1-4**: Switch accessories (Default, Nature, Valkyrie, Werewolf)
- **E**: Toggle earplate
- **H**: Toggle headset topper
- **B**: Toggle both
- **C**: Checkout
- **A**: AR view
- **ESC**: Pause/resume
- **F1**: Toggle debug mode
- **F2**: Toggle performance stats
- **F5**: Reload scene

## Performance Optimization

### WebGL-Specific Optimizations
- Reduced texture quality and pixel light count
- Disabled shadows and anti-aliasing
- Optimized LOD settings
- Frame rate limiting to 60 FPS
- Efficient coroutine usage for animations

### General Optimizations
- Object pooling for info windows
- Efficient event system usage
- Minimal GameObject instantiation
- Optimized material and renderer access

## File Structure

```
Assets/Scripts/
├── AccessoryManager.cs      # Accessory management system
├── InteractionController.cs # Feature point interactions
├── UIController.cs         # UI management system
├── GameManager.cs          # Main game coordinator
└── README.md              # This documentation
```

## Dependencies

- Unity 2021.3 LTS or later
- WebGL build support
- UI Toolkit (for advanced UI features)
- Audio system for sound effects

## Troubleshooting

### Common Issues

1. **Accessories not switching:**
   - Check that AccessoryManager references are assigned
   - Verify accessory GameObjects are active in hierarchy
   - Ensure no feature interactions are currently active

2. **Feature points not responding:**
   - Verify colliders are attached to feature point transforms
   - Check that InteractionController references are set
   - Ensure camera is properly assigned

3. **UI buttons not working:**
   - Verify all button references in UIController
   - Check that EventSystem is present in scene
   - Ensure UI is not disabled during interactions

4. **WebGL performance issues:**
   - Enable WebGL optimizations in GameManager
   - Reduce texture quality and model complexity
   - Monitor performance stats with F2 key

### Debug Features

- Press **F1** to enable debug mode
- Press **F2** to show performance statistics
- Check Console for detailed debug logs
- Use GameManager's debug methods for system testing

## Extending the System

### Adding New Accessories
1. Add new enum value to `AccessoryManager.AccessoryType`
2. Create accessory GameObject and assign to AccessoryManager
3. Add corresponding UI button in UIController
4. Update keyboard shortcuts if needed

### Adding New Feature Points
1. Create new FeaturePoint in InteractionController array
2. Set up collider and animator
3. Configure highlight renderers and colors
4. Add info window content

### Customizing UI
1. Modify button styles in UIController
2. Adjust animation curves and durations
3. Customize color schemes and layouts
4. Add new UI panels as needed

## License

This script structure is provided as-is for educational and development purposes. Modify and extend as needed for your specific project requirements. 