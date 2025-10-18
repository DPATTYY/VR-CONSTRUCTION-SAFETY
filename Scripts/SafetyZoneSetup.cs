using UnityEngine;

public class SafetyZoneSetup : MonoBehaviour
{
    [Header("Quick Setup")]
    public bool autoSetup = true;
    public GameObject craneObject;
    public GameObject constructionWorker;
    
    [Header("Test Scenarios")]
    public bool testBackupAlarm = false;
    public bool testCraneMovement = false;
    public bool showDebugInfo = true;
    
    private SafetyAIMonitor aiMonitor;
    private CraneSafetyZone craneSafety;
    private SafetyUIManager uiManager;
    
    void Start()
    {
        if (autoSetup)
        {
            SetupSafetySystem();
        }
    }
    
    void SetupSafetySystem()
    {
        // Find crane if not assigned
        if (craneObject == null)
        {
            craneObject = GameObject.Find("Crane");
            if (craneObject == null)
            {
                Debug.LogError("No crane found! Please assign or name your crane GameObject 'Crane'");
                return;
            }
        }
        
        // Find construction worker if not assigned
        if (constructionWorker == null)
        {
            constructionWorker = GameObject.Find("Construction Worker");
            if (constructionWorker == null)
            {
                Debug.LogError("No construction worker found!");
                return;
            }
        }
        
        // Add AI Monitor to crane
        aiMonitor = craneObject.GetComponent<SafetyAIMonitor>();
        if (aiMonitor == null)
        {
            aiMonitor = craneObject.AddComponent<SafetyAIMonitor>();
            Debug.Log("Added SafetyAIMonitor to crane");
        }
        
        // Configure AI Monitor
        aiMonitor.crane = craneObject;
        aiMonitor.player = constructionWorker.transform;
        
        // Try to find camera
        Camera playerCamera = constructionWorker.GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        aiMonitor.playerCamera = playerCamera;
        
        // Add Crane Safety Zone
        craneSafety = craneObject.GetComponent<CraneSafetyZone>();
        if (craneSafety == null)
        {
            craneSafety = craneObject.AddComponent<CraneSafetyZone>();
            craneSafety.craneBase = craneObject.transform;
            craneSafety.aiMonitor = aiMonitor;
            Debug.Log("Added CraneSafetyZone to crane");
        }
        
        // Setup UI Manager
        GameObject uiObject = GameObject.Find("Safety UI Manager");
        if (uiObject == null)
        {
            uiObject = new GameObject("Safety UI Manager");
        }
        
        uiManager = uiObject.GetComponent<SafetyUIManager>();
        if (uiManager == null)
        {
            uiManager = uiObject.AddComponent<SafetyUIManager>();
            Debug.Log("Added SafetyUIManager");
        }
        
        // Connect UI to AI Monitor
        aiMonitor.uiCanvas = FindObjectOfType<Canvas>();
        if (aiMonitor.uiCanvas == null)
        {
            GameObject canvasObj = new GameObject("UI Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            aiMonitor.uiCanvas = canvas;
        }
        
        Debug.Log("Safety monitoring system setup complete!");
        Debug.Log("- AI Monitor: Active");
        Debug.Log("- Crane Safety: Active");
        Debug.Log("- UI System: Active");
        Debug.Log("Walk near the crane to test the system!");
    }
    
    void Update()
    {
        // Test scenarios
        if (testBackupAlarm)
        {
            testBackupAlarm = false;
            TestBackupAlarm();
        }
        
        if (testCraneMovement)
        {
            testCraneMovement = false;
            TestCraneMovement();
        }
        
        // Debug info
        if (showDebugInfo && aiMonitor != null)
        {
            ShowDebugInfo();
        }
    }
    
    void TestBackupAlarm()
    {
        if (craneSafety != null)
        {
            Vector3 alarmPosition = constructionWorker.transform.position + 
                                  constructionWorker.transform.forward * 10f;
            craneSafety.TriggerBackupAlarm(alarmPosition);
            Debug.Log("Backup alarm triggered at: " + alarmPosition);
        }
    }
    
    void TestCraneMovement()
    {
        if (craneSafety != null)
        {
            craneSafety.simulateCraneMovement = true;
            Debug.Log("Crane movement simulation started");
        }
    }
    
    void ShowDebugInfo()
    {
        if (constructionWorker == null || craneObject == null) return;
        
        float distance = Vector3.Distance(
            constructionWorker.transform.position, 
            craneObject.transform.position
        );
        
        // Draw debug line
        Debug.DrawLine(
            constructionWorker.transform.position, 
            craneObject.transform.position, 
            GetDebugColor(distance)
        );
    }
    
    Color GetDebugColor(float distance)
    {
        if (distance < 2f) return Color.red;
        if (distance < 5f) return Color.yellow;
        if (distance < 8f) return new Color(1f, 0.5f, 0f);
        return Color.green;
    }
    
    void OnGUI()
    {
        if (!showDebugInfo || aiMonitor == null) return;
        
        // Create debug panel
        float panelWidth = 300f;
        float panelHeight = 200f;
        
        GUI.Box(new Rect(10, 10, panelWidth, panelHeight), "Safety Monitor Debug");
        
        int y = 35;
        int lineHeight = 20;
        
        if (constructionWorker != null && craneObject != null)
        {
            float distance = Vector3.Distance(
                constructionWorker.transform.position, 
                craneObject.transform.position
            );
            
            GUI.Label(new Rect(20, y, panelWidth - 40, lineHeight), 
                $"Distance to Crane: {distance:F1}m");
            y += lineHeight;
            
            // Get performance report
            PerformanceReport report = aiMonitor.GenerateReport();
            
            GUI.Label(new Rect(20, y, panelWidth - 40, lineHeight), 
                $"Safety Score: {(report.overallScore * 100):F0}%");
            y += lineHeight;
            
            GUI.Label(new Rect(20, y, panelWidth - 40, lineHeight), 
                $"Proximity Score: {(report.averageProximityScore * 100):F0}%");
            y += lineHeight;
            
            GUI.Label(new Rect(20, y, panelWidth - 40, lineHeight), 
                $"Awareness Score: {(report.averageAwarenessScore * 100):F0}%");
            y += lineHeight;
            
            GUI.Label(new Rect(20, y, panelWidth - 40, lineHeight), 
                $"Violations - Minor: {report.minorViolations}, " +
                $"Major: {report.majorViolations}, " +
                $"Critical: {report.criticalViolations}");
            y += lineHeight;
            
            // Test buttons
            y += 10;
            if (GUI.Button(new Rect(20, y, 120, 25), "Test Backup Alarm"))
            {
                TestBackupAlarm();
            }
            
            if (GUI.Button(new Rect(150, y, 120, 25), "Test Crane Move"))
            {
                TestCraneMovement();
            }
        }
    }
}