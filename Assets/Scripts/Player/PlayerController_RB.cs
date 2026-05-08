/*********************************************************************************************************
 * Class players movement
 * -  Add this component to the players root object
 * *******************************************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(FakeGravityBody))]
public class PlayerController_RB : MonoBehaviour {

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
    [SerializeField] private Transform cameraTransform;
 
    // transfer
    private bool _transfering = false;
    private bool _landed = false;
    private float _worldDistance = 0;

    public static PlayerController_RB Instance { get; private set; }

    
    void Awake()
    {
        if(Instance == null)
            Instance = this;
    }

    // Use this for initialization
    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        // set player details
        _playerRB = GetComponent<Rigidbody>();
        _playerMesh = transform.GetChild(0).transform;
        _worldGravity = GetComponent<FakeGravityBody>();

        if (_worldGravity == null)
        {
            Debug.LogError("PlayerController_RB: Missing FakeGravityBody on player.", gameObject);
            enabled = false;
            return;
        }

        // find worlds in scene from FakeGravity scripts instead of tag-only lookup
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
            Debug.LogWarning("PlayerController_RB: No valid FakeGravity world found in scene.", gameObject);
            return;
        }

        _currentWorld = CurrentWorldIndex();
        // update player speed
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

    // Update is called once per frame
    private void Update()
    {
        // if changing worlds
        if (_transfering)
        {
            return;
        }

        // update move direction
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        // убираем наклон камеры
        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        _moveDirection = (camForward * v + camRight * h).normalized;

        // world transfer
        //if (Input.GetKeyDown("e"))
        //{
        //    WorldTransfer();
        //}
//
        //// jump
        //if (Input.GetKeyDown("space"))
        //{
        //    Jump();
        //}

        // rotate player to face the right direction
        RotateForward();
    }

    // FixedUpdate is called every fixed framerate frame
    private void FixedUpdate()
    {
        if (_transfering)
        {
            return;
        }

        if(_moveDirection.magnitude > 0)
        {
            animator.Play("Walk");
        }
        else
        {
            animator.Play("Idle");
        }

        _playerRB.MovePosition(
            _playerRB.position + transform.TransformDirection(_moveDirection * speed * Time.deltaTime)
        );

        // 🔥 ВОТ ЭТО УБИРАЕТ СКОЛЬЖЕНИЕ
        _playerRB.linearVelocity = Vector3.zero;
        _playerRB.angularVelocity = Vector3.zero;
    }

    /// <summary>
    /// Player Jump
    /// </summary>
    private void Jump()
    {
        // get current jump height
        float jumpHeight = Vector3.Distance(_worlds[_currentWorld].transform.position, transform.position) - maxJumpHeight;
        // limit height to which jump is applied
        if (jumpHeight < maxJumpHeight)
        {
            // get direction of gravity
            Vector3 gravityDir = (_worlds[_currentWorld].transform.position - transform.position).normalized;
            // apply force against gravity
            _playerRB.AddForce(-gravityDir * jumpForce, ForceMode.Impulse);
          
        }
    }

    /// <summary>
    /// Initialise player transfer between worlds
    /// </summary>
    private void WorldTransfer()
    {
        // launch player
        Jump();
        // disconnect gravity
        _worldGravity.Attractor = null;
        // increment world ID
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
        // initialise planet transfer
        _transfering = true;
        // distance between _worlds
        _worldDistance = Vector3.Distance(_worlds[_prevWorld].transform.position, _worlds[_currentWorld].transform.position) -
                                        (_worlds[_prevWorld].GetComponent<FakeGravity>().WorldSize + _worlds[_currentWorld].GetComponent<FakeGravity>().WorldSize);
        // start change worlds coroutine
        StartCoroutine("ChangeWorlds");
    }
    
    /// <summary>
    /// Corountine that controls changing worlds
    /// </summary>
    /// <returns></returns>
    private IEnumerator ChangeWorlds()
    {
        while (_transfering)
        {
            // move to travel height
            yield return StartCoroutine("TakeOff");
            // rotate to target
            yield return StartCoroutine("RotateToTarget");
            // travel to target
            yield return StartCoroutine("TravelToTarget");
        }
    }

    /// <summary>
    /// Player take off coroutine
    /// </summary>
    /// <returns></returns>
    private IEnumerator TakeOff()
    {
        bool done = false;

        while (!done)
        {
            float jumpDistance = Vector3.Distance(_worlds[_prevWorld].transform.position, transform.position);
            // transfer once player reaches max jump height
            if (jumpDistance > (maxJumpHeight * 2.5))
            {
                // reduce velocity
                if (_playerRB.linearVelocity.magnitude > 2f)
                {
                    _playerRB.linearVelocity -= (transform.up * 10) * Time.deltaTime; ;
                }
                else
                {
                    _playerRB.linearVelocity = Vector3.zero;
                    // finish coroutine
                    done = true;
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// Coroutine to rotate player to target world
    /// </summary>
    /// <returns></returns>
    private IEnumerator RotateToTarget()
    {
        bool done = false;

        while(!done)
        {
            // set move direction
            _moveDirection = (_worlds[_currentWorld].transform.position - transform.position).normalized;
            // rotate player
            transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        Quaternion.FromToRotation(Vector3.up, _moveDirection),
                        Time.deltaTime
                        );
            // check if rotation is complete
            if (Vector3.Distance(_moveDirection, transform.up) <= 0.01f)
            {
               
                // finish coroutine
                done = true;
            }
            yield return null;
        }
    }

    /// <summary>
    /// Coroutine to move player to new world and land
    /// </summary>
    /// <returns></returns>
    private IEnumerator TravelToTarget()
    {
        bool done = false;

        while(!done)
        {
            // get direction to world and move player
            _moveDirection = (_worlds[_currentWorld].transform.position - transform.position).normalized;
            // get distance from new world
            float distance = Vector3.Distance(_worlds[_currentWorld].transform.position, transform.position);
            // start landing rotation before hitting atmosphere
            if (distance < (_worldDistance * landDistance) + 5)
            {
                // rotate to land
                transform.rotation = Quaternion.Slerp(
                                                    transform.rotation,
                                                    Quaternion.FromToRotation(-Vector3.up, _moveDirection),
                                                    Time.deltaTime
                                                    );
                // move slower now closer to world
                _playerRB.MovePosition(_playerRB.position + (_moveDirection * (transferSpeed * 0.5f) * Time.deltaTime));
            }
            else
            {
                // apply normal travel speed
                _playerRB.MovePosition(_playerRB.position + (_moveDirection * transferSpeed * Time.deltaTime));
            }
            // check if entering atmosphere
            if (distance < (_worldDistance * landDistance))
            {
                // if landed then arrived at new world
                if (_landed)
                {
                    // set new attractor
                    _worldGravity.Attractor = _worlds[_currentWorld].GetComponent<FakeGravity>();
                    // reset transfer state
                    ResetState();
                    // finish coroutine
                    done = true;
                }
                else
                {
                   
                    
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// Rotate player to face direction of movement
    /// </summary>
    private void RotateForward()
    {
        Vector3 dir = _moveDirection;

        if (dir.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

            Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.up);

            // плавный поворот
            _playerMesh.localRotation = Quaternion.Slerp(
                _playerMesh.localRotation,
                targetRotation,
                10f * Time.deltaTime
            );
        }
    }

    /// <summary>
    /// Update player speed based on world
    /// </summary>
    private float SpeedUpdate()
    {
        float newSpeed = speed;
        // update speed value
        if (_worldGravity != null && _worldGravity.Attractor != null && _worldGravity.Attractor.gameObject.name == "PlaneWorld")
        {
            newSpeed = speed / 2;
        }
        // return result
        return newSpeed;
    }

    /// <summary>
    /// Get current world player is on
    /// </summary>
    private int CurrentWorldIndex()
    {
        if (_worlds == null || _worlds.Count == 0)
        {
            return 0;
        }

        if (_worldGravity == null || _worldGravity.Attractor == null)
        {
            return 0;
        }

        int worldIndex = 0;
        // get name of current world player is attracted to
        string worldName = _worldGravity.Attractor.gameObject.name;
        // iterate through list of worlds
        int count = _worlds.Count;
        for (int i = 0; i < count; i++)
        {
            // check if world in list has same name as curretn attractor
            if (worldName == _worlds[i].name)
            {
                worldIndex = i;
                break;
            }
        }
        // return result
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
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        if (!_transfering)
        {
            return;
        }

        bool hitWorld = collision.transform.CompareTag("World") || collision.transform.GetComponentInParent<FakeGravity>() != null;
        // if player transfering between worlds and has collided with a world
        if(hitWorld)
        {
            // player landed on world
            _landed = true;
         
        }
    }
}
