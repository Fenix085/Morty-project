using UnityEngine;

public class NextLevelFenya : MonoBehaviour
{
    [SerializeField] private CameraControllerFenya cameraController;

    private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Player"))
    {
        cameraController.MoveToNext();
    }
}
}
