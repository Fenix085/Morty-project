using UnityEngine;

public class TrashMagnet : MonoBehaviour
{
    public float attractDistance = 3f;
    public float moveSpeed = 5f;

    private Transform player;
    private bool isAttracting = false;
    private Collider col;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        col = GetComponent<Collider>();
    }

    void Update()
    {
        // ❗ если мусор нельзя собрать — ничего не делаем
        if (!col.isTrigger)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance < attractDistance)
        {
            isAttracting = true;
        }

        if (isAttracting)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                moveSpeed * Time.deltaTime
            );
        }
    }
}