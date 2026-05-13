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
    private float uiMoveX;
    private bool jumpQueued;

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
        float keyboardMoveX = Input.GetAxis("Horizontal");
        float moveX = Mathf.Abs(uiMoveX) > Mathf.Abs(keyboardMoveX) ? uiMoveX : keyboardMoveX;
        Vector3 vel = rb.linearVelocity;
        vel.x = moveX * speed;
        rb.linearVelocity = vel;

        if ((Input.GetButtonDown("Jump") || jumpQueued) && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jump, 0f);
        }

        jumpQueued = false;
    }

    public void SetHorizontal(float value)
    {
        uiMoveX = Mathf.Clamp(value, -1f, 1f);
    }

    public void StopHorizontal()
    {
        uiMoveX = 0f;
    }

    public void PressLeft()
    {
        uiMoveX = -1f;
    }

    public void PressRight()
    {
        uiMoveX = 1f;
    }

    public void ReleaseLeft()
    {
        if (uiMoveX < 0f)
        {
            uiMoveX = 0f;
        }
    }

    public void ReleaseRight()
    {
        if (uiMoveX > 0f)
        {
            uiMoveX = 0f;
        }
    }

    public void JumpPressed()
    {
        jumpQueued = true;
    }

    void LateUpdate()
    {
        Vector3 pos = transform.position;
        pos.z = 0f;
        transform.position = pos;
    }
}
