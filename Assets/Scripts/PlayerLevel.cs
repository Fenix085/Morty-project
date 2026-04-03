using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    public int level = 1;
    public int maxLevel = 3;

    void Start()
    {
        UpdateTrash(); // 🔥 ВАЖНО — фикс твоей проблемы
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (level < maxLevel)
            {
                level++;
                Debug.Log("Level up! Current level: " + level);

                UpdateTrash();
            }
            else
            {
                Debug.Log("Max level reached!");
            }
        }
    }

    void UpdateTrash()
    {
        TrashLevel[] allTrash = FindObjectsOfType<TrashLevel>();

        foreach (TrashLevel trash in allTrash)
        {
            trash.UpdateState(level);
        }
    }
}