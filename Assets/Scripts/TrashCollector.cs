using UnityEngine;

public class TrashCollector : MonoBehaviour
{
    private PlayerLevel playerLevel;

    void Start()
    {
        playerLevel = GetComponent<PlayerLevel>();
    }

    private void OnTriggerEnter(Collider other)
    {
        TrashLevel trash = other.GetComponent<TrashLevel>();

        if (trash != null && playerLevel.level >= trash.trashLevel)
        {
            Debug.Log("Trash collected, good job!");
            Destroy(other.gameObject);
        }
    }
}