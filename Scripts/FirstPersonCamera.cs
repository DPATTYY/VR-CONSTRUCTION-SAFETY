using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform player;
    public float mouseSensitivity = 100f;
    public bool invertY = false;
    
    [Header("Camera Position")]
    public Vector3 cameraOffset = new Vector3(0f, 1.6f, 0f); // Head height
    public bool smoothFollow = true;
    public float followSpeed = 10f;
    
    [Header("Look Constraints")]
    public float minYAngle = -90f;
    public float maxYAngle = 90f;
    
    [Header("VR Ready Settings")]
    public bool enableMouseLook = true; // Disable for VR
    public bool lockCursor = true; // Disable for VR
    
    private float xRotation = 0f;
    private Camera cam;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        
        // Find the construction worker automatically if not assigned
        if (player == null)
        {
            GameObject worker = GameObject.Find("Construction Worker");
            if (worker != null)
            {
                player = worker.transform;
                Debug.Log("Automatically found Construction Worker");
            }
            else
            {
                Debug.LogError("No Construction Worker found! Please assign the player manually.");
                return;
            }
        }
        
        // Lock cursor for FPS experience (disable for VR)
        if (lockCursor && enableMouseLook)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        // Position camera at player's head
        PositionCamera();
    }
    
    void Update()
    {
        if (player == null) return;
        
        // Handle mouse look (disable for VR)
        if (enableMouseLook)
        {
            HandleMouseLook();
        }
        
        // Follow player
        FollowPlayer();
        
        // Toggle cursor lock with Escape key
        if (Input.GetKeyDown(KeyCode.Escape) && enableMouseLook)
        {
            ToggleCursorLock();
        }
    }
    
    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        
        if (invertY)
            mouseY = -mouseY;
        
        // Rotate the player body left and right
        player.Rotate(Vector3.up * mouseX);
        
        // Rotate the camera up and down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minYAngle, maxYAngle);
        
        // Apply both horizontal (from player) and vertical (from camera) rotation
        transform.rotation = Quaternion.Euler(xRotation, player.eulerAngles.y, 0f);
    }
    
    void FollowPlayer()
    {
        Vector3 targetPosition = player.position + player.TransformDirection(cameraOffset);
        
        if (smoothFollow)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = targetPosition;
        }
        
        // Match player's Y rotation if not using mouse look
        if (!enableMouseLook)
        {
            transform.rotation = Quaternion.Euler(0, player.eulerAngles.y, 0);
        }
    }
    
    void PositionCamera()
    {
        if (player != null)
        {
            transform.position = player.position + player.TransformDirection(cameraOffset);
            transform.rotation = Quaternion.Euler(0, player.eulerAngles.y, 0);
        }
    }
    
    void ToggleCursorLock()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    
    // Call this when setting up for VR
    public void SetupForVR()
    {
        enableMouseLook = false;
        lockCursor = false;
        Cursor.lockState = CursorLockMode.None;
        smoothFollow = false; // More responsive for VR
        
        Debug.Log("Camera configured for VR mode");
    }
    
    // Reset to desktop mode
    public void SetupForDesktop()
    {
        enableMouseLook = true;
        lockCursor = true;
        Cursor.lockState = CursorLockMode.Locked;
        smoothFollow = true;
        
        Debug.Log("Camera configured for desktop mode");
    }
    
    void OnDrawGizmosSelected()
    {
        // Show camera position in Scene view
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 targetPos = player.position + player.TransformDirection(cameraOffset);
            Gizmos.DrawWireSphere(targetPos, 0.1f);
            Gizmos.DrawLine(player.position, targetPos);
        }
    }
}