using UnityEngine;

public class TrashMagnet : MonoBehaviour
{
    [Header("Settings")]
    public float attractDistance = 5f;
    public float moveSpeed = 6f; // Faster for better feel
    public float energyGrant = 10f;
    public float collectionRadius = 0.8f; // Collect slightly earlier to avoid bumping

    private Transform _playerTransform;
    private RobotEnergy _robotEnergy;
    private PlayerLevel _playerLevel;
    private Collider _trashCollider;
    private Vector3 _initialScale;

    private bool _isAttracting = false;
    private bool _isCollected = false;

    void Start()
    {
        _initialScale = transform.localScale;
        _trashCollider = GetComponent<Collider>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
            _robotEnergy = player.GetComponent<RobotEnergy>();
            _playerLevel = player.GetComponent<PlayerLevel>();
        }
    }

    void Update()
    {
        if (_playerTransform == null || _isCollected) return;

        // 1. Variable 'distance' is already declared here
        float distance = Vector3.Distance(transform.position, _playerTransform.position);

        // Check level requirement
        TrashLevel trashData = GetComponent<TrashLevel>();
        bool canSuck = trashData == null || (_playerLevel != null && _playerLevel.level >= trashData.trashLevel);

        if (distance < attractDistance && canSuck && !_isAttracting)
        {
            StartSucking();
        }

        if (_isAttracting && !_isCollected)
        {
            // 2. Move towards player
            transform.position = Vector3.MoveTowards(
                transform.position,
                _playerTransform.position,
                moveSpeed * Time.deltaTime
            );

            // 3. VISUAL IMPROVEMENT: Scale down
            // Use the 'distance' variable declared above instead of declaring it again
            float scaleRatio = distance / attractDistance;
            transform.localScale = Vector3.Lerp(Vector3.zero, _initialScale, scaleRatio);

            // 4. Collection check
            if (distance < collectionRadius)
            {
                Collect();
            }
        }
    }

    private void StartSucking()
    {
        _isAttracting = true;

        // DISABLE COLLIDER: This is the key. 
        // Once it starts flying, it effectively "leaves" the physical world.
        if (_trashCollider != null)
        {
            _trashCollider.enabled = false;
        }
    }

    private void Collect()
    {
        if (_isCollected) return;
        _isCollected = true;

        if (_robotEnergy != null)
            _robotEnergy.RestoreEnergy(energyGrant);

        if (TrashCounterUI.Instance != null)
            TrashCounterUI.Instance.RegisterCollectedTrash(gameObject);

        // Instant removal
        Destroy(gameObject);
    }
}