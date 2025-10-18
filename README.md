# AI-Powered VR Construction Safety Training System

A comprehensive virtual reality training system for construction site safety, featuring real-time AI behavioral monitoring and adaptive feedback for crane operation scenarios.

## 🎯 Overview

This Unity-based VR training system combines immersive virtual reality with artificial intelligence to provide personalized construction safety education. The system monitors worker behavior in real-time, tracking proximity awareness, environmental scanning habits, hazard recognition, and response times to create a detailed safety competency assessment.

### Key Features

- **Real-time AI Behavioral Monitoring**: Tracks multiple safety indicators simultaneously including proximity to hazards, head movement patterns, and response times
- **Adaptive Feedback System**: Provides personalized guidance based on individual performance patterns
- **Comprehensive Data Collection**: Hierarchical JSON-based storage for longitudinal analysis and pattern identification
- **Validated Safety Metrics**: Industry-standard proximity zones (2m critical, 5m danger, 8m caution) and response time thresholds
- **First-Person VR Experience**: Immersive construction worker simulation with comfort-oriented controls

## 📋 Project Structure

### Core Components

#### Character & Camera Systems
- **`ConstructionWorkerController.cs`**: Character movement controller with walking, running, jumping, and ground detection
- **`FirstPersonCamera.cs`**: VR-ready first-person camera system with mouse look and smooth follow
- **`CreateConstructionWorker.cs`**: Procedural character generation from Unity primitives

#### Safety Monitoring Systems
- **`SafetyAIMonitor.cs`**: Central AI system that monitors and evaluates safety behaviors
  - Proximity violation detection
  - Environmental scanning analysis
  - Crane awareness tracking
  - Audio warning response measurement
  - Real-time performance scoring

- **`CraneSafetyZone.cs`**: Manages crane operations and safety zones
  - Automated crane movement simulation
  - Swing radius calculations
  - Audio warning triggers
  - Visual safety zone indicators

#### Data & Analytics
- **`SafetyDataSaver.cs`**: Comprehensive session data collection and JSON export
  - Performance metrics tracking
  - Incident logging
  - Audio response analysis
  - Position history recording

#### User Interface
- **`SafetyUIManager.cs`**: Real-time feedback display system
  - Warning notifications
  - Violation alerts
  - Score display
  - Fade animations

#### Setup & Testing
- **`SafetyZoneSetup.cs`**: Automated system configuration and debug tools
  - Auto-setup for safety components
  - Test scenario triggers
  - Real-time debug information

## 🚀 Getting Started

### Prerequisites

- Unity 2021.3 LTS or later
- VR headset (optional - system supports both VR and desktop modes)
- C# development environment

### Installation

1. Clone the repository:
```bash
git clone https://github.com/yourusername/vr-construction-safety.git
```

2. Open the project in Unity

3. Import required packages:
   - Unity XR Plugin Management (for VR support)
   - TextMeshPro (for UI elements)

### Quick Setup

1. Create a new scene or open an existing one
2. Add a `SafetyZoneSetup` component to an empty GameObject
3. Create or assign a crane GameObject (name it "Crane")
4. The system will automatically:
   - Generate a construction worker character
   - Configure safety monitoring components
   - Set up UI elements
   - Initialize data collection

### Manual Setup

For more control over the setup:

1. **Create Worker**:
   - Add `CreateConstructionWorker` component to a GameObject
   - Check the "Create Worker" option or use the context menu
   - Customize materials (body, helmet, vest) as desired

2. **Configure Camera**:
   - The camera is automatically created with the worker
   - Adjust `FirstPersonCamera` settings for VR or desktop mode
   - Toggle `enableMouseLook` for VR compatibility

3. **Setup Safety System**:
   - Add `SafetyAIMonitor` to your crane GameObject
   - Add `CraneSafetyZone` to the same crane
   - Configure safety distances and monitoring intervals
   - Add `SafetyDataSaver` for session recording

## 🎮 Controls

### Desktop Mode
- **WASD**: Move character
- **Mouse**: Look around
- **Left Shift**: Run
- **Space**: Jump
- **Escape**: Toggle cursor lock
- **C**: Test crane rotation (debug)
- **F12**: Save session data

### VR Mode
- **Thumbstick/Trackpad**: Movement
- **Head tracking**: Look around
- Physical movement supported in room-scale VR

## 📊 Data Collection

The system automatically collects comprehensive training data:

### Performance Metrics
- Overall safety score
- Proximity score (distance maintenance)
- Awareness score (scanning behavior)
- Response time to warnings

### Incident Tracking
- Violation type (Minor, Major, Critical)
- Timestamp and location
- Distance to hazard
- Contextual description

### Audio Response Data
- Warning type identification
- Response time measurement
- Response quality assessment
- Player position during warning

### Position History
- 3D position and rotation tracking
- Distance to hazards
- Current safety zone classification
- Temporal progression

## 🔧 Configuration

### Safety Distance Thresholds
```csharp
public float criticalDistance = 2f;  // Red zone - immediate danger
public float dangerDistance = 5f;    // Yellow zone - high risk
public float cautionDistance = 8f;   // Orange zone - moderate risk
```

### Monitoring Intervals
```csharp
public float monitoringInterval = 0.1f;      // AI update frequency
public float dataRecordingInterval = 0.5f;   // Data logging frequency
public float requiredScanInterval = 3f;      // Maximum time between scans
```

### Response Time Quality Thresholds

- **Excellent**: < 0.5 seconds
- **Good**: 0.5 - 1.0 seconds
- **Acceptable**: 1.0 - 2.0 seconds
- **Slow**: 2.0 - 5.0 seconds
- **No Response**: > 5.0 seconds

## 📈 Performance Scoring

The overall safety score combines three weighted components:
```
Overall Score = (Proximity × 0.4) + (Awareness × 0.3) + (Response × 0.3)
```

### Proximity Score
Based on distance maintenance from crane swing radius:
- Inside danger zone: 0.0 - 0.2
- Critical distance: 0.2
- Danger distance: 0.5
- Caution distance: 0.8
- Safe distance: 1.0

### Awareness Score
Calculated from:
- Scanning frequency (time since last 360° scan)
- Crane awareness (looking at overhead hazards)
- Visual attention patterns

### Response Score
Derived from:
- Audio warning reaction times
- Evasive action quality
- Directional awareness

## 💾 Data Export

Training sessions are automatically saved as JSON files to:
```
Application.persistentDataPath/SafetyTrainingData/
```

### File naming format:
```
PlayerName_ScenarioName_YYYY-MM-DD_HH-mm-ss.json
```

### Data structure includes:
- Session metadata (ID, duration, timestamps)
- Performance scores and metrics
- Complete incident history
- Audio response records
- Position trajectory data
- AI-generated recommendations

## 🧪 Testing Features

### Debug Tools
- **Real-time performance display**: On-screen safety metrics
- **Visual safety zones**: Color-coded distance indicators
- **Gizmo visualization**: Scene view debugging aids
- **Test buttons**: Trigger specific scenarios on demand

### Test Scenarios
```csharp
// Trigger backup alarm test
testBackupAlarm = true;

// Start crane movement simulation
testCraneMovement = true;
```

## 🎯 Training Scenarios

### Clear the Zone (Scenario 1A)
Focus areas:
- Maintaining safe distance from swing radiuses
- Regular environmental scanning
- Response to crane movement warnings
- Spatial awareness in dynamic environments

### Key Learning Objectives
1. **Proximity Management**: Maintain OSHA-compliant distances
2. **Situational Awareness**: Perform 360° scans every 3 seconds
3. **Hazard Recognition**: Identify crane swing paths and exclusion zones
4. **Warning Response**: React to audio/visual alerts within 2 seconds

## 📚 Research Foundation

This system is built on extensive research in:
- VR-based construction safety training
- AI behavioral monitoring
- Eye-tracking and head movement analysis
- Cognitive load and learning theory
- Industry safety standards (OSHA, ANSI)

### Key References
- Wang et al. (2018): VR effectiveness in construction training
- Hasanzadeh et al. (2018): Visual attention and hazard recognition
- Sacks et al. (2013): Immersive VR safety training
- ANSI/ASSE A10.28-2018: Safety standards

## 🤝 Contributing

Contributions are welcome! Areas for development:
- Additional safety scenarios (blind spots, vehicle backing)
- Multi-user collaborative training
- Integration with learning management systems
- Advanced haptic feedback
- Eye-tracking support
- Machine learning-based adaptive difficulty


## 🙏 Acknowledgments

- Unity Technologies for the development platform
- OSHA for safety standards and guidelines
- Construction safety research community
- Academic advisors and research participants

## 📧 Contact

For questions, suggestions, or collaboration opportunities, please open an issue or contact the development team.

---

**Note**: This system is designed for training purposes and should supplement, not replace, traditional safety training and on-site supervision.
