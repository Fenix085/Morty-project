using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LevelGenerator : MonoBehaviour
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
        " ### ",
        " .T.#",
        "#$$$#",
        "#.@.#",
        "#U F#"
    };

    private Vector2Int? teleportSource = null;
    private Vector2Int? teleportDestination = null;

    private GameObject playerInstance;
    private Transform playerTransform;

    private PuzzlePlayerMovement playerMovement;

    private bool hasTeleported = false;
    private bool levelCompleted = false;

    public PuzzlePlayerMovement PlayerMovement => playerMovement;

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
            return;
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

                // ПОЛ
                if (tile == '.' || tile == '@')
                {
                    Instantiate(floorPrefab, pos, Quaternion.identity);
                }

                // ТЕЛЕПОРТ ПОЛ
                if (tile == 'T')
                {
                    if (teleportPrefab != null)
                        Instantiate(teleportPrefab, pos, Quaternion.identity);
                    else
                        Instantiate(floorPrefab, pos, Quaternion.identity);

                    teleportSource = gridPos;
                }

                // КОРОБКИ
                if (tile == '$')
                {
                    Instantiate(floorPrefab, pos, Quaternion.identity);
                    Instantiate(boxPrefab, pos, Quaternion.identity);
                }

                // СТЕНЫ
                if (tile == '#')
                {
                    Instantiate(wallPrefab, pos, Quaternion.identity);
                }

                // СТЕНА-ТЕЛЕПОРТ
                if (tile == 'U')
                {
                    if (wallTeleportPrefab != null)
                        Instantiate(wallTeleportPrefab, pos, Quaternion.identity);
                    else
                        Instantiate(wallPrefab, pos, Quaternion.identity);

                    teleportDestination = gridPos;
                }

                // 🔥 ФИНИШ (КАК СТЕНА)
                if (tile == 'F')
                {
                    Instantiate(wallPrefab, pos, Quaternion.identity);
                }

                // ИГРОК
                if (tile == '@')
                {
                    playerInstance = Instantiate(playerPrefab, pos, Quaternion.identity);
                    playerInstance.name = "PuzzlePlayer";
                    playerInstance.SetActive(true);
                    playerTransform = playerInstance.transform;

                    PuzzlePlayerMovement movement = playerInstance.GetComponent<PuzzlePlayerMovement>();
                    if (movement != null)
                    {
                        movement.currentLevelGenerator = gameObject;
                    }

                    if (playerPrefab.scene.IsValid())
                    {
                        playerPrefab.SetActive(false);
                    }
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

    public void MoveUp()
    {
        if (EnsurePlayerMovement())
        {
            playerMovement.MoveUp();
        }
    }

    public void MoveDown()
    {
        if (EnsurePlayerMovement())
        {
            playerMovement.MoveDown();
        }
    }

    public void MoveLeft()
    {
        if (EnsurePlayerMovement())
        {
            playerMovement.MoveLeft();
        }
    }

    public void MoveRight()
    {
        if (EnsurePlayerMovement())
        {
            playerMovement.MoveRight();
        }
    }

    bool EnsurePlayerMovement()
    {
        if (playerMovement == null)
        {
            FindPlayer();
        }

        return playerMovement != null;
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

    // 🔥 ФИНИШ
    void CheckFinish()
    {
        if (levelCompleted) return;
        if (playerMovement != null && playerMovement.IsMoving()) return;

        Vector2Int gridPos = WorldToGrid(playerTransform.position);

        if (gridPos.x < 0 || gridPos.x >= level[0].Length || gridPos.y < 0 || gridPos.y >= level.Length)
            return;

        char tile = level[gridPos.y][gridPos.x];

        // Срабатывает ТОЛЬКО если на высоте
        if (tile == 'F' && playerTransform.position.y > 0.5f)
        {
            levelCompleted = true;
            SceneManager.LoadScene("FakeGravity");
            Debug.Log("🎉 Пазл пройден! Молодец!");
        }
    }

    public void CheckTeleport(Vector3 playerPosition)
    {
        if (hasTeleported) return;
        if (!teleportSource.HasValue || !teleportDestination.HasValue) return;

        Vector2Int gridPos = WorldToGrid(playerPosition);

        if (playerPosition.y < 0.5f && gridPos == teleportSource.Value)
        {
            hasTeleported = true;

            Vector3 wallPos = new Vector3(teleportDestination.Value.x, 0, -teleportDestination.Value.y);
            Vector3 teleportPos = new Vector3(
                wallPos.x,
                wallHeight + teleportOffsetY,
                wallPos.z
            );

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
            return false;
        }

        char tile = level[gridPos.y][gridPos.x];

        if (tile == '.' || tile == 'T' || tile == '#' || tile == 'U' || tile == '@' || tile == '$' || tile == 'F')
        {
            return true;
        }

        return false;
    }

    void Die()
    {
        Debug.Log("Игрок умер! Перезапуск уровня...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(-worldPos.z));
    }
}
