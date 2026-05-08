using UnityEngine;

public class SunRotation : MonoBehaviour
{
    [SerializeField] private float degreesPerSecond = 20f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.right + Vector3.left, degreesPerSecond * Time.deltaTime, Space.Self);
    }
}
