using UnityEngine;

public class CreateConstructionWorker : MonoBehaviour
{
    [Header("Worker Creation")]
    [SerializeField] private bool createWorker = false;
    
    [Header("Materials (Optional)")]
    public Material bodyMaterial;
    public Material helmetMaterial;
    public Material vestMaterial;
    
    void OnValidate()
    {
        // This runs when values change in the inspector
        if (createWorker && Application.isPlaying)
        {
            CreateWorkerFromPrimitives();
            createWorker = false;
        }
    }
    
void Update()
{
    if (createWorker)
    {
        Debug.Log("Create Worker checkbox was checked!");
        CreateWorkerFromPrimitives();
        createWorker = false;
    }
}
    
    void CreateWorkerFromPrimitives()
    {   

        Debug.Log("Starting worker creation process...");


        // Add scale for the worker ~ adjust this value to make worker smaller/bigger

        float workerScale = 0.25f; // 0.5 = half size, 0.75 = 3/4 size, 1.0 = normal size

        // Check if worker already exists
        GameObject existingWorker = GameObject.Find("Construction Worker");
        if (existingWorker != null)
        {
            Debug.LogWarning("Construction Worker already exists! Deleting old one.");
            DestroyImmediate(existingWorker);
        }
        
        // Create main worker object
        GameObject worker = new GameObject("Construction Worker");
        Debug.Log("Created worker GameObject: " + worker.name);
        worker.transform.position = transform.position;
        
        // Create body (capsule)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(worker.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
        
        // Create head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(worker.transform);
        head.transform.localPosition = new Vector3(0, 1.5f, 0);
        head.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        
        // Create hard hat
        GameObject helmet = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        helmet.name = "Hard Hat";
        helmet.transform.SetParent(worker.transform);
        helmet.transform.localPosition = new Vector3(0, 1.7f, 0);
        helmet.transform.localScale = new Vector3(0.7f, 0.2f, 0.7f);
        
        // Create safety vest (flattened cube)
        GameObject vest = GameObject.CreatePrimitive(PrimitiveType.Cube);
        vest.name = "Safety Vest";
        vest.transform.SetParent(worker.transform);
        vest.transform.localPosition = new Vector3(0, 0.3f, 0);
        vest.transform.localScale = new Vector3(0.9f, 0.8f, 0.5f);
        
        // Create arms
        CreateArm(worker.transform, "Left Arm", new Vector3(-0.6f, 0.2f, 0));
        CreateArm(worker.transform, "Right Arm", new Vector3(0.6f, 0.2f, 0));
        
        // Create legs
        CreateLeg(worker.transform, "Left Leg", new Vector3(-0.3f, -1.2f, 0));
        CreateLeg(worker.transform, "Right Leg", new Vector3(0.3f, -1.2f, 0));
        
        // Apply materials if provided
        ApplyMaterials(worker);

        // Scale the entire worker
        worker.transform.localScale = Vector3.one * workerScale;


        
        // Add character controller TO THE WORKER (not this creator object)
        CharacterController controller = worker.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.5f;
        controller.center = new Vector3(0, 0, 0);
        
        // Add the movement script TO THE WORKER
        ConstructionWorkerController workerController = worker.AddComponent<ConstructionWorkerController>();
        
        // Position worker at spawn point (slightly above ground)
        worker.transform.position = transform.position + Vector3.up * 1f;
        
        Debug.Log("Construction Worker created successfully at " + worker.transform.position);
    }
    
    void CreateArm(Transform parent, string name, Vector3 position)
    {
        GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        arm.name = name;
        arm.transform.SetParent(parent);
        arm.transform.localPosition = position;
        arm.transform.localScale = new Vector3(0.3f, 0.6f, 0.3f);
        
        // Remove colliders from body parts to avoid interference
        Collider armCollider = arm.GetComponent<Collider>();
        if (armCollider != null)
        {
            Destroy(armCollider);
        }
    }
    
    void CreateLeg(Transform parent, string name, Vector3 position)
    {
        GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        leg.name = name;
        leg.transform.SetParent(parent);
        leg.transform.localPosition = position;
        leg.transform.localScale = new Vector3(0.4f, 0.8f, 0.4f);
        
        // Remove colliders from body parts to avoid interference
        Collider legCollider = leg.GetComponent<Collider>();
        if (legCollider != null)
        {
            Destroy(legCollider);
        }
    }
    
    void ApplyMaterials(GameObject worker)
    {
        // Remove colliders from non-essential body parts
        RemoveCollidersFromBodyParts(worker);
        
        // Apply body material
        if (bodyMaterial != null)
        {
            ApplyMaterialToChild(worker, "Body", bodyMaterial);
            ApplyMaterialToChild(worker, "Head", bodyMaterial);
            ApplyMaterialToChild(worker, "Left Arm", bodyMaterial);
            ApplyMaterialToChild(worker, "Right Arm", bodyMaterial);
            ApplyMaterialToChild(worker, "Left Leg", bodyMaterial);
            ApplyMaterialToChild(worker, "Right Leg", bodyMaterial);
        }
        
        // Apply helmet material
        if (helmetMaterial != null)
        {
            ApplyMaterialToChild(worker, "Hard Hat", helmetMaterial);
        }
        
        // Apply vest material
        if (vestMaterial != null)
        {
            ApplyMaterialToChild(worker, "Safety Vest", vestMaterial);
        }
    }
    
    void RemoveCollidersFromBodyParts(GameObject worker)
    {
        // Remove colliders from visual-only parts to avoid collision conflicts
        string[] bodyParts = { "Head", "Hard Hat", "Safety Vest", "Body" };
        
        foreach (string partName in bodyParts)
        {
            Transform part = worker.transform.Find(partName);
            if (part != null)
            {
                Collider partCollider = part.GetComponent<Collider>();
                if (partCollider != null)
                {
                    Destroy (partCollider);
                }
            }
        }
    }
    
    void ApplyMaterialToChild(GameObject parent, string childName, Material material)
    {
        Transform child = parent.transform.Find(childName);
        if (child != null)
        {
            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = material;
            }
        }
    }
    
    // Button in inspector to create worker
    [ContextMenu("Create Construction Worker")]
    void CreateWorkerButton()
    {
        CreateWorkerFromPrimitives();
    }
}