using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement_2D_Fn : MonoBehaviour
{
    [SerializeField] private float speed = 7f;
    [SerializeField] private float jump = 8f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    private Rigidbody rb;
    private bool isGrounded;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.FreezePositionZ
                       | RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationY
                       | RigidbodyConstraints.FreezeRotationZ;
    }
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
        float moveX = Input.GetAxis("Horizontal");
        Vector3 vel = rb.linearVelocity;
        vel.x = moveX * speed;
        rb.linearVelocity = vel;

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jump, 0f);
        }
    }
    void LateUpdate()
    {
        Vector3 pos = transform.position;
        pos.z = 0f;
        transform.position = pos;
    }
}
