using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Camera Settings")]
    public float mouseSensitivity = 200f;

    [Header("Position")]
    public float height = 4f;     // высота камеры
    public float distance = 6f;   // дистанция от игрока

    private float xRotation = 20f;
    private float yRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // вращение мышкой
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        Quaternion rotation = Quaternion.Euler(xRotation, yRotation, 0f);

        // позиция камеры
        Vector3 offset = rotation * new Vector3(0f, height, -distance);

        transform.position = player.position + offset;

        // камера смотрит на игрока
        transform.LookAt(player.position + Vector3.up * 1.5f);
    }
}