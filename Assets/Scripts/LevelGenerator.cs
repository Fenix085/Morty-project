using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject playerPrefab;
    public GameObject boxPrefab;

    string[] level =
    {
        "#######",
        "#     #",
        "# $ $ #",
        "#  @  #",
        "#     #",
        "# $   #",
        "#######"
    };

    void Start()
    {
        GenerateLevel();
    }

    void GenerateLevel()
    {
        for (int y = 0; y < level.Length; y++)
        {
            for (int x = 0; x < level[y].Length; x++)
            {
                char tile = level[y][x];

                Vector3 pos = new Vector3(x, 0, -y);

                // создаём пол
                Instantiate(floorPrefab, pos, Quaternion.identity);

                if (tile == '#')
                {
                    Instantiate(wallPrefab, pos, Quaternion.identity);
                }
                else if (tile == '@')
                {
                    Instantiate(playerPrefab, pos, Quaternion.identity);
                }
                else if (tile == '$')
                {
                    Instantiate(boxPrefab, pos, Quaternion.identity);
                }
            }
        }
    }
}