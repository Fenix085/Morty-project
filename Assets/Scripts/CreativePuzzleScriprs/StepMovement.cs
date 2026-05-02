using UnityEngine;

public class StepMovement : MonoBehaviour
{
    public float stepDistance = 2f;
    public float stepSpeed = 8f;

    private bool isMoving = false;
    private Vector3 targetPosition;

    void Update()
    {
        if (!isMoving)
        {
            // Вперёд
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                Step(Vector3.forward);

            // Назад
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                Step(Vector3.back);

            // Влево
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                Step(Vector3.left);

            // Вправо
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                Step(Vector3.right);
        }

        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                stepSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                isMoving = false;
            }
        }
    }

    void Step(Vector3 direction)
    {
        targetPosition = transform.position + transform.TransformDirection(direction) * stepDistance;
        isMoving = true;
    }
}