using UnityEngine;
using System.Collections;

public class CraneSafetyZone : MonoBehaviour
{
    [Header("Crane Components")]
    public Transform craneBase;
    public Transform craneBoom;
    public Transform craneHook;
    public GameObject loadObject;
    
    [Header("Safety Configuration")]
    public float swingRadius = 10f;
    public float boomRotationSpeed = 5f;
    public bool simulateCraneMovement = true;
    
    [Header("Audio Warnings")]
    public AudioClip craneAlarmSound;
    public AudioClip backupAlarmSound;
    public AudioSource audioSource;
    
    [Header("AI Monitor Reference")]
    public SafetyAIMonitor aiMonitor;
    
    // Animation state
    private bool isRotating = false;
    private float currentRotation = 0f;
    private float targetRotation = 0f;
    private bool alarmPlaying = false;
    
    void Start()
    {
        // Debugging

        Debug.Log("CraneSafetyZone Start() called");

        // Find AI Monitor if not assigned
        if (aiMonitor == null)
        {
            aiMonitor = FindObjectOfType<SafetyAIMonitor>();
        }
        
        // Setup audio source if not assigned
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.volume = 0.7f;
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.maxDistance = 50f;
        }
        
        // Start crane simulation if enabled
        if (simulateCraneMovement)
        {
            StartCoroutine(CraneMovementSimulation());
        }
    }
    
    IEnumerator CraneMovementSimulation()
    {

        Debug.Log("Crane movement simulation corooute started");
        while (true)
        {

            Debug.Log("Crane simulation loop iteration");
            
            // Wait for random interval
            yield return new WaitForSeconds(Random.Range(5f, 15f));

            // Start crane rotation
            StartCraneRotation();

            // Wait for rotation to complete
            while (isRotating)
            {
                yield return null;
            }

            // Simulate load movement
            yield return StartCoroutine(SimulateLoadMovement());
        }
    }
    
    void StartCraneRotation()
    {
        isRotating = true;
        targetRotation = currentRotation + Random.Range(45f, 180f);
        
        // Play crane alarm
        PlayCraneAlarm();
        
        // Notify AI monitor
        if (aiMonitor != null)
        {
            aiMonitor.RegisterAudioWarning(
                "crane_movement_" + Time.time,
                audioSource,
                WarningType.CraneMovement
            );
        }
        
        Debug.Log("Crane starting rotation from " + currentRotation + " to " + targetRotation);
    }
    
    void Update()
    {
        
        // Test key
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("C key pressed - testing crane rotation");
            TestCraneRotation();
        }

        if (isRotating)
        {
            // Rotate crane
            currentRotation = Mathf.MoveTowards(
                currentRotation,
                targetRotation,
                boomRotationSpeed * Time.deltaTime
            );

            if (craneBoom != null)
            {
                craneBoom.localRotation = Quaternion.Euler(0, currentRotation, 0);
            }

            // Check if rotation complete
            if (Mathf.Abs(currentRotation - targetRotation) < 0.1f)
            {
                isRotating = false;
                StopCraneAlarm();
                Debug.Log("Crane rotation complete");
            }
        }
    }
    
    void TestCraneRotation()
    {
        if (craneBoom != null)
        {
            Debug.Log("Rotating crane boom manually");
            craneBoom.Rotate(0, 10f, 0); // Rotate 10 degrees
        }
        else
        {
            Debug.Log("Crane boom is null!");
        }
    }


    IEnumerator SimulateLoadMovement()
    {
        if (craneHook == null || loadObject == null) yield break;
        
        // Lower the load
        Vector3 startPos = craneHook.position;
        Vector3 targetPos = startPos - Vector3.up * 5f;
        
        float duration = 3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            if (loadObject != null)
            {
                loadObject.transform.position = Vector3.Lerp(startPos, targetPos, t);
            }
            
            yield return null;
        }
        
        // Wait
        yield return new WaitForSeconds(2f);
        
        // Raise the load back up
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            if (loadObject != null)
            {
                loadObject.transform.position = Vector3.Lerp(targetPos, startPos, t);
            }
            
            yield return null;
        }
    }
    
    void PlayCraneAlarm()
    {
        if (audioSource != null && craneAlarmSound != null)
        {
            audioSource.clip = craneAlarmSound;
            audioSource.loop = true;
            audioSource.Play();
            alarmPlaying = true;
        }
    }
    
    void StopCraneAlarm()
    {
        if (audioSource != null && alarmPlaying)
        {
            audioSource.Stop();
            alarmPlaying = false;
        }
    }
    
    public void TriggerBackupAlarm(Vector3 vehiclePosition)
    {
        // Create temporary audio source for backup alarm
        GameObject alarmObject = new GameObject("Backup Alarm");
        alarmObject.transform.position = vehiclePosition;
        
        AudioSource backupSource = alarmObject.AddComponent<AudioSource>();
        backupSource.clip = backupAlarmSound;
        backupSource.volume = 0.8f;
        backupSource.spatialBlend = 1f;
        backupSource.rolloffMode = AudioRolloffMode.Linear;
        backupSource.maxDistance = 30f;
        backupSource.Play();
        
        // Register with AI monitor
        if (aiMonitor != null)
        {
            aiMonitor.RegisterAudioWarning(
                "backup_alarm_" + Time.time,
                backupSource,
                WarningType.BackupAlarm
            );
        }
        
        // Destroy after sound completes
        Destroy(alarmObject, backupAlarmSound.length);
    }
    
    public float GetCurrentSwingRadius()
    {
        // Calculate actual swing radius based on boom angle and length
        if (craneBoom != null)
        {
            // This could be more complex based on boom extension
            return swingRadius;
        }
        
        return swingRadius;
    }
    
    void OnDrawGizmos()
    {
        if (craneBase == null) return;
        
        // Draw swing radius
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        DrawSwingRadius();
        
        // Draw boom direction
        if (craneBoom != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 boomEnd = craneBoom.position + craneBoom.forward * swingRadius;
            Gizmos.DrawLine(craneBoom.position, boomEnd);
        }
        
        // Draw load path
        if (craneHook != null && loadObject != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(craneHook.position, loadObject.transform.position);
        }
    }
    
    void DrawSwingRadius()
    {
        int segments = 50;
        float angleStep = 360f / segments;
        Vector3 center = craneBase.position;
        Vector3 prevPoint = center + new Vector3(swingRadius, 0, 0);
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(
                Mathf.Cos(angle) * swingRadius, 
                0, 
                Mathf.Sin(angle) * swingRadius
            );
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}