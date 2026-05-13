using UnityEngine;

public class TestyPlayer : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 velocity;
    private Vector3 inputDirection;
    [SerializeField] private Transform model;
    [SerializeField] private float speed = 10;
    [SerializeField] private float acceleration = 10;
    [SerializeField] private float rotationSpeed = 10;
    [SerializeField] private float jumpForce = 10;
    [SerializeField] private float groundDrag = 5;
    [SerializeField] private float airDrag = 0.1f;
    private Animator animator;
    [SerializeField] private Transform cam;
    private bool isGrounded;
    [SerializeField] private float groundDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponentInChildren<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
        // Check if grounded
        CheckIfGrounded();

        // Just read input, don't modify velocity
        inputDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        
        if(inputDirection.magnitude > 0)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection, model.up);
            model.rotation = Quaternion.Lerp(model.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            
        }

        
        
        // Jump input
        if(Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
        else
        {
            if(inputDirection.magnitude > 0 && isGrounded)
            {
                animator.Play("Walk");
            }
            else if (isGrounded)
            {
                animator.Play("Idle");
            }
            else if (!isGrounded)
            {
                animator.Play("Jump");
            }
        }
        
        cam.transform.position = Vector3.Lerp(cam.transform.position, transform.position, Time.deltaTime * 3);
    }

    void FixedUpdate()
    {
        // Apply drag based on grounded state
        rb.linearDamping = isGrounded ? groundDrag : airDrag;

        // Apply acceleration toward desired velocity (horizontal only)
        Vector3 targetVelocity = inputDirection.normalized * speed;
        velocity.x = Mathf.Lerp(velocity.x, targetVelocity.x, Time.fixedDeltaTime * acceleration);
        velocity.z = Mathf.Lerp(velocity.z, targetVelocity.z, Time.fixedDeltaTime * acceleration);
        
        // Preserve gravity from rigidbody
        velocity.y = rb.linearVelocity.y;
        
        rb.linearVelocity = velocity;
    }

    void CheckIfGrounded()
    {
        // Raycast slightly above the player's center downward to avoid hitting walls
        Vector3 origin = transform.position + Vector3.up * 0.2f;
        float castDist = groundDistance + 0.2f;
        isGrounded = Physics.Raycast(origin, Vector3.down, castDist, groundLayer);
    }

    void Jump()
    {
        animator.Play("Jump");
        // Apply jump force upward
        Debug.Log("Jump");
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
    }
}
