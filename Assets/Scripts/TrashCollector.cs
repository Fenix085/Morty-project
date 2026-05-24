using UnityEngine;

public class TrashCollector : MonoBehaviour
{
    private PlayerLevel playerLevel;
    private RobotEnergy robotEnergy; // Added reference

    [SerializeField] private float energyRestoreAmount = 5f; // Amount to restore per trash

    void Start()
    {
        playerLevel = GetComponent<PlayerLevel>();
        robotEnergy = GetComponent<RobotEnergy>(); // Initialize reference
    }

    private void OnTriggerEnter(Collider other)
    {
        TrashLevel trash = other.GetComponent<TrashLevel>();

        if (trash != null && playerLevel.level >= trash.trashLevel)
        {
            // Restore energy before destroying the object
            if (robotEnergy != null)
            {
                robotEnergy.RestoreEnergy(energyRestoreAmount);
            }

            Debug.Log("Trash collected, energy restored!");
            Destroy(other.gameObject);
        }
    }
}