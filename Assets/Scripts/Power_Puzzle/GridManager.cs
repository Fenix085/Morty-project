using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 7;
    public int height = 6;
    public GameObject tilePrefab;
    public Tile[,] grid;
    public DraggableItem[,] placedItems;
    public Vector2Int sourcePosition;
    public Direction sourceDirection;
    public Vector2 gridOffset;

    private Coroutine powerRoutine;

    [System.Serializable]
    public struct TargetData
    {
        public Vector2Int position;
        public Direction direction;
    }

    public List<TargetData> targets = new List<TargetData>();

    public void RegisterTarget(Vector2Int pos, Direction dir)
    {
        bool exists = false;
        foreach (var t in targets)
        {
            if (t.position == pos && t.direction == dir) { exists = true; break; }
        }

        if (!exists)
        {
            targets.Add(new TargetData { position = pos, direction = dir });
            Debug.Log($"Target registered at {pos} from direction {dir}");
        }
    }

    public Vector2Int DirectionToVector(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up: return new Vector2Int(0, 1);
            case Direction.Right: return new Vector2Int(1, 0);
            case Direction.Down: return new Vector2Int(0, -1);
            case Direction.Left: return new Vector2Int(-1, 0);
        }
        return Vector2Int.zero;
    }

    public Direction Opposite(Direction dir)
    {
        return (Direction)(((int)dir + 2) % 4);
    }

    public void UpdatePower(Vector2Int sourcePos)
    {
        
        StopPowerAnimation();
        foreach (var item in placedItems)
        {
            if (item != null) item.SetPowered(false);
        }

        
        List<DraggableItem> currentPath = GetCurrentPowerPath();
        bool isComplete = CheckVictoryCondition(currentPath);

       
        if (isComplete)
        {
            powerRoutine = StartCoroutine(AnimatePowerFlow(currentPath));
        }
    }

    
    private List<DraggableItem> GetCurrentPowerPath()
    {
        List<DraggableItem> path = new List<DraggableItem>();
        Queue<Vector2Int> checkQueue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        Vector2Int startPos = sourcePosition + DirectionToVector(sourceDirection);

        if (IsInside(startPos))
        {
            DraggableItem firstItem = placedItems[startPos.x, startPos.y];
            if (firstItem != null && firstItem.GetConnections().Contains(Opposite(sourceDirection)))
            {
                checkQueue.Enqueue(startPos);
            }
        }

        while (checkQueue.Count > 0)
        {
            Vector2Int current = checkQueue.Dequeue();
            if (visited.Contains(current)) continue;
            visited.Add(current);

            DraggableItem currentItem = placedItems[current.x, current.y];
            if (currentItem == null) continue;

            path.Add(currentItem);

            foreach (var dir in currentItem.GetConnections())
            {
                Vector2Int nextPos = current + DirectionToVector(dir);
                if (!IsInside(nextPos)) continue;

                DraggableItem nextItem = placedItems[nextPos.x, nextPos.y];
                if (nextItem == null || visited.Contains(nextPos)) continue;

                if (nextItem.GetConnections().Contains(Opposite(dir)))
                {
                    checkQueue.Enqueue(nextPos);
                }
            }
        }
        return path;
    }

    private bool CheckVictoryCondition(List<DraggableItem> path)
    {
        if (targets.Count == 0) return false;

        foreach (var target in targets)
        {
            Vector2Int neighborToTarget = target.position + DirectionToVector(target.direction);
            bool targetReached = false;

            if (IsInside(neighborToTarget))
            {
                DraggableItem lastItem = placedItems[neighborToTarget.x, neighborToTarget.y];
                
                if (lastItem != null && path.Contains(lastItem))
                {
                    if (lastItem.GetConnections().Contains(Opposite(target.direction)))
                    {
                        targetReached = true;
                    }
                }
            }

            if (!targetReached) return false;
        }
        return true;
    }

  
    private IEnumerator AnimatePowerFlow(List<DraggableItem> path)
    {
        float delayPerStep = 0.5f;

        foreach (var item in path)
        {
            item.SetPowered(true);
            yield return new WaitForSeconds(delayPerStep);
        }

        yield return new WaitForSeconds(0.3f);

        if (LevelWinUI.Instance != null)
        {
            LevelWinUI.Instance.ShowWin();
        }
    }

    private void StopPowerAnimation()
    {
        if (powerRoutine != null)
        {
            StopCoroutine(powerRoutine);
            powerRoutine = null;
        }
    }

    bool IsInside(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    void Awake()
    {
        Resources.UnloadUnusedAssets();
        GenerateGrid();
    }

    void GenerateGrid()
    {
        grid = new Tile[width, height];
        placedItems = new DraggableItem[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x + gridOffset.x, y + gridOffset.y, 1);
                GameObject obj = Instantiate(tilePrefab, pos, Quaternion.identity, transform);

                Tile tile = obj.GetComponent<Tile>();
                tile.gridPos = new Vector2Int(x, y);
                grid[x, y] = tile;
            }
        }
    }
}