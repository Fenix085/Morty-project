using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGenerator4 : MonoBehaviour
{
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject playerPrefab;
    public GameObject boxPrefab;
    public GameObject teleportPrefab;
    public GameObject wallTeleportPrefab;

    public float wallHeight = 1f;
    public float teleportOffsetY = 0.5f;
    public float deathY = -5f;

    string[] level =
    {
        "... ",
        ".U.$",
        "..$.",
        " $.$",
        " .$.",
        " ..$",
        " T@F"
    };

    private Vector2Int? teleportSource = null;
    private Vector2Int? teleportDestination = null;

    private GameObject playerInstance;
    private Transform playerTransform;
    private PuzzlePlayerMovement playerMovement;

    private bool hasTeleported = false;
    private bool levelCompleted = false;

    void Start()
    {
        GenerateLevel();
        FindPlayer();
    }

    void Update()
    {
        if (playerTransform == null) return;

        HandleHeight();
        CheckFinish();

        if (playerTransform.position.y < deathY || !IsPlayerOnValidSurface(playerTransform.position))
        {
            Die();
        }
    }

    void GenerateLevel()
    {
        for (int y = 0; y < level.Length; y++)
        {
            for (int x = 0; x < level[y].Length; x++)
            {
                char tile = level[y][x];
                Vector3 pos = new Vector3(x, 0, -y);
                Vector2Int gridPos = new Vector2Int(x, y);

                if (tile == '.' || tile == '@')
                {
                    Instantiate(floorPrefab, pos, Quaternion.identity);
                }

                if (tile == 'T')
                {
                    if (teleportPrefab != null)
                        Instantiate(teleportPrefab, pos, Quaternion.identity);
                    else
                        Instantiate(floorPrefab, pos, Quaternion.identity);

                    teleportSource = gridPos;
                }

                if (tile == '$')
                {
                    Instantiate(floorPrefab, pos, Quaternion.identity);
                    Instantiate(boxPrefab, pos, Quaternion.identity);
                }

                if (tile == '#')
                {
                    Instantiate(wallPrefab, pos, Quaternion.identity);
                }

                if (tile == 'U')
                {
                    if (wallTeleportPrefab != null)
                        Instantiate(wallTeleportPrefab, pos, Quaternion.identity);
                    else
                        Instantiate(wallPrefab, pos, Quaternion.identity);

                    teleportDestination = gridPos;
                }

                if (tile == 'F')
                {
                    Instantiate(wallPrefab, pos, Quaternion.identity);
                }

                if (tile == '@')
                {
                    playerInstance = Instantiate(playerPrefab, pos, Quaternion.identity);
                    playerTransform = playerInstance.transform;
                }
            }
        }
    }

    void FindPlayer()
    {
        if (playerInstance == null)
        {
            playerInstance = GameObject.FindGameObjectWithTag("Player");
        }

        if (playerInstance != null)
        {
            playerTransform = playerInstance.transform;
            playerMovement = playerInstance.GetComponent<PuzzlePlayerMovement>();
        }
    }

    void HandleHeight()
    {
        if (playerTransform == null || playerMovement == null) return;
        if (playerMovement.IsMoving()) return;

        Vector2Int gridPos = WorldToGrid(playerTransform.position);
        if (gridPos.x < 0 || gridPos.x >= level[0].Length || gridPos.y < 0 || gridPos.y >= level.Length)
            return;

        char tile = level[gridPos.y][gridPos.x];
        float currentY = playerTransform.position.y;

        bool isOnWallTile = (tile == '#' || tile == 'U' || tile == 'F');

        bool isOnBox = false;
        Vector3 checkPos = playerTransform.position + Vector3.down * 0.6f;
        Collider[] hits = Physics.OverlapSphere(checkPos, 0.2f);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Box"))
            {
                isOnBox = true;
                break;
            }
        }

        bool isHighSurface = isOnWallTile || isOnBox;

        if (currentY > 0.6f && !isHighSurface)
        {
            Vector3 pos = playerTransform.position;
            pos.y = 0f;
            playerTransform.position = pos;
        }

        if (currentY < 0.4f && isHighSurface)
        {
            Vector3 pos = playerTransform.position;
            pos.y = wallHeight;
            playerTransform.position = pos;
        }
    }

    void CheckFinish()
    {
        if (levelCompleted) return;
        if (playerMovement != null && playerMovement.IsMoving()) return;

        Vector2Int gridPos = WorldToGrid(playerTransform.position);
        if (gridPos.x < 0 || gridPos.x >= level[0].Length || gridPos.y < 0 || gridPos.y >= level.Length)
            return;

        char tile = level[gridPos.y][gridPos.x];
        if (tile == 'F' && playerTransform.position.y > 0.5f)
        {
            levelCompleted = true;
            Debug.Log("Пазл пройден!");
        }
    }

    public void CheckTeleport(Vector3 playerPosition)
    {
        if (hasTeleported) return;
        if (!teleportSource.HasValue || !teleportDestination.HasValue) return;

        Vector2Int gridPos = WorldToGrid(playerPosition);

        if (gridPos == teleportSource.Value)
        {
            hasTeleported = true;

            Vector3 wallPos = new Vector3(teleportDestination.Value.x, 0, -teleportDestination.Value.y);
            Vector3 teleportPos = new Vector3(wallPos.x, wallHeight + teleportOffsetY, wallPos.z);

            playerTransform.position = teleportPos;
            Invoke(nameof(ResetTeleport), 0.5f);
        }
    }

    void ResetTeleport()
    {
        hasTeleported = false;
    }

    public bool IsPlayerOnValidSurface(Vector3 playerPosition)
    {
        Vector2Int gridPos = WorldToGrid(playerPosition);
        if (gridPos.x < 0 || gridPos.x >= level[0].Length || gridPos.y < 0 || gridPos.y >= level.Length)
        {
            Debug.Log($"Вне карты! gridPos={gridPos}");
            return false;
        }

        char tile = level[gridPos.y][gridPos.x];
        
        // Разрешаем все валидные поверхности включая коробки
        bool isValid = (tile == '.' || tile == 'T' || tile == '#' || tile == 'U' || tile == '@' || tile == '$' || tile == 'F');
        
        if (!isValid)
        {
            Debug.Log($"НЕВАЛИДНАЯ ПОВЕРХНОСТЬ! gridPos={gridPos}, tile='{tile}', playerPos={playerPosition}");
        }
        
        return isValid;
    }

    void Die()
    {
        Debug.Log("Игрок умер! Перезапуск...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(-worldPos.z));
    }
}