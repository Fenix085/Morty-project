using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    public int level = 1;       // стартовый уровень игрока
    public int maxLevel = 3;    // максимальный уровень

    void Update()
    {
        // Проверяем нажатие клавиши U
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (level < maxLevel)
            {
                level++; // повышаем уровень на 1
                Debug.Log("Player level increased! Current level: " + level);
            }
            else
            {
                Debug.Log("Player is already at max level!");
            }
        }
    }
}