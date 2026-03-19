using UnityEngine;

public class TrashLevel : MonoBehaviour
{
    public int trashLevel = 1;

    private Collider col;

    void Start()
    {
        col = GetComponent<Collider>();
    }

    public void UpdateState(int playerLevel)
    {
        if (playerLevel >= trashLevel)
        {
            col.isTrigger = true; // можно собрать
        }
        else
        {
            col.isTrigger = false; // блокирует игрока
        }
    }
}