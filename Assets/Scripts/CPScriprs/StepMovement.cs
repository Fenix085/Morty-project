using UnityEngine;

public class StepMovement : MonoBehaviour
{
    public Transform cam;

    public float stepDistance = 2f;
    public float stepDuration = 0.15f;

    private bool isMoving = false;

    private Vector3 startPos;
    private Vector3 targetPos;
    private float t;

    void Update()
    {
        if (!isMoving)
        {
            Vector3 dir = GetInputDirection();

            if (dir != Vector3.zero)
            {
                StartMove(dir);
            }
        }
        else
        {
            t += Time.deltaTime / stepDuration;

            transform.position = Vector3.Lerp(startPos, targetPos, t);

            if (t >= 1f)
            {
                transform.position = targetPos;
                isMoving = false;
            }
        }
    }

    void StartMove(Vector3 dir)
    {
        transform.rotation = Quaternion.LookRotation(dir);

        startPos = transform.position;
        targetPos = transform.position + dir * stepDistance;

        t = 0f;
        isMoving = true;
    }

    Vector3 GetInputDirection()
    {
        Vector3 input = Vector3.zero;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            input = Vector3.forward;

        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            input = Vector3.back;

        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            input = Vector3.left;

        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            input = Vector3.right;

        if (input == Vector3.zero)
            return Vector3.zero;

        Vector3 forward = cam.forward;
        Vector3 right = cam.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 dir = forward * input.z + right * input.x;
        dir.y = 0;

        return dir.normalized;
    }
}