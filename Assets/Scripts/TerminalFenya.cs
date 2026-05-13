using UnityEngine;

public class TerminalFenya : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player is near the terminal.");
            
        }
    }
}
