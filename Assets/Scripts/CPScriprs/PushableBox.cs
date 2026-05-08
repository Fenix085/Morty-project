using UnityEngine;

public class PushableBox : MonoBehaviour
{
    public float pushDistance = 2f;
    public float moveSpeed = 6f;

    private bool isMoving = false;
    private Vector3 targetPos;

    void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPos) < 0.01f)
            {
                transform.position = targetPos;
                isMoving = false;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        if (isMoving)
            return;

        // направление от игрока к коробке
        Vector3 dir = (transform.position - collision.transform.position);
        dir.y = 0;
        dir.Normalize();

        // округляем до 4 направлений (очень важно для стабильности)
        dir = GetCardinalDirection(dir);

        Vector3 nextPos = transform.position + dir * pushDistance;

        // проверка, чтобы не влезть в стену
        if (Physics.OverlapSphere(nextPos, 0.3f).Length > 0)
            return;

        targetPos = nextPos;
        isMoving = true;
    }

    Vector3 GetCardinalDirection(Vector3 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.z))
            return dir.x > 0 ? Vector3.right : Vector3.left;
        else
            return dir.z > 0 ? Vector3.forward : Vector3.back;
    }
}