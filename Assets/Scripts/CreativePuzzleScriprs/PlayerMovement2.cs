using UnityEngine;

public class PlayerMovement2 : MonoBehaviour
{
    public float speed = 5f;

    void Update()
    {
        float moveX = 0f;
        float moveZ = 0f;

        // WASD
        if (Input.GetKey(KeyCode.W)) moveZ = 1;
        if (Input.GetKey(KeyCode.S)) moveZ = -1;
        if (Input.GetKey(KeyCode.A)) moveX = -1;
        if (Input.GetKey(KeyCode.D)) moveX = 1;

        // стрелки (добавляются к WASD)
        if (Input.GetKey(KeyCode.UpArrow)) moveZ = 1;
        if (Input.GetKey(KeyCode.DownArrow)) moveZ = -1;
        if (Input.GetKey(KeyCode.LeftArrow)) moveX = -1;
        if (Input.GetKey(KeyCode.RightArrow)) moveX = 1;

        Vector3 move = new Vector3(moveX, 0, moveZ).normalized;

        transform.position += move * speed * Time.deltaTime;

        // поворот в сторону движения
        if (move != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(move);
        }
    }
}