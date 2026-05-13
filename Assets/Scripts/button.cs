using UnityEngine;

public class button : MonoBehaviour
{
    [SerializeField] private GameObject door;

    private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Player"))
    {
        door.transform.position += new Vector3(0, 100f, 0);
        Collider doorCol = door.GetComponent<Collider>();
        if (doorCol != null) doorCol.enabled = false;
    }
}
}
