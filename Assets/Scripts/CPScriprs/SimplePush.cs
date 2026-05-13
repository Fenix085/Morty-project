using UnityEngine;

public class SimplePush : MonoBehaviour
{
    public float pushSpeed = 5f;

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // направление от игрока к коробке
            Vector3 dir = (transform.position - collision.transform.position);
            dir.y = 0;
            dir.Normalize();

            // двигаем коробку
            transform.position += dir * pushSpeed * Time.deltaTime;
        }
    }
}