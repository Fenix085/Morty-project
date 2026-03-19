using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform playerPivot;
    [SerializeField] private InputActionAsset inputActions;
    private Rigidbody rb;

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
    [SerializeField] private GameObject mesh;

    private Vector3 playerVelocity = Vector3.zero;

    public static PlayerMovement Instance { get; private set; }

    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if(Instance == null)
            Instance = this;

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
        //rb.AddForce((Vector3.zero - transform.position).normalized * 10);

        Vector3 gravityDir = -(transform.position - Vector3.zero).normalized;
        Vector3 bodyUp = transform.up;
        // apply gravity to objects rigidbody
        rb.AddForce(gravityDir * 9);
        // update the objects rotation in relation to the planet
        // Lock rotation around player's up axis: align up vector, preserve forward projected onto up plane
        Vector3 forwardProjected = Vector3.ProjectOnPlane(transform.forward, -gravityDir).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(forwardProjected, -gravityDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 100 * Time.deltaTime);
        Quaternion targetRotation2;
        if(playerVelocity != Vector3.zero)
            targetRotation2 = Quaternion.LookRotation(playerVelocity, -gravityDir);
        else
            targetRotation2 = Quaternion.LookRotation(forwardProjected, -gravityDir);
        mesh.transform.rotation = targetRotation2;
    }
    void FixedUpdate()
    {
        if (playerVelocity.magnitude < 0.001f)
            return;
        rb.MovePosition(rb.position + transform.TransformDirection(playerVelocity * Time.deltaTime));
    }

    private void MoveOnSphere()
    {
        
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
        float smove = -moveInput.y;

        Vector3.Normalize(forward);
        Vector3.Normalize(right);

        for (int i = 0; i < 3; i++)
            wishvel[i] = forward[i] * smove + right[i] * fmove;

        wishdir = wishvel;
        wishspeed = wishdir.magnitude * speed;
        wishdir = wishdir.normalized;

        if (wishspeed > MAX_SPEED)
        {
            VectorScale(wishvel, MAX_SPEED / wishspeed, wishvel);
            wishspeed = MAX_SPEED;
        }
        Friction();
        Accelerate(wishdir, wishspeed);
    }

    private void Accelerate(Vector3 wishDir, float wishSpeed)
    {
        
        float wishSpd = wishSpeed;

        if (wishSpd > airMaxSpeed)
            wishSpd = airMaxSpeed;

        Vector3 currentHorizontalVel = new Vector3(playerVelocity[0], 0, playerVelocity[2]);
        float currentSpeed = currentHorizontalVel.magnitude;

        float targetSpeed = wishSpd;

        if(currentSpeed > targetSpeed)
            targetSpeed = currentSpeed;

        if (targetSpeed > airMaxSpeed)
        {
            targetSpeed = Mathf.Lerp(currentSpeed, airMaxSpeed, Time.deltaTime * 2f);
        }

        if(wishSpd > 0)
        {
            playerVelocity[0] = Mathf.Lerp(playerVelocity[0], (wishDir * targetSpeed)[0], airMaxSpeed * Time.deltaTime);
            playerVelocity[2] = Mathf.Lerp(playerVelocity[2], (wishDir * targetSpeed)[2], airMaxSpeed * Time.deltaTime);
        }
    }

    private void Friction()
    {
        //ref float vel;
        float control, drop, newspeed;

        float speed = playerVelocity.magnitude;

        if (speed < 0.01)
        {
            playerVelocity = Vector3.zero;
            return;
        }

        drop = 0;

        control = speed < stopSpeed ? stopSpeed : speed;
        drop += control * friction * Time.deltaTime;
        

        newspeed = speed - drop;
        if (newspeed < 0)
            newspeed = 0;
        newspeed /= speed;


        playerVelocity[0] *= newspeed;
        playerVelocity[2] *= newspeed;
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