using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

public class LevelGenerator : MonoBehaviour
{
    [Header("Префабы")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject boxPrefab;
    public GameObject playerPrefab;
    public GameObject teleportPrefab;
    public GameObject wallTeleportPrefab;

    [Header("Настройки")]
    public float wallHeight = 1f;
    public float fallSpeed = 5f;
    public float deathY = -5f;
    
    [Header("Выбор уровня")]
    public int currentLevel = 1;
    public bool autoLoadNextLevel = true;

    private static int savedLevel = 1;
    private static List<string[]> savedAllLevels = null;

    private List<string[]> allLevels = new List<string[]>();
    private List<string> currentLevelRows = new List<string>();
    private Vector2Int? teleportSource = null;
    private Vector2Int? teleportDestination = null;
    private GameObject playerInstance;
    private Transform playerTransform;
    private PuzzlePlayerMovement playerMovement;
    private List<Transform> allBoxes = new List<Transform>();
    private bool isDying = false;
    private bool levelCompleted = false;
    private bool levelGenerated = false;

    void Awake()
    {
        if (savedLevel != 1)
        {
            currentLevel = savedLevel;
        }
        
        if (savedAllLevels != null)
        {
            allLevels = savedAllLevels;
        }
    }

    void Start()
    {
        if (allLevels.Count == 0)
        {
            LoadAllLevels();
            savedAllLevels = allLevels;
        }
        
        if (allLevels.Count == 0)
        {
            Debug.LogError("Не найдено ни одного уровня в папке Assets/Puzzles/");
            return;
        }
        
        if (!levelGenerated)
        {
            LoadLevelData(currentLevel);
            GenerateLevel();
            FindPlayer();
            FindAllBoxes();
            levelGenerated = true;
        }
    }

    void Update()
    {
        if (playerTransform == null || isDying || levelCompleted) return;

        HandleHeight();
        ApplyGravity(playerTransform);
        HandleBoxGravity();
        CheckFinish();
        CheckPlayerDeath();
    }

    void LoadAllLevels()
    {
        allLevels.Clear();
        
        string puzzlesPath = Application.dataPath + "/Puzzles/";
        
        if (!Directory.Exists(puzzlesPath))
        {
            Debug.LogError($"Папка не найдена: {puzzlesPath}");
            return;
        }
        
        string[] levelFiles = Directory.GetFiles(puzzlesPath, "PuzzleLevel*.txt");
        System.Array.Sort(levelFiles);
        
        foreach (string filePath in levelFiles)
        {
            try
            {
                string content = File.ReadAllText(filePath);
                string[] lines = content.Split('\n');
                List<string> levelRows = new List<string>();
                
                foreach (string line in lines)
                {
                    string trimmedEnd = line.TrimEnd('\r');
                    if (!string.IsNullOrWhiteSpace(trimmedEnd))
                    {
                        levelRows.Add(trimmedEnd);
                    }
                }
                
                if (levelRows.Count > 0)
                {
                    allLevels.Add(levelRows.ToArray());
                    string fileName = Path.GetFileName(filePath);
                    Debug.Log($"Загружен уровень {allLevels.Count}: {fileName}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Ошибка загрузки {filePath}: {e.Message}");
            }
        }
        
        Debug.Log($"Всего загружено уровней: {allLevels.Count}");
    }

    void LoadLevelData(int levelNumber)
    {
        if (levelNumber < 1 || levelNumber > allLevels.Count)
        {
            Debug.LogError($"Уровень {levelNumber} не существует!");
            return;
        }
        
        currentLevel = levelNumber;
        currentLevelRows.Clear();
        currentLevelRows.AddRange(allLevels[levelNumber - 1]);
        
        teleportSource = null;
        teleportDestination = null;
        allBoxes.Clear();
        levelCompleted = false;
        isDying = false;
        
        Debug.Log($"=== ЗАГРУЖЕН УРОВЕНЬ {currentLevel} ===");
    }

    void GenerateLevel()
{
    for (int y = 0; y < currentLevelRows.Count; y++)
    {
        string row = currentLevelRows[y];
        for (int x = 0; x < row.Length; x++)
        {
            char tile = row[x];
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
                
                // ПАРТИКЛЫ ДЛЯ ТЕЛЕПОРТА (источник)
                GameObject tileObj = new GameObject($"TeleportSource_{x}_{y}");
                tileObj.transform.position = pos;
                TileParticles particles = tileObj.AddComponent<TileParticles>();
                particles.type = TileParticles.ParticleType.TeleportSource;
                
                teleportSource = gridPos;
            }

            if (tile == '$')
            {
                Instantiate(floorPrefab, pos, Quaternion.identity);
                GameObject box = Instantiate(boxPrefab, pos, Quaternion.identity);
                allBoxes.Add(box.transform);
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
                
                // ПАРТИКЛЫ ДЛЯ ВЫХОДА ИЗ ТЕЛЕПОРТА
                GameObject tileObj = new GameObject($"TeleportDestination_{x}_{y}");
                tileObj.transform.position = pos;
                TileParticles particles = tileObj.AddComponent<TileParticles>();
                particles.type = TileParticles.ParticleType.TeleportDestination;
                
                teleportDestination = gridPos;
            }

            if (tile == 'F')
            {
                Instantiate(wallPrefab, pos, Quaternion.identity);
                
                // ПАРТИКЛЫ ДЛЯ ФИНИША
                GameObject tileObj = new GameObject($"Finish_{x}_{y}");
                tileObj.transform.position = pos;
                TileParticles particles = tileObj.AddComponent<TileParticles>();
                particles.type = TileParticles.ParticleType.Finish;
            }

            if (tile == '*')
            {
                Instantiate(wallPrefab, pos, Quaternion.identity);
                Vector3 boxPos = pos + Vector3.up * wallHeight;
                GameObject box = Instantiate(boxPrefab, boxPos, Quaternion.identity);
                allBoxes.Add(box.transform);
            }

            if (tile == '@')
            {
                playerInstance = Instantiate(playerPrefab, pos, Quaternion.identity);
                playerTransform = playerInstance.transform;
            }
        }
    }
}

    void HandleHeight()
    {
        if (playerTransform == null || playerMovement == null) return;
        if (playerMovement.IsMoving()) return;

        Vector2Int gridPos = WorldToGrid(playerTransform.position);
        if (!IsInside(gridPos)) return;

        char tile = GetTileAt(gridPos);
        float currentY = playerTransform.position.y;

        bool isOnWallTile = (tile == '#' || tile == 'U' || tile == 'F' || tile == '*');
        
        bool isOnBox = false;
        Vector3 checkPos = playerTransform.position + Vector3.down * 0.6f;
        Collider[] hits = Physics.OverlapSphere(checkPos, 0.2f);
        foreach (Collider hit in hits)
        {
            if (hit != null && hit.CompareTag("Box"))
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
        }
        
        if (currentY > 0.6f && !isHighSurface)
        {
            Vector3 pos = playerTransform.position;
            pos.y = 0f;
            playerTransform.position = pos;
        }
    }

    void ApplyGravity(Transform obj)
    {
        if (obj == null) return;
        
        Vector3 pos = obj.position;
        Vector2Int gridPos = WorldToGrid(pos);
        
        if (!IsInside(gridPos))
        {
            if (obj.CompareTag("Box"))
            {
                allBoxes.Remove(obj);
                Destroy(obj.gameObject);
            }
            return;
        }

        char tile = GetTileAt(gridPos);
        
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
        
        List<Transform> boxesCopy = new List<Transform>(allBoxes);
        
        foreach (Transform box in boxesCopy)
        {
            if (box == null) continue;
            
            float currentY = box.position.y;
            
            if (currentY < -1f)
            {
                Debug.Log($"Коробка упала в пустоту и исчезла");
                allBoxes.Remove(box);
                Destroy(box.gameObject);
                continue;
            }
            
            Vector2Int gridPos = WorldToGrid(box.position);
            
            if (!IsInside(gridPos))
            {
                Debug.Log($"Коробка за пределами карты");
                allBoxes.Remove(box);
                Destroy(box.gameObject);
                continue;
            }
            
            char tileBelow = GetTileAt(gridPos);
            
            if (tileBelow == ' ')
            {
                Vector3 newPos = box.position;
                newPos.y -= fallSpeed * Time.deltaTime;
                box.position = newPos;
                continue;
            }
            
            if (tileBelow == '.' || tileBelow == 'T')
            {
                if (Mathf.Abs(box.position.y - 0f) > 0.05f)
                {
                    Vector3 newPos = box.position;
                    newPos.y = Mathf.Lerp(newPos.y, 0f, Time.deltaTime * 10f);
                    box.position = newPos;
                }
            }
            
            if (tileBelow == '#' || tileBelow == 'U' || tileBelow == 'F')
            {
                if (Mathf.Abs(box.position.y - wallHeight) > 0.05f)
                {
                    Vector3 newPos = box.position;
                    newPos.y = Mathf.Lerp(newPos.y, wallHeight, Time.deltaTime * 10f);
                    box.position = newPos;
                }
            }
        }
    }

    public void CheckTeleport(Vector3 playerPosition)
    {
        if (teleportSource == null || teleportDestination == null) return;

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

    void CheckFinish()
    {
        if (levelCompleted) return;
        if (playerMovement != null && playerMovement.IsMoving()) return;

        Vector2Int gridPos = WorldToGrid(playerTransform.position);
        if (!IsInside(gridPos)) return;

        char tile = GetTileAt(gridPos);
        
        if (tile == 'F' && playerTransform.position.y >= wallHeight - 0.1f)
        {
            levelCompleted = true;
            Debug.Log($"✅ УРОВЕНЬ {currentLevel} ПРОЙДЕН! ✅");
            
            if (autoLoadNextLevel)
            {
                LoadNextLevel();
            }
        }
    }
    
    void CheckPlayerDeath()
    {
        if (playerTransform == null) return;
        
        Vector2Int gridPos = WorldToGrid(playerTransform.position);
        float currentY = playerTransform.position.y;
        
        // Умер если упал ниже deathY
        if (currentY < deathY)
        {
            Debug.Log("Игрок упал слишком глубоко - СМЕРТЬ!");
            Die();
            return;
        }
        
        // Проверяем клетку ПОД игроком (на которой он стоит или над которой висит)
        if (IsInside(gridPos))
        {
            char tile = GetTileAt(gridPos);
            
            // ЕСЛИ ПОД ИГРОКОМ ПУСТОТА (ПРОБЕЛ) - СРАЗУ СМЕРТЬ, НЕЗАВИСИМО ОТ ВЫСОТЫ
            if (tile == ' ')
            {
                Debug.Log($"Игрок над пустотой (пробел) на клетке {gridPos} на высоте {currentY} - СМЕРТЬ!");
                Die();
                return;
            }
        }
        else
        {
            // За пределами карты - смерть
            Debug.Log("Игрок за пределами карты - СМЕРТЬ!");
            Die();
            return;
        }
    }

    bool IsPlayerOnValidSurface(Vector3 pos)
    {
        Vector2Int gridPos = WorldToGrid(pos);
        if (!IsInside(gridPos)) return false;

        char tile = GetTileAt(gridPos);
        
        if (tile == ' ')
            return false;
            
        return true;
    }

    public bool IsWalkableTile(Vector3 worldPos)
    {
        Vector2Int gridPos = WorldToGrid(worldPos);
        if (!IsInside(gridPos)) return false;
        char tile = GetTileAt(gridPos);
        return tile != ' ';
    }

    void LoadNextLevel()
    {
        int nextLevel = currentLevel + 1;
        
        if (nextLevel <= allLevels.Count)
        {
            savedLevel = nextLevel;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            Debug.Log("🎉 ПОЗДРАВЛЯЮ! ВСЕ УРОВНИ ПРОЙДЕНЫ! 🎉");
        }
    }

    Vector2Int WorldToGrid(Vector3 pos)
    {
        return new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(-pos.z));
    }

    bool IsInside(Vector2Int pos)
    {
        return pos.y >= 0 && pos.y < currentLevelRows.Count && 
               pos.x >= 0 && pos.x < currentLevelRows[pos.y].Length;
    }

    char GetTileAt(Vector2Int pos)
    {
        if (!IsInside(pos)) return ' ';
        return currentLevelRows[pos.y][pos.x];
    }

    void FindAllBoxes()
    {
        GameObject[] boxes = GameObject.FindGameObjectsWithTag("Box");
        allBoxes.Clear();
        foreach (GameObject box in boxes) 
        {
            if (box != null) allBoxes.Add(box.transform);
        }
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
        if (isDying) return;
        isDying = true;
        
        Debug.Log("💀 Игрок умер! Перезагрузка уровня... 💀");
        savedLevel = currentLevel;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}