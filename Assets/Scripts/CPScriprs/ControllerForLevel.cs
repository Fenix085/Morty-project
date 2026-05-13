/*********************************************************************************************************
 * Class players movement
 * - Add this component to the players root object
 *********************************************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(FakeGravityBody))]
public class ControllerForLevel : MonoBehaviour {

    // inspector variables
    [SerializeField, Tooltip("Player movement speed")]
    private float speed = 10.0f;
    [SerializeField, Tooltip("Player movement speed between worlds")]
    private float transferSpeed = 10.0f;
    [SerializeField, Tooltip("Player jump force")]
    private float jumpForce = 10.0f;
    [SerializeField, Tooltip("Player landing distance")]
    private float maxJumpHeight = 10.0f;

    [SerializeField, Tooltip("Distance from world to play landing particles (percentage of distance between worlds)"), Range(0.0f, 1.0f)]
    private float landDistance = 0.4f;

    // privates
    private List<GameObject> _worlds = new List<GameObject>();
    private int _currentWorld = 0;
    private int _prevWorld = 0;
    private Vector3 _moveDirection;
    private Rigidbody _playerRB;
    private Transform _playerMesh;
    private FakeGravityBody _worldGravity;

    private Animator animator;

    // transfer
    private bool _transfering = false;
    private bool _landed = false;
    private float _worldDistance = 0;

    public static ControllerForLevel Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        _playerRB = GetComponent<Rigidbody>();
        _playerMesh = transform.GetChild(0).transform;
        _worldGravity = GetComponent<FakeGravityBody>();

        if (_worldGravity == null)
        {
            Debug.LogError("ControllerForLevel: Missing FakeGravityBody on player.", gameObject);
            enabled = false;
            return;
        }

        RefreshWorlds();

        if (_worldGravity.Attractor == null && _worlds.Count > 0)
        {
            _worldGravity.Attractor = _worlds[0].GetComponent<FakeGravity>();
        }

        if (_worldGravity.Attractor != null && !_worlds.Contains(_worldGravity.Attractor.gameObject))
        {
            _worlds.Add(_worldGravity.Attractor.gameObject);
        }

        if (_worlds.Count == 0 || _worldGravity.Attractor == null)
        {
            Debug.LogWarning("ControllerForLevel: No valid FakeGravity world found in scene.", gameObject);
            return;
        }

        _currentWorld = CurrentWorldIndex();
        speed = SpeedUpdate();
    }

    /// <summary>
    /// Rebuild world list using actual gravity sources in scene.
    /// </summary>
    private void RefreshWorlds()
    {
        _worlds.Clear();
        FakeGravity[] gravitySources = FindObjectsByType<FakeGravity>(FindObjectsSortMode.None);
        int count = gravitySources.Length;
        for (int i = 0; i < count; i++)
        {
            _worlds.Add(gravitySources[i].gameObject);
        }
    }

    private void Update()
    {
        if (_transfering)
            return;

        Vector2 mobileInput = MobileJoystick.Instance != null ? MobileJoystick.Instance.Value : Vector2.zero;
        Vector2 keyboardInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Vector2 moveInput = mobileInput.sqrMagnitude > keyboardInput.sqrMagnitude ? mobileInput : keyboardInput;

        _moveDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;

        RotateForward();
    }

    private void FixedUpdate()
    {
        if (_transfering)
            return;

        if (_moveDirection.magnitude > 0)
            animator.Play("Walk");
        else
            animator.Play("Idle");

        if (CanMove(_moveDirection))
        {
            Vector3 targetVelocity = transform.TransformDirection(_moveDirection * speed);
            _playerRB.linearVelocity = new Vector3(targetVelocity.x, _playerRB.linearVelocity.y, targetVelocity.z);
        }
        else
        {
            _playerRB.linearVelocity = Vector3.zero;
        }

        _playerRB.angularVelocity = Vector3.zero;
    }

    /// <summary>
    /// Player Jump
    /// </summary>
    private void Jump()
    {
        float jumpHeight = Vector3.Distance(_worlds[_currentWorld].transform.position, transform.position) - maxJumpHeight;
        if (jumpHeight < maxJumpHeight)
        {
            Vector3 gravityDir = (_worlds[_currentWorld].transform.position - transform.position).normalized;
            _playerRB.AddForce(-gravityDir * jumpForce, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// Initialise player transfer between worlds
    /// </summary>
    private void WorldTransfer()
    {
        Jump();
        _worldGravity.Attractor = null;

        if (_currentWorld + 1 >= _worlds.Count)
        {
            _prevWorld = _currentWorld;
            _currentWorld = 0;
        }
        else
        {
            _prevWorld = _currentWorld;
            _currentWorld++;
        }

        _transfering = true;
        _worldDistance = Vector3.Distance(_worlds[_prevWorld].transform.position, _worlds[_currentWorld].transform.position) -
                         (_worlds[_prevWorld].GetComponent<FakeGravity>().WorldSize + _worlds[_currentWorld].GetComponent<FakeGravity>().WorldSize);

        StartCoroutine("ChangeWorlds");
    }

    /// <summary>
    /// Coroutine that controls changing worlds
    /// </summary>
    private IEnumerator ChangeWorlds()
    {
        while (_transfering)
        {
            yield return StartCoroutine("TakeOff");
            yield return StartCoroutine("RotateToTarget");
            yield return StartCoroutine("TravelToTarget");
        }
    }

    /// <summary>
    /// Player take off coroutine
    /// </summary>
    private IEnumerator TakeOff()
    {
        bool done = false;

        while (!done)
        {
            float jumpDistance = Vector3.Distance(_worlds[_prevWorld].transform.position, transform.position);
            if (jumpDistance > (maxJumpHeight * 2.5))
            {
                if (_playerRB.linearVelocity.magnitude > 2f)
                {
                    _playerRB.linearVelocity -= (transform.up * 10) * Time.deltaTime;
                }
                else
                {
                    _playerRB.linearVelocity = Vector3.zero;
                    done = true;
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// Coroutine to rotate player to target world
    /// </summary>
    private IEnumerator RotateToTarget()
    {
        bool done = false;

        while (!done)
        {
            _moveDirection = (_worlds[_currentWorld].transform.position - transform.position).normalized;
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.FromToRotation(Vector3.up, _moveDirection),
                Time.deltaTime
            );

            if (Vector3.Distance(_moveDirection, transform.up) <= 0.01f)
                done = true;

            yield return null;
        }
    }

    /// <summary>
    /// Coroutine to move player to new world and land
    /// </summary>
    private IEnumerator TravelToTarget()
    {
        bool done = false;

        while (!done)
        {
            _moveDirection = (_worlds[_currentWorld].transform.position - transform.position).normalized;
            float distance = Vector3.Distance(_worlds[_currentWorld].transform.position, transform.position);

            if (distance < (_worldDistance * landDistance) + 5)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.FromToRotation(-Vector3.up, _moveDirection),
                    Time.deltaTime
                );
                _playerRB.MovePosition(_playerRB.position + (_moveDirection * (transferSpeed * 0.5f) * Time.deltaTime));
            }
            else
            {
                _playerRB.MovePosition(_playerRB.position + (_moveDirection * transferSpeed * Time.deltaTime));
            }

            if (distance < (_worldDistance * landDistance))
            {
                if (_landed)
                {
                    _worldGravity.Attractor = _worlds[_currentWorld].GetComponent<FakeGravity>();
                    ResetState();
                    done = true;
                }
            }

            yield return null;
        }
    }

    /// <summary>
    /// Check if player can move in direction without hitting a wall
    /// </summary>
    private bool CanMove(Vector3 direction)
    {
        if (direction.magnitude == 0) return true;
        return !Physics.SphereCast(
            transform.position,
            0.4f,
            transform.TransformDirection(direction),
            out _,
            speed * Time.fixedDeltaTime + 0.05f
        );
    }

    /// <summary>
    /// Rotate player to face direction of movement
    /// </summary>
    private void RotateForward()
    {
        Vector3 dir = _moveDirection;
        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.up);
        if (Vector3.Magnitude(dir) > 0.0f)
        {
            _playerMesh.localRotation = targetRotation;
        }
    }

    /// <summary>
    /// Update player speed based on world
    /// </summary>
    private float SpeedUpdate()
    {
        float newSpeed = speed;
        if (_worldGravity != null && _worldGravity.Attractor != null && _worldGravity.Attractor.gameObject.name == "PlaneWorld")
        {
            newSpeed = speed / 2;
        }
        return newSpeed;
    }

    /// <summary>
    /// Get current world player is on
    /// </summary>
    private int CurrentWorldIndex()
    {
        if (_worlds == null || _worlds.Count == 0)
            return 0;

        if (_worldGravity == null || _worldGravity.Attractor == null)
            return 0;

        int worldIndex = 0;
        string worldName = _worldGravity.Attractor.gameObject.name;
        int count = _worlds.Count;
        for (int i = 0; i < count; i++)
        {
            if (worldName == _worlds[i].name)
            {
                worldIndex = i;
                break;
            }
        }
        return worldIndex;
    }

    /// <summary>
    /// Reset bools used for world transfer
    /// </summary>
    private void ResetState()
    {
        _transfering = false;
        _landed = false;
    }

    /// <summary>
    /// Called when player enters a collider
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (!_transfering)
            return;

        bool hitWorld = collision.transform.CompareTag("World") || collision.transform.GetComponentInParent<FakeGravity>() != null;
        if (hitWorld)
        {
            _landed = true;
        }
    }
}
