using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

[System.Serializable]
public class SafetySessionData
{
    public string sessionId;
    public string playerName;
    public DateTime sessionStartTime;
    public DateTime sessionEndTime;
    public float totalDuration;
    public string scenarioName;
    
    // Performance Scores
    public float overallSafetyScore;
    public float averageProximityScore;
    public float averageAwarenessScore;
    public float averageResponseTime;
    
    // Violation Counts
    public int totalViolations;
    public int minorViolations;
    public int majorViolations;
    public int criticalViolations;
    
    // Detailed Incidents
    public List<SafetyIncidentData> incidents = new List<SafetyIncidentData>();
    
    // Audio Response Data
    public List<AudioResponseData> audioResponses = new List<AudioResponseData>();
    
    // Movement Tracking
    public List<PlayerPositionData> positionHistory = new List<PlayerPositionData>();
    
    // Performance Metrics
    public float timeInDangerZone;
    public float timeInCautionZone;
    public float totalScanningTime;
    public int craneAwarenessChecks;
    public float bestResponseTime;
    public float worstResponseTime;
    
    // Recommendations
    public List<string> recommendations = new List<string>();
    
    // Session Settings
    public SafetySettings sessionSettings;
}

[System.Serializable]
public class SafetyIncidentData
{
    public string incidentId;
    public string violationType; // Minor, Major, Critical
    public string description;
    public float timestamp;
    public Vector3 playerPosition;
    public float distanceToHazard;
    public string hazardType;
}

[System.Serializable]
public class AudioResponseData
{
    public string warningId;
    public string warningType;
    public float responseTime;
    public bool playerResponded;
    public string responseQuality;
    public Vector3 warningPosition;
    public float timestamp;
}

[System.Serializable]
public class PlayerPositionData
{
    public Vector3 position;
    public Vector3 rotation;
    public float timestamp;
    public float distanceToHazard;
    public string currentZone; // Safe, Caution, Danger, Critical
}

[System.Serializable]
public class SafetySettings
{
    public float criticalDistance;
    public float dangerDistance;
    public float cautionDistance;
    public float monitoringInterval;
    public float requiredScanInterval;
}

public class SafetyDataSaver : MonoBehaviour
{
    [Header("Save Settings")]
    public string playerName = "Player1";
    public string scenarioName = "Crane Safety Training";
    public bool autoSaveOnExit = true;
    public bool saveDetailedPositionHistory = true;
    public float positionSaveInterval = 1f;
    
    [Header("File Settings")]
    public string saveDirectory = "SafetyTrainingData";
    public bool useTimestampInFilename = true;
    
    private SafetySessionData currentSessionData;
    private SafetyAIMonitor aiMonitor;
    private bool sessionActive = false;
    private float sessionStartTime;
    private float lastPositionSaveTime;
    
    void Start()
    {
        // Find the AI Monitor
        aiMonitor = FindObjectOfType<SafetyAIMonitor>();
        if (aiMonitor == null)
        {
            Debug.LogError("SafetyDataSaver: No SafetyAIMonitor found in scene!");
            return;
        }
        
        StartNewSession();
    }
    
    void StartNewSession()
    {
        sessionActive = true;
        sessionStartTime = Time.time;
        
        // Initialize session data
        currentSessionData = new SafetySessionData();
        currentSessionData.sessionId = System.Guid.NewGuid().ToString();
        currentSessionData.playerName = playerName;
        currentSessionData.sessionStartTime = DateTime.Now;
        currentSessionData.scenarioName = scenarioName;
        
        // Store initial settings
        currentSessionData.sessionSettings = new SafetySettings
        {
            criticalDistance = aiMonitor.criticalDistance,
            dangerDistance = aiMonitor.dangerDistance,
            cautionDistance = aiMonitor.cautionDistance,
            monitoringInterval = aiMonitor.monitoringInterval,
            requiredScanInterval = aiMonitor.requiredScanInterval
        };
        
        Debug.Log($"Safety training session started: {currentSessionData.sessionId}");
    }
    
    void Update()
    {
        if (!sessionActive || aiMonitor == null) return;
        
        // Save position data periodically
        if (saveDetailedPositionHistory && Time.time - lastPositionSaveTime > positionSaveInterval)
        {
            RecordCurrentPosition();
            lastPositionSaveTime = Time.time;
        }
        
        // Check for end session input
        if (Input.GetKeyDown(KeyCode.F12))
        {
            EndSessionAndSave();
        }
    }
    
    void RecordCurrentPosition()
    {
        if (aiMonitor.player == null) return;
        
        PlayerPositionData posData = new PlayerPositionData
        {
            position = aiMonitor.player.position,
            rotation = aiMonitor.player.eulerAngles,
            timestamp = Time.time - sessionStartTime,
            distanceToHazard = aiMonitor.performanceData.lastProximityDistance,
            currentZone = GetCurrentZone(aiMonitor.performanceData.lastProximityDistance)
        };
        
        currentSessionData.positionHistory.Add(posData);
    }
    
    string GetCurrentZone(float distance)
    {
        if (distance < aiMonitor.criticalDistance) return "Critical";
        if (distance < aiMonitor.dangerDistance) return "Danger";
        if (distance < aiMonitor.cautionDistance) return "Caution";
        return "Safe";
    }
    
    public void RecordIncident(ViolationType type, string description, Vector3 position, float distance)
    {
        if (!sessionActive) return;
        
        SafetyIncidentData incident = new SafetyIncidentData
        {
            incidentId = System.Guid.NewGuid().ToString(),
            violationType = type.ToString(),
            description = description,
            timestamp = Time.time - sessionStartTime,
            playerPosition = position,
            distanceToHazard = distance,
            hazardType = "Crane"
        };
        
        currentSessionData.incidents.Add(incident);
    }
    
    public void RecordAudioResponse(string warningType, float responseTime, bool responded, string quality, Vector3 warningPos)
    {
        if (!sessionActive) return;
        
        AudioResponseData audioData = new AudioResponseData
        {
            warningId = System.Guid.NewGuid().ToString(),
            warningType = warningType,
            responseTime = responseTime,
            playerResponded = responded,
            responseQuality = quality,
            warningPosition = warningPos,
            timestamp = Time.time - sessionStartTime
        };
        
        currentSessionData.audioResponses.Add(audioData);
    }
    
    public void EndSessionAndSave()
    {
        if (!sessionActive) return;
        
        sessionActive = false;
        
        // Finalize session data
        currentSessionData.sessionEndTime = DateTime.Now;
        currentSessionData.totalDuration = Time.time - sessionStartTime;
        
        // Get final performance report from AI Monitor
        PerformanceReport report = aiMonitor.GenerateReport();
        currentSessionData.overallSafetyScore = report.overallScore;
        currentSessionData.averageProximityScore = report.averageProximityScore;
        currentSessionData.averageAwarenessScore = report.averageAwarenessScore;
        currentSessionData.averageResponseTime = report.averageResponseTime;
        
        // Violation counts
        currentSessionData.minorViolations = report.minorViolations;
        currentSessionData.majorViolations = report.majorViolations;
        currentSessionData.criticalViolations = report.criticalViolations;
        currentSessionData.totalViolations = report.minorViolations + report.majorViolations + report.criticalViolations;
        
        // Additional metrics
        CalculateAdditionalMetrics();
        
        // Recommendations
        currentSessionData.recommendations = report.recommendations;
        
        // Save to JSON
        SaveToJSON();
    }
    
    void CalculateAdditionalMetrics()
    {
        currentSessionData.timeInDangerZone = 0f;
        currentSessionData.timeInCautionZone = 0f;
        currentSessionData.craneAwarenessChecks = aiMonitor.performanceData.lookedAtCraneCount;
        
        // Calculate time spent in different zones
        foreach (var pos in currentSessionData.positionHistory)
        {
            switch (pos.currentZone)
            {
                case "Danger":
                case "Critical":
                    currentSessionData.timeInDangerZone += positionSaveInterval;
                    break;
                case "Caution":
                    currentSessionData.timeInCautionZone += positionSaveInterval;
                    break;
            }
        }
        
        // Calculate best/worst response times
        if (currentSessionData.audioResponses.Count > 0)
        {
            currentSessionData.bestResponseTime = float.MaxValue;
            currentSessionData.worstResponseTime = 0f;
            
            foreach (var response in currentSessionData.audioResponses)
            {
                if (response.playerResponded)
                {
                    if (response.responseTime < currentSessionData.bestResponseTime)
                        currentSessionData.bestResponseTime = response.responseTime;
                    if (response.responseTime > currentSessionData.worstResponseTime)
                        currentSessionData.worstResponseTime = response.responseTime;
                }
            }
            
            if (currentSessionData.bestResponseTime == float.MaxValue)
                currentSessionData.bestResponseTime = 0f;
        }
    }
    
    void SaveToJSON()
    {
        try
        {
            // Create directory if it doesn't exist
            string fullPath = Path.Combine(Application.persistentDataPath, saveDirectory);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            
            // Create filename
            string filename = $"{playerName}_{scenarioName}";
            if (useTimestampInFilename)
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                filename += $"_{timestamp}";
            }
            filename += ".json";
            
            string filePath = Path.Combine(fullPath, filename);
            
            // Convert to JSON
            string json = JsonUtility.ToJson(currentSessionData, true);
            
            // Write to file
            File.WriteAllText(filePath, json);
            
            Debug.Log($"Safety training data saved to: {filePath}");
            Debug.Log($"Session Summary:");
            Debug.Log($"- Duration: {currentSessionData.totalDuration:F1}s");
            Debug.Log($"- Overall Score: {(currentSessionData.overallSafetyScore * 100):F1}%");
            Debug.Log($"- Total Violations: {currentSessionData.totalViolations}");
            Debug.Log($"- Position Records: {currentSessionData.positionHistory.Count}");
            
            // Show save notification
            ShowSaveNotification(filePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save safety data: {e.Message}");
        }
    }
    
    void ShowSaveNotification(string filePath)
    {
        // This could create a UI notification
        Debug.Log($"💾 Data saved successfully!");
        Debug.Log($"📁 File location: {filePath}");
    }
    
    // Manual save trigger
    [ContextMenu("Save Current Session")]
    public void ManualSave()
    {
        EndSessionAndSave();
    }
    
    // Auto-save when application closes
    void OnApplicationPause(bool pauseStatus)
    {
        if (autoSaveOnExit && pauseStatus && sessionActive)
        {
            EndSessionAndSave();
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (autoSaveOnExit && !hasFocus && sessionActive)
        {
            EndSessionAndSave();
        }
    }
    
    void OnDestroy()
    {
        if (autoSaveOnExit && sessionActive)
        {
            EndSessionAndSave();
        }
    }
}