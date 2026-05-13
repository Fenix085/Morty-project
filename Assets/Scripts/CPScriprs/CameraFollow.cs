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
    private Vector3 lastUp;
    private Vector3 referenceForward;

    void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;

        if (player != null)
        {
            lastUp = player.up;
            referenceForward = player.forward;
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 localUp = player.up;

        // keep the reference forward aligned with the surface as the player
        // moves across the planet (up vector changes)
        Quaternion upCorrection = Quaternion.FromToRotation(lastUp, localUp);
        referenceForward = (upCorrection * referenceForward).normalized;
        lastUp = localUp;

        // project out any drift so it stays perfectly tangent to the surface
        referenceForward = Vector3.ProjectOnPlane(referenceForward, localUp).normalized;

        // apply mouse rotation around the local up
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        yRotation += mouseX;

        Quaternion rotation = Quaternion.AngleAxis(yRotation, localUp);
        Vector3 offset = rotation * (-referenceForward * distance) + localUp * height;

        transform.position = player.position + offset;
        transform.LookAt(player.position + localUp * 1.5f, localUp);
    }
}