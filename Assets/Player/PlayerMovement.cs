using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform playerPivot;
    [SerializeField] private InputActionAsset inputActions;

    private InputAction moveAction;
    private InputAction jumpAction;

    [SerializeField] private float MAX_SPEED = 30f;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float accel = 11f;
    [SerializeField] private float airMaxSpeed = 2f;
    [SerializeField] private float airAccel = 11f;
    [SerializeField] private float friction = 7f;
    [SerializeField] private float stopSpeed = 0.1f;
    [SerializeField] private float jumpStrength = 10f;

    private Vector3 playerVelocity = Vector3.zero;

    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        var playerMap = inputActions.FindActionMap("Player");
        moveAction = playerMap.FindAction("Move");
    }

    private void OnEnable()
    {
        moveAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
    }

    private void Update()
    {
        AirMove();
        MoveOnSphere();
    }

    private void MoveOnSphere()
    {
        if (playerVelocity.magnitude < 0.001f)
            return;

        // Up direction is from sphere center (origin) to player
        Vector3 up = transform.position.normalized;

        // Project velocity onto the sphere's tangent plane
        Vector3 tangentVelocity = Vector3.ProjectOnPlane(playerVelocity, up);

        if (tangentVelocity.magnitude < 0.001f)
            return;

        // Rotation axis is perpendicular to both up and movement direction
        Vector3 rotationAxis = Vector3.Cross(up, tangentVelocity.normalized);

        // Angular speed = linear speed / radius
        float radius = transform.position.magnitude;
        float angularSpeed = tangentVelocity.magnitude / radius * Mathf.Rad2Deg;

        // Rotate player around sphere center
        transform.RotateAround(Vector3.zero, rotationAxis, angularSpeed * Time.deltaTime);
    }
    private void AirMove()
    {
        Vector3 wishdir;
        Vector3 wishvel = new Vector3();
        float wishspeed;

        Vector3 forward;
        Vector3 right;

        //Debug.Log(transform.forward);
        forward = playerPivot.forward;
        right = playerPivot.right;

        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        float fmove = moveInput.x;
        float smove = moveInput.y;

        Vector3.Normalize(forward);
        Vector3.Normalize(right);

        for (int i = 0; i < 3; i++)
            wishvel[i] = forward[i] * smove + right[i] * fmove;

        wishdir = wishvel;
        wishspeed = wishdir.magnitude * speed;
        Vector3.Normalize(wishdir);

        if (wishspeed > MAX_SPEED)
        {
            VectorScale(wishvel, MAX_SPEED / wishspeed, wishvel);
            wishspeed = MAX_SPEED;
        }

        Accelerate(wishdir, wishspeed);
    }

    private void Accelerate(Vector3 wishDir, float wishSpeed)
    {
        float currentSpeed, addSpeed, accelSpeed;

        currentSpeed = Vector3.Dot(playerVelocity, wishDir);
        addSpeed = wishSpeed - currentSpeed;

        if (addSpeed <= 0)
            return;

        accelSpeed = accel * Time.deltaTime * wishSpeed;

        if (accelSpeed > addSpeed)
            accelSpeed = addSpeed;

        for (int i = 0; i < 3; i++)
            playerVelocity[i] += wishDir[i] * accelSpeed;
    }



    public static void VectorScale(Vector3 in_, float scale, Vector3 out_)
    {
        out_[0] = in_[0] * scale;
        out_[1] = in_[1] * scale;
        out_[2] = in_[2] * scale;
    }


    //private void OnGUI()
    //{
    //    GUI.color = Color.green;
    //    var ups = playerVelocity;
    //    GUI.Label(new Rect(0, 15, 400, 100),
    //    "Speed: " + Mathf.Round(ups.magnitude * 100) / 100 + "ups\n" +
    //    "Velocity: " + ups + "\n" +
    //    "Grounded: " + movementController.GroundCheck());
    //}
}