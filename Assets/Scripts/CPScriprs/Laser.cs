using UnityEngine;

public class Laser : MonoBehaviour
{
    public string laserID;

    public void SetState(bool active)
    {
        gameObject.SetActive(active);
    }
}