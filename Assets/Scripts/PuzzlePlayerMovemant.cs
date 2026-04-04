using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzlePlayerMovement : MonoBehaviour
{
    public GameObject mesh;
    public float moveDistance = 1f;
    public float moveSpeed = 5f;

    private bool isMoving = false;
    private Vector3 targetPosition;

    private Transform movingBox;
    private Vector3 boxTargetPosition;
    private bool isBoxMoving = false;

    private LevelGenerator levelGenerator;

    void Start()
    {
        levelGenerator = FindObjectOfType<LevelGenerator>();
    }

    void Update()
    {
        if (isBoxMoving)
        {
            MoveBox();
        }

        if (isMoving)
        {
            MovePlayer();
            return;
        }

        if (isBoxMoving) return;

        if (Keyboard.current.wKey.wasPressedThisFrame)
            TryMove(Vector3.forward);

        if (Keyboard.current.sKey.wasPressedThisFrame)
            TryMove(Vector3.back);

        if (Keyboard.current.aKey.wasPressedThisFrame)
            TryMove(Vector3.left);

        if (Keyboard.current.dKey.wasPressedThisFrame)
            TryMove(Vector3.right);
    }

    public bool IsMoving()
    {
        return isMoving || isBoxMoving;
    }

    void TryMove(Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction, transform.up);

        mesh.transform.rotation = targetRotation;

        Vector3 newPos = transform.position + direction * moveDistance;

        Collider[] hits = Physics.OverlapSphere(newPos, 0.2f);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Wall"))
                return;

            if (hit.CompareTag("Box"))
            {
                Vector3 boxTarget = hit.transform.position + direction * moveDistance;

                Collider[] boxHits = Physics.OverlapSphere(boxTarget, 0.2f);

                foreach (Collider boxHit in boxHits)
                {
                    if (boxHit.CompareTag("Wall") || boxHit.CompareTag("Box"))
                        return;
                }

                movingBox = hit.transform;
                boxTargetPosition = boxTarget;
                isBoxMoving = true;
            }
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

            if (levelGenerator != null)
                levelGenerator.CheckTeleport(transform.position);
        }
    }

    void MoveBox()
    {
        if (movingBox == null) return;

        movingBox.position = Vector3.MoveTowards(
            movingBox.position,
            boxTargetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(movingBox.position, boxTargetPosition) < 0.01f)
        {
            movingBox.position = boxTargetPosition;
            isBoxMoving = false;
            movingBox = null;
        }
    }
}