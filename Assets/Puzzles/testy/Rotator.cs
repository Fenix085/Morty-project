using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    private Rigidbody rb;
    private readonly HashSet<Rigidbody> touchingBodies = new HashSet<Rigidbody>();
    [SerializeField] private float rotationSpeed = 90f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Quaternion currentRotation = rb.rotation;
        Quaternion targetRotation = currentRotation * Quaternion.AngleAxis(rotationSpeed * Time.fixedDeltaTime, Vector3.up);
        Quaternion deltaRotation = targetRotation * Quaternion.Inverse(currentRotation);

        rb.MoveRotation(targetRotation);

        foreach (Rigidbody touchingBody in touchingBodies)
        {
            if (touchingBody == null)
            {
                continue;
            }

            Vector3 relativePosition = touchingBody.position - rb.position;
            touchingBody.MovePosition(rb.position + deltaRotation * relativePosition);
            touchingBody.MoveRotation(deltaRotation * touchingBody.rotation);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.rigidbody != null)
        {
            touchingBodies.Add(collision.rigidbody);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.rigidbody != null)
        {
            touchingBodies.Remove(collision.rigidbody);
        }
    }
}
