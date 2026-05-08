using UnityEngine;

public class PressureButton : MonoBehaviour
{
    public string[] targetLaserIDs;

    private int boxCount = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Box"))
        {
            boxCount++;
            UpdateLasers();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Box"))
        {
            boxCount--;
            UpdateLasers();
        }
    }

    void UpdateLasers()
    {
        bool active = boxCount == 0;

        for (int i = 0; i < targetLaserIDs.Length; i++)
        {
            LaserManager.Instance.SetLaser(targetLaserIDs[i], active);
        }
    }
}