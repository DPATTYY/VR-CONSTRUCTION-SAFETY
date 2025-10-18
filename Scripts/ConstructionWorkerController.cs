using UnityEngine;

public class ConstructionWorkerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    
    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask = 1; // Default layer
    
    [Header("Audio (Optional)")]
    public AudioClip[] footstepSounds;
    public AudioSource audioSource;
    
    // Private variables
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isRunning;
    private float footstepTimer;
    private Animator animator; // Optional for animations
    
    void Start()
    {
        // Get required components
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>(); // Optional
        
        // Create ground check if it doesn't exist
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = groundCheckObj.transform;
        }
        
        // Setup audio source if not assigned
        /*if (audioSource == null && footstepSounds.Length > 0)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.volume = 0.3f;
        }*/
    }
    
    void Update()
    {
        HandleGroundCheck();
        HandleMovement();
        HandleJump();
        HandleFootsteps();
        UpdateAnimations();
    }
    
    void HandleGroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small downward force to keep grounded
        }
    }
    
    void HandleMovement()
    {
        // Get input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        // Check if running (Left Shift)
        isRunning = Input.GetKey(KeyCode.LeftShift);
        
        // Calculate movement direction
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        
        if (direction.magnitude >= 0.1f)
        {
            // Rotate towards movement direction
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Lerp(transform.rotation, 
                Quaternion.Euler(0f, targetAngle, 0f), Time.deltaTime * 10f);
            
            // Move the character
            float currentSpeed = isRunning ? runSpeed : walkSpeed;
            controller.Move(direction * currentSpeed * Time.deltaTime);
        }
    }
    
    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        
        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    
    void HandleFootsteps()
    {
        // Only play footsteps when moving and grounded
        if (isGrounded && controller.velocity.magnitude > 0.1f && footstepSounds != null && footstepSounds.Length > 0)
        {
            footstepTimer += Time.deltaTime;
            
            float footstepInterval = isRunning ? 0.3f : 0.5f;
            
            if (footstepTimer >= footstepInterval)
            {
                PlayFootstepSound();
                footstepTimer = 0f;
            }
        }
    }
    
    void PlayFootstepSound()
    {
        if (audioSource != null && footstepSounds != null && footstepSounds.Length > 0)
        {
            AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
            audioSource.PlayOneShot(clip);
        }
    }
    
    void UpdateAnimations()
    {
        if (animator != null)
        {
            // Set animation parameters (if you have an Animator)
            float speed = controller.velocity.magnitude;
            animator.SetFloat("Speed", speed);
            animator.SetBool("IsRunning", isRunning);
            animator.SetBool("IsGrounded", isGrounded);
        }
    }
    
    // Visual debug in Scene view
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}