using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Camera Settings")]
    public float mouseSensitivity = 200f;

    [Header("Position")]
    public float height = 4f;
    public float distance = 6f;

    private float yRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // только горизонтальное вращение
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        yRotation += mouseX;

        Quaternion rotation = Quaternion.Euler(0f, yRotation, 0f);

        // позиция камеры
        Vector3 offset = rotation * new Vector3(0f, height, -distance);
        transform.position = player.position + offset;

        // камера смотрит на игрока
        transform.LookAt(player.position + Vector3.up * 1.5f);
    }
}