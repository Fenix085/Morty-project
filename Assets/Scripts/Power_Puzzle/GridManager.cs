using UnityEngine;
using System.Collections.Generic;
public class GridManager : MonoBehaviour
{
    public int width = 7;
    public int height = 6;
    public GameObject tilePrefab;
    public Tile[,] grid;
    public DraggableItem[,] placedItems;
    public Vector2Int sourcePosition;
    public Direction sourceDirection;
    public Vector2Int targetPosition;
    public Direction targetDirection;

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
        Debug.Log("UpdatePower called");

        foreach (var item in placedItems)
        {
            if (item != null)
                item.SetPowered(false);
        }
        
        Vector2Int startPos = sourcePosition + DirectionToVector(sourceDirection);
        Debug.Log("StartPos: " + startPos);

        if (!IsInside(startPos)) return;

        DraggableItem startItem = placedItems[startPos.x, startPos.y];
        if (startItem == null)
        {
            Debug.Log("No channel at start position");
            return;
        }

        Debug.Log("Start item found");

        if (!startItem.GetConnections().Contains(Opposite(sourceDirection)))
        {
            Debug.Log("Channel NOT connected to source");
            return;
        }

        Debug.Log("Channel CONNECTED to source");
        
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(startPos);
        startItem.SetPowered(true);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            DraggableItem currentItem = placedItems[current.x, current.y];

            if (currentItem == null) continue;

            currentItem.SetPowered(true);

            foreach (var dir in currentItem.GetConnections())
            {
                Vector2Int nextPos = current + DirectionToVector(dir);

                if (!IsInside(nextPos)) continue;

                DraggableItem nextItem = placedItems[nextPos.x, nextPos.y];

                if (nextItem == null) continue;

                
                if (currentItem.GetConnections().Contains(dir) &&
                    nextItem.GetConnections().Contains(Opposite(dir)))
                {
                    if (!nextItem.isPowered)
                    {
                        nextItem.SetPowered(true);
                        queue.Enqueue(nextPos);
                    }
                }
            }
        }
        Vector2Int neighborToTarget = targetPosition + DirectionToVector(targetDirection);

        if (IsInside(neighborToTarget))
        {
            DraggableItem lastItem = placedItems[neighborToTarget.x, neighborToTarget.y];

            if (lastItem != null && lastItem.isPowered)
            {
                
                if (lastItem.GetConnections().Contains(Opposite(targetDirection)))
                {
                    Debug.Log("Victory!");
                    
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
                }
            }
        }
    }
    bool IsInside(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }
    void Start()
    {
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
                Vector3 pos = new Vector3(x, y, 1);
                GameObject obj = Instantiate(tilePrefab, pos, Quaternion.identity, transform);

                Tile tile = obj.GetComponent<Tile>();
                tile.gridPos = new Vector2Int(x, y);

                grid[x, y] = tile;
            }
        }
    }

}