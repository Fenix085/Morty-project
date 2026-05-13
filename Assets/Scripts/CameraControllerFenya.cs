using UnityEngine;

public class CameraControllerFenya : MonoBehaviour
{
    [SerializeField] private Transform[] cameraSections;
    [SerializeField] private Transform[] playerSections;
    [SerializeField] private Transform player;
    [SerializeField] private float cameraMoveSpeed = 3f;
    [SerializeField] private float playerMoveSpeed = 100f;
    
    private int currentSection = 0;
    private Transform targetCamera;
    private Transform targetPlayer;

    void Start()
    {
        if (cameraSections.Length > 0)
        {
            targetCamera = cameraSections[0];
            targetPlayer = playerSections[0];
            transform.position = targetCamera.position;
            transform.rotation = targetCamera.rotation;
        }
    }

    void Update()
    {
        if (targetCamera == null) return;
        transform.position = Vector3.Lerp(transform.position, targetCamera.position, cameraMoveSpeed * Time.deltaTime);
    }

    public void MoveToNext()
    {
        currentSection++;
        if (currentSection < cameraSections.Length)
        {
            targetCamera = cameraSections[currentSection];
            player.position = playerSections[currentSection].position;
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = Vector3.zero;
        }
}
}
