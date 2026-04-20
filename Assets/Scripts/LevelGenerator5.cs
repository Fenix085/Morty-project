using UnityEngine;

public class LevelGenerator5 : MonoBehaviour
{
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject boxPrefab;
    public GameObject playerPrefab;
    public GameObject teleportPrefab;
    public GameObject wallTeleportPrefab;

    public float wallHeight = 1f;
    public float fallSpeed = 5f;
    public float deathY = -5f;

    private string[] level =
    {
        " ... ",
        " .$. ",
        "F.*.U",
        " @.T "
    };

    private Vector2Int? teleportSource = null;
    private Vector2Int? teleportDestination = null;

    private GameObject playerInstance;
    private Transform playerTransform;

    void Start()
    {
        GenerateLevel();
        FindPlayer();
    }

    void Update()
    {
        if (playerTransform == null) return;

        ApplyGravity(playerTransform);

        if (playerTransform.position.y < deathY || !IsPlayerOnValidSurface(playerTransform.position))
        {
            Die();
        }
    }

    // ================== ГЕНЕРАЦИЯ ==================
    void GenerateLevel()
    {
        for (int y = 0; y < level.Length; y++)
        {
            for (int x = 0; x < level[y].Length; x++)
            {
                char tile = level[y][x];
                Vector3 pos = new Vector3(x, 0, -y);
                Vector2Int gridPos = new Vector2Int(x, y);

                if (tile == '.' || tile == '$' || tile == '@' || tile == 'T')
                {
                    Instantiate(floorPrefab, pos, Quaternion.identity);
                }

                if (tile == '#')
                {
                    Instantiate(wallPrefab, pos, Quaternion.identity);
                }

                if (tile == 'F')
                {
                    Instantiate(wallPrefab, pos, Quaternion.identity);
                }

                if (tile == '$')
                {
                    Instantiate(boxPrefab, pos, Quaternion.identity);
                }

                if (tile == '@')
                {
                    playerInstance = Instantiate(playerPrefab, pos, Quaternion.identity);
                    playerTransform = playerInstance.transform;
                }

                if (tile == 'T')
                {
                    Instantiate(teleportPrefab, pos, Quaternion.identity);
                    teleportSource = gridPos;
                }

                if (tile == 'U')
                {
                    Instantiate(wallTeleportPrefab, pos, Quaternion.identity);
                    teleportDestination = gridPos;
                }

                if (tile == '*')
                {
                    Instantiate(wallPrefab, pos, Quaternion.identity);

                    Vector3 boxPos = pos + Vector3.up * wallHeight;
                    Instantiate(boxPrefab, boxPos, Quaternion.identity);
                }
            }
        }
    }

    // ================== ГРАВИТАЦИЯ ==================
    void ApplyGravity(Transform obj)
    {
        Vector3 pos = obj.position;
        Vector2Int gridPos = WorldToGrid(pos);

        if (!IsInside(gridPos)) return;

        char tile = level[gridPos.y][gridPos.x];

        if (tile == ' ')
        {
            pos.y -= fallSpeed * Time.deltaTime;
        }
        else
        {
            pos.y = Mathf.Max(pos.y, 0f);
        }

        obj.position = pos;
    }

    // ================== ПРОВЕРКА (ИСПРАВЛЕНА) ==================
    bool IsPlayerOnValidSurface(Vector3 pos)
    {
        Vector2Int gridPos = WorldToGrid(pos);

        if (!IsInside(gridPos))
            return false;

        char tile = level[gridPos.y][gridPos.x];

        // если в воздухе — ок
        if (pos.y > 0.1f)
            return true;

        // ❌ запрещаем только пустоту
        if (tile == ' ')
        {
            Debug.Log($"НЕВАЛИДНАЯ ПОВЕРХНОСТЬ! gridPos={gridPos}, tile='{tile}', playerPos={pos}");
            return false;
        }

        // ✅ всё остальное можно
        return true;
    }

    // ================== ВСПОМОГАТЕЛЬНОЕ ==================
    Vector2Int WorldToGrid(Vector3 pos)
    {
        int x = Mathf.RoundToInt(pos.x);
        int y = Mathf.RoundToInt(-pos.z);
        return new Vector2Int(x, y);
    }

    bool IsInside(Vector2Int pos)
    {
        return pos.y >= 0 && pos.y < level.Length &&
               pos.x >= 0 && pos.x < level[pos.y].Length;
    }

    void FindPlayer()
    {
        if (playerInstance != null)
            playerTransform = playerInstance.transform;
    }

    void Die()
    {
        Debug.Log("Игрок умер");
    }
}