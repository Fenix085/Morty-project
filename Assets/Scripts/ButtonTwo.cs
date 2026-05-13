using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ButtonTwo : MonoBehaviour
{
    [SerializeField] private GameObject door;

    private void OnMouseDown()
    {
        door.transform.position += new Vector3(0, 100f, 0);
        Collider doorCol = door.GetComponent<Collider>();
        if (doorCol != null) doorCol.enabled = false;
    }
}