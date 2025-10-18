using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SafetyAIMonitor : MonoBehaviour
{
    [Header("Zone Configuration")]
    public GameObject crane;
    public float zoneRadius = 15f;
    public Color zoneColor = new Color(1f, 0f, 0f, 0.2f);
    
    [Header("Safety Distances")]
    public float criticalDistance = 2f;
    public float dangerDistance = 5f;
    public float cautionDistance = 8f;
    
    [Header("Monitoring Settings")]
    public float monitoringInterval = 0.1f;
    public float dataRecordingInterval = 0.5f;
    public float requiredScanInterval = 3f;
    public float minScanAngle = 45f;
    
    [Header("Player Reference")]
    public Transform player;
    public Camera playerCamera;
    
    [Header("UI Elements")]
    public GameObject warningPrefab;
    public GameObject violationPrefab;
    public Canvas uiCanvas;
    
    [Header("Data Saving")]
    public SafetyDataSaver dataSaver;
    
    // Performance tracking
    [System.NonSerialized]
    public PerformanceData performanceData;
    private float timeSinceLastScan = 0f;
    private Quaternion lastHeadRotation;
    private Vector3 lastPlayerPosition;
    private bool isMonitoring = false;
    
    // Violation tracking
    private int minorViolations = 0;
    private int majorViolations = 0;
    private int criticalViolations = 0;
    private List<SafetyIncident> incidents = new List<SafetyIncident>();
    
    // Audio warning system
    private Dictionary<string, AudioWarning> activeWarnings = new Dictionary<string, AudioWarning>();
    private float currentResponseTime = 0f;
    private bool awaitingResponse = false;
    
    // Visualization
    private GameObject zoneVisualizer;
    
    void Start()
    {
        InitializeMonitoring();
        CreateZoneVisualizer();
        StartCoroutine(MonitoringLoop());
        StartCoroutine(DataRecordingLoop());
        
        // Find data saver
        if (dataSaver == null)
        {
            dataSaver = FindObjectOfType<SafetyDataSaver>();
        }
    }
    
    void InitializeMonitoring()
    {
        performanceData = new PerformanceData();
        
        // Find player references if not assigned
        if (player == null)
        {
            GameObject constructionWorker = GameObject.Find("Construction Worker");
            if (constructionWorker != null)
            {
                player = constructionWorker.transform;
            }
        }
        
        if (playerCamera == null && player != null)
        {
            playerCamera = player.GetComponentInChildren<Camera>();
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }
        }
        
        lastHeadRotation = playerCamera != null ? playerCamera.transform.rotation : Quaternion.identity;
        lastPlayerPosition = player != null ? player.position : Vector3.zero;
        
        Debug.Log("Safety AI Monitor initialized. Monitoring zone around crane.");
    }
    
    void CreateZoneVisualizer()
    {
        // Create visual representation of safety zones
        zoneVisualizer = new GameObject("Safety Zone Visualizer");
        zoneVisualizer.transform.SetParent(crane.transform);
        zoneVisualizer.transform.localPosition = Vector3.zero;
        
        // Create zone indicators for each safety distance
        CreateZoneRing("Critical Zone", criticalDistance, Color.red);
        CreateZoneRing("Danger Zone", dangerDistance, Color.yellow);
        CreateZoneRing("Caution Zone", cautionDistance, new Color(1f, 0.5f, 0f));
    }
    
    void CreateZoneRing(string name, float radius, Color color)
    {
        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = name;
        ring.transform.SetParent(zoneVisualizer.transform);
        ring.transform.localPosition = Vector3.zero;
        ring.transform.localScale = new Vector3(radius * 2, 0.1f, radius * 2);
        
        Renderer renderer = ring.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(color.r, color.g, color.b, 0.3f);
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        renderer.material = mat;
        
        // Remove collider to avoid interference
        Destroy(ring.GetComponent<Collider>());
    }
    
    IEnumerator MonitoringLoop()
    {
        isMonitoring = true;
        
        while (isMonitoring)
        {
            if (player != null && IsPlayerInZone())
            {
                // Check proximity violations
                CheckProximityViolations();
                
                // Monitor head movement for scanning
                CheckScanningBehavior();
                
                // Check for active audio warnings
                ProcessAudioWarnings();
            }
            
            yield return new WaitForSeconds(monitoringInterval);
        }
    }
    
    IEnumerator DataRecordingLoop()
    {
        while (isMonitoring)
        {
            if (player != null && IsPlayerInZone())
            {
                RecordPerformanceData();
            }
            
            yield return new WaitForSeconds(dataRecordingInterval);
        }
    }
    
    bool IsPlayerInZone()
    {
        if (player == null || crane == null) return false;
        
        float distance = Vector3.Distance(player.position, crane.transform.position);
        return distance <= zoneRadius;
    }
    
    void CheckProximityViolations()
    {
        float distanceToHazard = CalculateDistanceToSwingRadius(
            player.position, 
            crane.transform.position, 
            GetCurrentSwingRadius()
        );
        
        performanceData.lastProximityDistance = distanceToHazard;
        
        if (distanceToHazard < criticalDistance)
        {
            CreateViolation(ViolationType.Critical, 
                "DANGER: Too close to crane swing radius!", 
                distanceToHazard);
            ShowImmediateWarning("DANGER: Move away from crane immediately!");
        }
        else if (distanceToHazard < dangerDistance)
        {
            CreateViolation(ViolationType.Major, 
                "Warning: Entering dangerous proximity to crane", 
                distanceToHazard);
            ShowWarning("Caution: You are too close to the crane");
        }
        else if (distanceToHazard < cautionDistance)
        {
            ShowHint("Remember to maintain safe distance from equipment");
        }
    }
    
    float CalculateDistanceToSwingRadius(Vector3 playerPos, Vector3 cranePos, float swingRadius)
    {
        Vector3 horizontalDistance = new Vector3(
            playerPos.x - cranePos.x, 
            0, 
            playerPos.z - cranePos.z
        );
        
        float distanceFromCenter = horizontalDistance.magnitude;
        float distanceFromSwingEdge = Mathf.Max(0, swingRadius - distanceFromCenter);
        
        // If player is outside swing radius, return distance to edge
        if (distanceFromCenter > swingRadius)
        {
            return distanceFromCenter - swingRadius;
        }
        
        // If inside swing radius, return negative distance (more dangerous)
        return -distanceFromSwingEdge;
    }
    
    float GetCurrentSwingRadius()
    {
        // This could be dynamic based on crane animation
        // For now, return a fixed radius
        return 10f;
    }
    
    void CheckScanningBehavior()
    {
        if (playerCamera == null) return;
        
        Quaternion currentRotation = playerCamera.transform.rotation;
        float angularVelocity = Quaternion.Angle(lastHeadRotation, currentRotation) / Time.deltaTime;
        
        // Check if player is actively scanning
        if (angularVelocity > 30f) // degrees per second
        {
            timeSinceLastScan = 0f;
            performanceData.lastScanTime = Time.time;
            
            // Check if looking at crane
            if (IsLookingAtCrane())
            {
                performanceData.lookedAtCraneCount++;
                performanceData.lastCraneLookTime = Time.time;
            }
        }
        else
        {
            timeSinceLastScan += Time.deltaTime;
        }
        
        // Create violation if hasn't scanned in required interval
        if (timeSinceLastScan > requiredScanInterval)
        {
            CreateViolation(ViolationType.Minor, 
                "Failed to scan surroundings for safety", 
                timeSinceLastScan);
            ShowHint("Remember to regularly check your surroundings");
            timeSinceLastScan = 0f; // Reset to avoid spam
        }
        
        lastHeadRotation = currentRotation;
    }
    
    bool IsLookingAtCrane()
    {
        if (playerCamera == null || crane == null) return false;
        
        Vector3 directionToCrane = (crane.transform.position - playerCamera.transform.position).normalized;
        float angle = Vector3.Angle(playerCamera.transform.forward, directionToCrane);
        
        return angle < 30f; // Within 30-degree cone
    }
    
    public void RegisterAudioWarning(string warningId, AudioSource source, WarningType type)
    {
        AudioWarning warning = new AudioWarning
        {
            id = warningId,
            source = source,
            type = type,
            startTime = Time.time,
            position = source.transform.position
        };
        
        activeWarnings[warningId] = warning;
        awaitingResponse = true;
        currentResponseTime = 0f;
        
        Debug.Log($"Audio warning registered: {warningId} at {source.transform.position}");
    }
    
    void ProcessAudioWarnings()
    {
        if (!awaitingResponse || activeWarnings.Count == 0) return;
        
        currentResponseTime += Time.deltaTime;
        
        foreach (var warning in activeWarnings.Values)
        {
            // Check if player looked toward warning
            if (IsLookingToward(warning.position))
            {
                RecordAudioResponse(warning, currentResponseTime, true);
                awaitingResponse = false;
                break;
            }
            
            // Check if player moved away from danger
            if (HasMovedAwayFrom(warning.position))
            {
                RecordAudioResponse(warning, currentResponseTime, true);
                awaitingResponse = false;
                break;
            }
        }
        
        // Timeout after 5 seconds
        if (currentResponseTime > 5f)
        {
            foreach (var warning in activeWarnings.Values)
            {
                RecordAudioResponse(warning, currentResponseTime, false);
            }
            awaitingResponse = false;
            activeWarnings.Clear();
        }
    }
    
    bool IsLookingToward(Vector3 position)
    {
        if (playerCamera == null) return false;
        
        Vector3 direction = (position - playerCamera.transform.position).normalized;
        float angle = Vector3.Angle(playerCamera.transform.forward, direction);
        
        return angle < 45f;
    }
    
    bool HasMovedAwayFrom(Vector3 position)
    {
        float currentDistance = Vector3.Distance(player.position, position);
        float lastDistance = Vector3.Distance(lastPlayerPosition, position);
        
        return currentDistance > lastDistance + 0.5f; // Moved at least 0.5m away
    }
    
    void RecordAudioResponse(AudioWarning warning, float responseTime, bool responded)
    {
        ResponseQuality quality = ResponseQuality.NoResponse;
        
        if (responded)
        {
            if (responseTime < 0.5f) quality = ResponseQuality.Excellent;
            else if (responseTime < 1f) quality = ResponseQuality.Good;
            else if (responseTime < 2f) quality = ResponseQuality.Acceptable;
            else quality = ResponseQuality.Slow;
        }
        
        performanceData.audioResponses.Add(new AudioResponse
        {
            warningType = warning.type,
            responseTime = responseTime,
            quality = quality,
            responded = responded
        });
        
        // Save to data saver
        if (dataSaver != null)
        {
            dataSaver.RecordAudioResponse(
                warning.type.ToString(),
                responseTime,
                responded,
                quality.ToString(),
                warning.position
            );
        }
        
        if (!responded || quality == ResponseQuality.Slow)
        {
            CreateViolation(ViolationType.Major, 
                $"Failed to respond to {warning.type} warning in time", 
                responseTime);
        }
        else if (quality == ResponseQuality.Excellent || quality == ResponseQuality.Good)
        {
            ShowPositiveFeedback($"Good awareness! Response time: {responseTime:F1}s");
        }
        
        activeWarnings.Remove(warning.id);
    }
    
    void RecordPerformanceData()
    {
        // Update proximity score
        float proximityScore = CalculateProximityScore();
        performanceData.proximityScores.Add(proximityScore);
        
        // Update awareness score
        float awarenessScore = CalculateAwarenessScore();
        performanceData.awarenessScores.Add(awarenessScore);
        
        // Update position tracking
        performanceData.positionHistory.Add(new PositionData
        {
            position = player.position,
            rotation = player.rotation,
            timestamp = Time.time
        });
        
        // Limit history size
        if (performanceData.positionHistory.Count > 1000)
        {
            performanceData.positionHistory.RemoveAt(0);
        }
        
        lastPlayerPosition = player.position;
    }
    
    float CalculateProximityScore()
    {
        float distance = performanceData.lastProximityDistance;
        
        if (distance < 0) return 0f; // Inside danger zone
        if (distance < criticalDistance) return 0.2f;
        if (distance < dangerDistance) return 0.5f;
        if (distance < cautionDistance) return 0.8f;
        
        return 1f;
    }
    
    float CalculateAwarenessScore()
    {
        float scanScore = Mathf.Clamp01(1f - (timeSinceLastScan / requiredScanInterval));
        float craneAwarenessScore = performanceData.lookedAtCraneCount > 0 ? 1f : 0.5f;
        
        return (scanScore + craneAwarenessScore) / 2f;
    }
    
    void CreateViolation(ViolationType type, string description, float value)
    {
        SafetyIncident incident = new SafetyIncident
        {
            type = type,
            description = description,
            timestamp = Time.time,
            position = player.position,
            value = value
        };
        
        incidents.Add(incident);
        performanceData.incidents.Add(incident);
        
        // Save to data saver
        if (dataSaver != null)
        {
            dataSaver.RecordIncident(type, description, player.position, value);
        }
        
        switch (type)
        {
            case ViolationType.Minor:
                minorViolations++;
                break;
            case ViolationType.Major:
                majorViolations++;
                break;
            case ViolationType.Critical:
                criticalViolations++;
                PauseSimulation();
                break;
        }
        
        Debug.Log($"Safety Violation ({type}): {description}");
    }
    
    void ShowImmediateWarning(string message)
    {
        // This would create a prominent UI warning
        Debug.LogWarning($"IMMEDIATE WARNING: {message}");
        
        // Flash screen effect
        StartCoroutine(FlashWarning());
    }
    
    void ShowWarning(string message)
    {
        Debug.LogWarning($"WARNING: {message}");
    }
    
    void ShowHint(string message)
    {
        Debug.Log($"HINT: {message}");
    }
    
    void ShowPositiveFeedback(string message)
    {
        Debug.Log($"GOOD: {message}");
    }
    
    IEnumerator FlashWarning()
    {
        // Create a full-screen flash effect
        GameObject flash = new GameObject("Warning Flash");
        flash.transform.SetParent(uiCanvas.transform);
        
        UnityEngine.UI.Image flashImage = flash.AddComponent<UnityEngine.UI.Image>();
        flashImage.color = new Color(1f, 0f, 0f, 0.5f);
        
        RectTransform rect = flash.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
        
        // Fade out
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0.5f, 0f, elapsed / duration);
            flashImage.color = new Color(1f, 0f, 0f, alpha);
            yield return null;
        }
        
        Destroy(flash);
    }
    
    void PauseSimulation()
    {
        Time.timeScale = 0f;
        ShowPauseDialog("Critical safety violation! Review the situation before continuing.");
    }
    
    void ShowPauseDialog(string message)
    {
        // This would show a dialog requiring user acknowledgment
        Debug.LogError($"SIMULATION PAUSED: {message}");
        
        // In a real implementation, show UI dialog
        // For now, resume after 3 seconds
        StartCoroutine(AutoResume());
    }
    
    IEnumerator AutoResume()
    {
        yield return new WaitForSecondsRealtime(3f);
        Time.timeScale = 1f;
    }
    
    public PerformanceReport GenerateReport()
    {
        PerformanceReport report = new PerformanceReport();
        
        // Calculate averages
        report.averageProximityScore = performanceData.proximityScores.Count > 0 
            ? performanceData.proximityScores.Average() : 0f;
        
        report.averageAwarenessScore = performanceData.awarenessScores.Count > 0
            ? performanceData.awarenessScores.Average() : 0f;
        
        report.averageResponseTime = performanceData.audioResponses.Count > 0
            ? performanceData.audioResponses.Average(r => r.responseTime) : 0f;
        
        // Calculate overall score
        report.overallScore = (report.averageProximityScore * 0.4f) +
                             (report.averageAwarenessScore * 0.3f) +
                             (1f - Mathf.Clamp01(report.averageResponseTime / 2f)) * 0.3f;
        
        // Violation summary
        report.minorViolations = minorViolations;
        report.majorViolations = majorViolations;
        report.criticalViolations = criticalViolations;
        
        // Recommendations
        report.recommendations = GenerateRecommendations();
        
        return report;
    }
    
    List<string> GenerateRecommendations()
    {
        List<string> recommendations = new List<string>();
        
        if (performanceData.proximityScores.Count > 0 && performanceData.proximityScores.Average() < 0.7f)
        {
            recommendations.Add("Practice maintaining safe distances from equipment");
        }
        
        if (performanceData.lookedAtCraneCount < 5)
        {
            recommendations.Add("Remember to regularly check overhead hazards");
        }
        
        if (performanceData.audioResponses.Any(r => r.quality == ResponseQuality.Slow))
        {
            recommendations.Add("Improve reaction time to audio warnings");
        }
        
        if (timeSinceLastScan > requiredScanInterval * 2)
        {
            recommendations.Add("Develop habit of regular 360-degree scanning");
        }
        
        return recommendations;
    }
    
    void OnDestroy()
    {
        isMonitoring = false;
    }
    
    void OnDrawGizmos()
    {
        if (crane == null) return;
        
        // Draw safety zones
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        DrawCircle(crane.transform.position, criticalDistance, 20);
        
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        DrawCircle(crane.transform.position, dangerDistance, 30);
        
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        DrawCircle(crane.transform.position, cautionDistance, 40);
        
        // Draw overall zone
        Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
        Gizmos.DrawWireSphere(crane.transform.position, zoneRadius);
    }
    
    void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}

// Data structures
[System.Serializable]
public class PerformanceData
{
    public List<float> proximityScores = new List<float>();
    public List<float> awarenessScores = new List<float>();
    public List<AudioResponse> audioResponses = new List<AudioResponse>();
    public List<SafetyIncident> incidents = new List<SafetyIncident>();
    public List<PositionData> positionHistory = new List<PositionData>();
    
    public float lastProximityDistance;
    public float lastScanTime;
    public float lastCraneLookTime;
    public int lookedAtCraneCount;
}

[System.Serializable]
public class SafetyIncident
{
    public ViolationType type;
    public string description;
    public float timestamp;
    public Vector3 position;
    public float value;
}

[System.Serializable]
public class AudioWarning
{
    public string id;
    public AudioSource source;
    public WarningType type;
    public float startTime;
    public Vector3 position;
}

[System.Serializable]
public class AudioResponse
{
    public WarningType warningType;
    public float responseTime;
    public ResponseQuality quality;
    public bool responded;
}

[System.Serializable]
public class PositionData
{
    public Vector3 position;
    public Quaternion rotation;
    public float timestamp;
}

[System.Serializable]
public class PerformanceReport
{
    public float overallScore;
    public float averageProximityScore;
    public float averageAwarenessScore;
    public float averageResponseTime;
    
    public int minorViolations;
    public int majorViolations;
    public int criticalViolations;
    
    public List<string> recommendations = new List<string>();
}

// Enums
public enum ViolationType
{
    Minor,
    Major,
    Critical
}

public enum WarningType
{
    BackupAlarm,
    CraneMovement,
    GeneralHazard,
    EmergencyStop
}

public enum ResponseQuality
{
    Excellent,
    Good,
    Acceptable,
    Slow,
    NoResponse
}