using UnityEngine;

public class BoxThrower : MonoBehaviour
{
    [SerializeField] private float throwDistance = 4f;
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private LayerMask boxLayer;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryThrowBox();
        }
    }

    void TryThrowBox()
    {
        Vector3 origin = transform.position + Vector3.up;

        Debug.DrawRay(origin, transform.forward * interactDistance, Color.red, 1f);

        if (Physics.Raycast(origin, transform.forward, out RaycastHit hit, interactDistance))
        {
            if (!hit.collider.CompareTag("Box"))
                return;

            Rigidbody rb = hit.collider.attachedRigidbody;

            Vector3 targetPos = transform.position - transform.forward * throwDistance;

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.Sleep();
            }

            hit.collider.transform.position = targetPos;

            if (rb != null)
            {
                rb.WakeUp();
            }
        }
    }
}