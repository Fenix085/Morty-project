using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // ДОБАВЛЕНО для перезагрузки сцены

public class LevelGenerator6 : MonoBehaviour
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
        "U##   ",
        "....T ",
        "..*.. ",
        ".$.*.#",
        "@.$..#",
        "F#..*#"
    };

    private Vector2Int? teleportSource = null;
    private Vector2Int? teleportDestination = null;

    private GameObject playerInstance;
    private Transform playerTransform;
    private PuzzlePlayerMovement playerMovement;

    private List<Transform> allBoxes = new List<Transform>();
    
    // ДОБАВЛЕНО: флаг для предотвращения множественной смерти
    private bool isDying = false;

    void Start()
    {
        GenerateLevel();
        FindPlayer();
        FindAllBoxes();
    }

    void Update()
    {
        if (playerTransform == null) return;
        if (isDying) return; // ДОБАВЛЕНО: не обрабатываем ничего во время смерти

        HandleHeight();
        ApplyGravity(playerTransform);
        HandleBoxGravity();
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
                    GameObject box = Instantiate(boxPrefab, pos, Quaternion.identity);
                    allBoxes.Add(box.transform);
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
                    GameObject box = Instantiate(boxPrefab, boxPos, Quaternion.identity);
                    allBoxes.Add(box.transform);
                }
            }
        }
    }

    void CheckFinish()
    {
        if (playerMovement == null) return;
        if (playerMovement.IsMoving()) return;

        Vector2Int gridPos = WorldToGrid(playerTransform.position);
        if (!IsInside(gridPos)) return;

        char tile = level[gridPos.y][gridPos.x];
        
        if (tile == 'F' && playerTransform.position.y >= wallHeight - 0.1f)
        {
            Debug.Log("ПАЗЛ ПРОЙДЕН! Игрок на финише F");
            // Здесь можно загрузить следующий уровень
            // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    void HandleHeight()
    {
        if (playerTransform == null || playerMovement == null) return;
        
        if (playerMovement.IsMoving()) return;

        Vector2Int gridPos = WorldToGrid(playerTransform.position);
        if (!IsInside(gridPos)) return;

        char tile = level[gridPos.y][gridPos.x];
        float currentY = playerTransform.position.y;

        bool isOnWallTile = (tile == '#' || tile == 'U' || tile == 'F' || tile == '*');
        
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

        if (currentY < 0.4f && isHighSurface)
        {
            Vector3 pos = playerTransform.position;
            pos.y = wallHeight;
            playerTransform.position = pos;
            Debug.Log($"Игрок поднялся на высоту {wallHeight} на клетке {gridPos} с символом '{tile}'");
        }
        
        if (currentY > 0.6f && !isHighSurface)
        {
            Vector3 pos = playerTransform.position;
            pos.y = 0f;
            playerTransform.position = pos;
            Debug.Log($"Игрок опустился на пол на клетке {gridPos} с символом '{tile}'");
        }
    }

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

    void HandleBoxGravity()
    {
        if (playerMovement != null && playerMovement.IsMoving()) return;
        
        foreach (Transform box in allBoxes)
        {
            if (box == null) continue;
            
            Vector2Int gridPos = WorldToGrid(box.position);
            
            if (!IsInside(gridPos)) continue;
            
            char tileBelow = level[gridPos.y][gridPos.x];
            
            bool hasBoxBelow = false;
            Vector3 checkPos = box.position + Vector3.down * 0.8f;
            Collider[] hits = Physics.OverlapSphere(checkPos, 0.3f);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Box") && hit.transform != box)
                {
                    hasBoxBelow = true;
                    break;
                }
            }
            
            bool hasWallBelow = (tileBelow == '#' || tileBelow == 'U' || tileBelow == 'F');
            
            float targetHeight = box.position.y;
            bool shouldChangeHeight = false;
            
            if (hasWallBelow)
            {
                if (Mathf.Abs(box.position.y - wallHeight) > 0.05f)
                {
                    targetHeight = wallHeight;
                    shouldChangeHeight = true;
                }
            }
            else if (hasBoxBelow)
            {
                shouldChangeHeight = false;
            }
            else if (tileBelow == ' ' || tileBelow == '.')
            {
                if (Mathf.Abs(box.position.y - 0f) > 0.05f)
                {
                    targetHeight = 0f;
                    shouldChangeHeight = true;
                }
            }
            else if (tileBelow == 'T')
            {
                if (Mathf.Abs(box.position.y - 0f) > 0.05f)
                {
                    targetHeight = 0f;
                    shouldChangeHeight = true;
                }
            }
            
            if (shouldChangeHeight)
            {
                Vector3 newPos = box.position;
                newPos.y = Mathf.Lerp(newPos.y, targetHeight, Time.deltaTime * 10f);
                box.position = newPos;
            }
        }
    }

    public void CheckTeleport(Vector3 playerPosition)
    {
        if (teleportSource == null || teleportDestination == null)
        {
            return;
        }

        Vector2Int playerGridPos = WorldToGrid(playerPosition);
        
        if (playerGridPos == teleportSource.Value)
        {
            Vector3 destinationWorldPos = new Vector3(
                teleportDestination.Value.x, 
                wallHeight,
                -teleportDestination.Value.y
            );
            
            if (playerInstance != null)
            {
                playerInstance.transform.position = destinationWorldPos;
                Debug.Log($"Телепорт: {teleportSource} -> {teleportDestination}");
            }
        }
    }

    bool IsPlayerOnValidSurface(Vector3 pos)
    {
        Vector2Int gridPos = WorldToGrid(pos);

        if (!IsInside(gridPos))
            return false;

        char tile = level[gridPos.y][gridPos.x];

        if (pos.y > 0.1f)
            return true;

        if (tile == ' ')
        {
            Debug.Log($"НЕВАЛИДНАЯ ПОВЕРХНОСТЬ! gridPos={gridPos}, tile='{tile}', playerPos={pos}");
            return false;
        }

        return true;
    }

    public bool IsWalkableTile(Vector3 worldPos)
    {
        Vector2Int gridPos = WorldToGrid(worldPos);
        
        if (!IsInside(gridPos))
            return false;
            
        char tile = level[gridPos.y][gridPos.x];
        
        if (tile == ' ')
            return false;
            
        return true;
    }

    void FindAllBoxes()
    {
        GameObject[] boxes = GameObject.FindGameObjectsWithTag("Box");
        allBoxes.Clear();
        foreach (GameObject box in boxes)
        {
            allBoxes.Add(box.transform);
        }
    }

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
        {
            playerTransform = playerInstance.transform;
            playerMovement = playerInstance.GetComponent<PuzzlePlayerMovement>();
        }
    }

    void Die()
    {
        if (isDying) return; // Защита от многократного вызова
        isDying = true;
        
        Debug.Log("Игрок умер! Перезагрузка уровня...");
        
        // ПЕРЕЗАГРУЖАЕМ ВСЮ СЦЕНУ
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}