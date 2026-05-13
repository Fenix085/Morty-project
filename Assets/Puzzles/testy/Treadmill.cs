using UnityEngine;

public class Treadmill : MonoBehaviour
{
    [SerializeField] float speed = 5;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            // Check if there's at least one contact point
            if (collision.contactCount > 0)
            {
                // Get the first contact point
                ContactPoint contact = collision.GetContact(0);

                // Check if hitting the TOP surface (normal points upward)
                if (contact.normal.y < 0.1f)
                {
                    rb.AddForce(transform.forward * speed, ForceMode.VelocityChange);
                }
            }
        }
    }
}
