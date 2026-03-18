using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzlePlayerMovement : MonoBehaviour
{
    public float moveDistance = 1f;
    public float moveSpeed = 5f;

    private bool isMoving = false;
    private Vector3 targetPosition;

    void Update()
    {
        if (isMoving)
        {
            MovePlayer();
            return;
        }

        if (Keyboard.current.wKey.wasPressedThisFrame)
            TryMove(Vector3.forward);

        if (Keyboard.current.sKey.wasPressedThisFrame)
            TryMove(Vector3.back);

        if (Keyboard.current.aKey.wasPressedThisFrame)
            TryMove(Vector3.left);

        if (Keyboard.current.dKey.wasPressedThisFrame)
            TryMove(Vector3.right);
    }

    void TryMove(Vector3 direction)
    {
        Vector3 newPos = transform.position + direction * moveDistance;

        Collider[] hits = Physics.OverlapSphere(newPos, 0.2f);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Wall") || hit.CompareTag("Box"))
                return;
        }

        targetPosition = newPos;
        isMoving = true;
    }

    void MovePlayer()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            transform.position = targetPosition;
            isMoving = false;
        }
    }
}