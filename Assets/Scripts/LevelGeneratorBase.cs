using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class LevelGeneratorBase : MonoBehaviour
{
    [Header("Level Data")]
    [SerializeField] private TextAsset levelText;

    [Header("Prefabs")]
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject playerPrefab;
    public GameObject boxPrefab;
    public GameObject teleportPrefab;
    public GameObject wallTeleportPrefab;

    [Header("Settings")]
    public float wallHeight = 1f;
    public float teleportOffsetY = 0.5f;
    public float fallSpeed = 5f;
    public float deathY = -5f;

    private string[] level = Array.Empty<string>();
    private Vector2Int? teleportSource;
    private Vector2Int? teleportDestination;

    private GameObject playerInstance;
    private Transform playerTransform;
    private PuzzlePlayerMovement playerMovement;
    private readonly List<Transform> allBoxes = new List<Transform>();

    private bool hasTeleported;
    private bool levelCompleted;
    private bool isDying;

    protected abstract string DefaultLevelResourcePath { get; }

    protected virtual bool RequireGroundedTeleport => false;

    protected virtual float EffectiveTeleportOffsetY => teleportOffsetY;

    protected virtual bool UseVoidGravity => false;

    protected virtual bool AllowAirborneSurface => UseVoidGravity;

    protected virtual void Start()
    {
        if (!TryLoadLevelData())
        {
            return;
        }

        GenerateLevel();
        FindPlayer();
    }

    protected virtual void Update()
    {
        if (playerTransform == null || isDying)
        {
            return;
        }

        HandleHeight();
        if (UseVoidGravity)
        {
            ApplyGravity(playerTransform);
            HandleBoxGravity();
        }

        CheckFinish();

        if (playerTransform.position.y < deathY || !IsPlayerOnValidSurface(playerTransform.position))
        {
            Die();
        }
    }

    private bool TryLoadLevelData()
    {
        TextAsset source = levelText;
        if (source == null && !string.IsNullOrWhiteSpace(DefaultLevelResourcePath))
        {
            source = Resources.Load<TextAsset>(DefaultLevelResourcePath);
        }

        if (source == null)
        {
            Debug.LogError($"{GetType().Name}: level text asset is missing.");
            return false;
        }

        level = ParseLevel(source.text);
        if (level.Length == 0)
        {
            Debug.LogError($"{GetType().Name}: level text has no rows.");
            return false;
        }

        return true;
    }

    private static string[] ParseLevel(string rawText)
    {
        string normalized = rawText.Replace("\r\n", "\n").Replace('\r', '\n');
        string[] rows = normalized.Split('\n');

        int count = rows.Length;
        while (count > 0 && rows[count - 1].Length == 0)
        {
            count--;
        }

        if (count == 0)
        {
            return Array.Empty<string>();
        }

        int maxWidth = 0;
        for (int i = 0; i < count; i++)
        {
            if (rows[i].Length > maxWidth)
            {
                maxWidth = rows[i].Length;
            }
        }

        string[] parsed = new string[count];
        for (int i = 0; i < count; i++)
        {
            parsed[i] = rows[i].PadRight(maxWidth, ' ');
        }

        return parsed;
    }

    private void GenerateLevel()
    {
        teleportSource = null;
        teleportDestination = null;
        allBoxes.Clear();

        for (int y = 0; y < level.Length; y++)
        {
            for (int x = 0; x < level[y].Length; x++)
            {
                char tile = level[y][x];
                Vector3 pos = new Vector3(x, 0f, -y);
                Vector2Int gridPos = new Vector2Int(x, y);

                switch (tile)
                {
                    case '.':
                        Instantiate(floorPrefab, pos, Quaternion.identity);
                        break;
                    case '@':
                        Instantiate(floorPrefab, pos, Quaternion.identity);
                        playerInstance = Instantiate(playerPrefab, pos, Quaternion.identity);
                        playerTransform = playerInstance != null ? playerInstance.transform : null;
                        break;
                    case '$':
                        Instantiate(floorPrefab, pos, Quaternion.identity);
                        GameObject box = Instantiate(boxPrefab, pos, Quaternion.identity);
                        if (box != null)
                        {
                            allBoxes.Add(box.transform);
                        }

                        break;
                    case '#':
                        Instantiate(wallPrefab, pos, Quaternion.identity);
                        break;
                    case 'F':
                        Instantiate(wallPrefab, pos, Quaternion.identity);
                        break;
                    case 'T':
                        if (teleportPrefab != null)
                        {
                            Instantiate(teleportPrefab, pos, Quaternion.identity);
                        }
                        else
                        {
                            Instantiate(floorPrefab, pos, Quaternion.identity);
                        }

                        teleportSource = gridPos;
                        break;
                    case 'U':
                        if (wallTeleportPrefab != null)
                        {
                            Instantiate(wallTeleportPrefab, pos, Quaternion.identity);
                        }
                        else
                        {
                            Instantiate(wallPrefab, pos, Quaternion.identity);
                        }

                        teleportDestination = gridPos;
                        break;
                    case '*':
                        Instantiate(wallPrefab, pos, Quaternion.identity);
                        GameObject elevatedBox = Instantiate(boxPrefab, pos + Vector3.up * wallHeight, Quaternion.identity);
                        if (elevatedBox != null)
                        {
                            allBoxes.Add(elevatedBox.transform);
                        }

                        break;
                }
            }
        }
    }

    private void FindPlayer()
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

    private void HandleHeight()
    {
        if (playerTransform == null || playerMovement == null || playerMovement.IsMoving())
        {
            return;
        }

        Vector2Int gridPos = WorldToGrid(playerTransform.position);
        if (!IsInside(gridPos))
        {
            return;
        }

        char tile = level[gridPos.y][gridPos.x];
        bool isOnWallTile = tile == '#' || tile == 'U' || tile == 'F' || tile == '*';

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
        float currentY = playerTransform.position.y;

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

    private void ApplyGravity(Transform obj)
    {
        Vector2Int gridPos = WorldToGrid(obj.position);
        if (!IsInside(gridPos))
        {
            return;
        }

        char tile = level[gridPos.y][gridPos.x];
        Vector3 pos = obj.position;

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

    private void HandleBoxGravity()
    {
        if (playerMovement != null && playerMovement.IsMoving())
        {
            return;
        }

        foreach (Transform box in allBoxes)
        {
            if (box == null)
            {
                continue;
            }

            Vector2Int gridPos = WorldToGrid(box.position);
            if (!IsInside(gridPos))
            {
                continue;
            }

            char tileBelow = level[gridPos.y][gridPos.x];
            bool hasWallBelow = tileBelow == '#' || tileBelow == 'U' || tileBelow == 'F' || tileBelow == '*';

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
            else if (!hasBoxBelow && (tileBelow == ' ' || tileBelow == '.' || tileBelow == 'T' || tileBelow == '@' || tileBelow == '$'))
            {
                if (Mathf.Abs(box.position.y) > 0.05f)
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

    private void CheckFinish()
    {
        if (levelCompleted || playerTransform == null)
        {
            return;
        }

        if (playerMovement != null && playerMovement.IsMoving())
        {
            return;
        }

        Vector2Int gridPos = WorldToGrid(playerTransform.position);
        if (!IsInside(gridPos))
        {
            return;
        }

        char tile = level[gridPos.y][gridPos.x];
        if (tile == 'F' && playerTransform.position.y > 0.5f)
        {
            levelCompleted = true;
            Debug.Log("Puzzle completed.");
        }
    }

    public void CheckTeleport(Vector3 playerPosition)
    {
        if (hasTeleported || !teleportSource.HasValue || !teleportDestination.HasValue)
        {
            return;
        }

        if (RequireGroundedTeleport && playerPosition.y >= 0.5f)
        {
            return;
        }

        Vector2Int gridPos = WorldToGrid(playerPosition);
        if (gridPos != teleportSource.Value)
        {
            return;
        }

        hasTeleported = true;

        Vector3 wallPos = new Vector3(teleportDestination.Value.x, 0f, -teleportDestination.Value.y);
        Vector3 teleportPos = new Vector3(
            wallPos.x,
            wallHeight + EffectiveTeleportOffsetY,
            wallPos.z
        );

        if (playerTransform != null)
        {
            playerTransform.position = teleportPos;
        }

        Invoke(nameof(ResetTeleport), 0.5f);
    }

    private void ResetTeleport()
    {
        hasTeleported = false;
    }

    public bool IsPlayerOnValidSurface(Vector3 playerPosition)
    {
        Vector2Int gridPos = WorldToGrid(playerPosition);
        if (!IsInside(gridPos))
        {
            return false;
        }

        if (AllowAirborneSurface && playerPosition.y > 0.1f)
        {
            return true;
        }

        return level[gridPos.y][gridPos.x] != ' ';
    }

    public bool IsWalkableTile(Vector3 worldPos)
    {
        Vector2Int gridPos = WorldToGrid(worldPos);
        return IsInside(gridPos) && level[gridPos.y][gridPos.x] != ' ';
    }

    private void Die()
    {
        if (isDying)
        {
            return;
        }

        isDying = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(-worldPos.z));
    }

    private bool IsInside(Vector2Int pos)
    {
        return pos.y >= 0 &&
               pos.y < level.Length &&
               pos.x >= 0 &&
               pos.x < level[pos.y].Length;
    }
}