using UnityEngine;

public class ButtonThree : MonoBehaviour
{
    [SerializeField] private GameObject door;

    private Vector3 closedPos;

    void Start()
    {
        closedPos = door.transform.position;
        door.transform.position += new Vector3(0, 100f, 0);
        Collider doorCol = door.GetComponent<Collider>();
        if (doorCol != null) doorCol.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            door.transform.position = closedPos;
            Collider doorCol = door.GetComponent<Collider>();
            if (doorCol != null) doorCol.enabled = true;
        }
    }
}