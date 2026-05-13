using UnityEngine;

public class BoxCarry : MonoBehaviour
{
    [SerializeField, Tooltip("Pickup / Drop key")]
    private KeyCode pickupKey = KeyCode.E;

    [SerializeField, Tooltip("Maximum pickup range")]
    private float pickupRange = 2.5f;

    [SerializeField, Tooltip("Point where the box will be held")]
    private Transform holdPoint;

    [SerializeField, Tooltip("Box follow strength")]
    private float followStrength = 20f;

    [SerializeField, Tooltip("Distance of hold point from player")]
    private float holdDistance = 2f;

    [SerializeField, Tooltip("Height of hold point")]
    private float holdHeight = 1f;

    private GameObject heldBox;
    private Rigidbody heldRB;
    private Collider heldCollider;
    private Collider playerCollider;

    private void Start()
    {
        playerCollider = GetComponent<Collider>();
    }

    private void Update()
    {
        if (heldBox == null)
        {
            UpdateHoldPointPosition();
        }

        if (Input.GetKeyDown(pickupKey))
        {
            if (heldBox == null)
                TryPickup();
            else
                Drop();
        }
    }

    private void FixedUpdate()
    {
        if (heldBox == null || heldRB == null)
            return;

        Vector3 direction = holdPoint.position - heldBox.transform.position;

        heldRB.linearVelocity = direction * followStrength;
        heldRB.angularVelocity = Vector3.zero;
        if (heldBox != null && heldCollider != null)
        {
            bool touchingWall = Physics.CheckBox(
                heldCollider.bounds.center,
                heldCollider.bounds.extents * 0.9f,
                heldBox.transform.rotation,
                LayerMask.GetMask("Wall")
            );

            if (touchingWall)
            {
                Drop();
                return;
            }
        }
    }

    private void UpdateHoldPointPosition()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * v + right * h).normalized;

        if (moveDirection.magnitude > 0.1f)
        {
            holdPoint.position =
                transform.position +
                moveDirection * holdDistance +
                Vector3.up * holdHeight;
        }
    }

    private void TryPickup()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRange);

        float closestDistance = Mathf.Infinity;
        GameObject closestBox = null;

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Box"))
                continue;

            float distance = Vector3.Distance(transform.position, hit.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestBox = hit.gameObject;
            }
        }

        if (closestBox != null)
            Pickup(closestBox);
    }

    private void Pickup(GameObject box)
    {
        heldBox = box;
        heldRB = box.GetComponent<Rigidbody>();
        heldCollider = box.GetComponent<Collider>();

        if (heldRB != null)
        {
            heldRB.useGravity = false;
            heldRB.linearVelocity = Vector3.zero;
        }

        if (heldCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(heldCollider, playerCollider, true);
        }
    }

    private void Drop()
    {
        if (heldBox == null)
            return;

        if (heldCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(heldCollider, playerCollider, false);
        }

        if (heldRB != null)
        {
            heldRB.useGravity = true;
        }

        heldBox.transform.position += Vector3.up * 0.1f;

        heldBox = null;
        heldRB = null;
        heldCollider = null;
    }
}