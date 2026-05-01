using UnityEngine;
using System.Collections.Generic;
public class DraggableItem : MonoBehaviour
{
    private Vector3 offset;
    private bool dragging;
    public ChannelType type;
    public int rotation;
    public bool isPowered;
    private SpriteRenderer sr;
    private Vector2Int currentGridPos = new Vector2Int(-1, -1);
    public bool isLocked = false;
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetPowered(bool value)
    {
        isPowered = value;
        sr.color = value ? Color.yellow : Color.black;
    }

    public List<Direction> GetConnections()
    {
        List<Direction> dirs = new List<Direction>();

        switch (type)
        {
            case ChannelType.Straight:
                dirs.Add(Direction.Up);
                dirs.Add(Direction.Down);
                break;

            case ChannelType.Corner:
                dirs.Add(Direction.Up);
                dirs.Add(Direction.Right);
                break;

            case ChannelType.TShape:
                dirs.Add(Direction.Up);
                dirs.Add(Direction.Left);
                dirs.Add(Direction.Right);
                break;
        }

        
        for (int i = 0; i < dirs.Count; i++)
        {
            dirs[i] = RotateDirection(dirs[i], rotation);
        }

        return dirs;
    }

    Direction RotateDirection(Direction dir, int rotationCount)
    {
        // rotationCount — ??? ??????? ??? ?? ????????? ?? 90 ???????? (0, 1, 2, 3)
        return (Direction)(((int)dir + rotationCount) % 4);
    }

    void OnMouseDown()
    {
        if (isLocked) return;
        offset = transform.position - GetMouseWorldPos();
        dragging = true;
    }

    void OnMouseUp()
    {
        dragging = false;
        SnapToGrid();
    }
    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            rotation = (rotation + 1) % 4;
            transform.rotation = Quaternion.Euler(0, 0, -90 * rotation);

            GridManager gm = FindObjectOfType<GridManager>();
            if (gm != null)
            {
                gm.UpdatePower(gm.sourcePosition);
            }
        }

        DebugConnections();
    }
    void Update()
    {
        if (dragging)
        {
            transform.position = GetMouseWorldPos() + offset;
        }
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 mouse = Input.mousePosition;
        mouse.z = 10f;
        return Camera.main.ScreenToWorldPoint(mouse);
    }

    void ClearOldPosition()
    {
        GridManager gm = FindObjectOfType<GridManager>();
        if (gm == null) return;

        if (currentGridPos.x >= 0)
        {
            gm.placedItems[currentGridPos.x, currentGridPos.y] = null;
        }
    }

    void SnapToGrid()
    {
        int x = Mathf.RoundToInt(transform.position.x);
        int y = Mathf.RoundToInt(transform.position.y);
        GridManager gm = FindObjectOfType<GridManager>();

        if (gm != null && x >= 0 && x < gm.width && y >= 0 && y < gm.height)
        {
            transform.position = new Vector3(x, y, 0);
            RegisterToGrid(x, y);
        }
        else
        {
            ClearOldPosition();
            transform.position = startPosition;
        }
        gm.UpdatePower(gm.sourcePosition);
    }
    void RegisterToGrid(int x, int y)
    {
        GridManager gm = FindObjectOfType<GridManager>();
        if (gm == null)
        {
            Debug.LogError("GridManager NOT FOUND");
            return;
        }
        if (currentGridPos.x >= 0)
        {
            gm.placedItems[currentGridPos.x, currentGridPos.y] = null;
        }

        gm.placedItems[x, y] = this;
        currentGridPos = new Vector2Int(x, y);
    }
    void DebugConnections()
    {
        var dirs = GetConnections();
        string s = "Connections: ";

        foreach (var d in dirs)
            s += d + " ";

        Debug.Log(s);
    }
}