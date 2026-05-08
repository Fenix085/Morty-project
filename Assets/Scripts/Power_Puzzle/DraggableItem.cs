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
    public Sprite unpoweredSprite;
    public Sprite poweredSprite;

    public AudioClip snapSound;
    public AudioClip rotateSound;
    public AudioClip powerUpSound;
    void Start()
    {
        startPosition = transform.position;
        UpdateVisuals();
        if (isLocked)
        {
            SnapToGrid();
        }
    }
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetPowered(bool value)
    {
        if (isPowered == value) return;

        if (value && !isPowered)
        {
            if (SoundEffectsManager.Instance != null && powerUpSound != null)
                SoundEffectsManager.Instance.Play(powerUpSound);
        }

        isPowered = value;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (sr == null) return;

        sr.sprite = isPowered ? poweredSprite : unpoweredSprite;
        
        sr.color = Color.white;

        transform.rotation = Quaternion.Euler(0, 0, -90f * rotation);
    }

    public List<Direction> GetConnections()
    {
        List<Direction> dirs = new List<Direction>();

        switch (type)
        {
            case ChannelType.Straight:
                dirs.Add(Direction.Left);
                dirs.Add(Direction.Right);
                break;

            case ChannelType.Corner:
                dirs.Add(Direction.Left);
                dirs.Add(Direction.Down);
                break;

            case ChannelType.TShape:
                dirs.Add(Direction.Left);
                dirs.Add(Direction.Down);
                dirs.Add(Direction.Right);
                break;
            case ChannelType.Wall:
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
            UpdateVisuals();

            GridManager gm = FindObjectOfType<GridManager>();
            if (gm != null)
            {
                gm.UpdatePower(gm.sourcePosition);
            }

            if (SoundEffectsManager.Instance != null && rotateSound != null)
            {
                SoundEffectsManager.Instance.Play(rotateSound);
            }
        }
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
        GridManager gm = FindObjectOfType<GridManager>();
        if (gm == null) return;

        int x = Mathf.RoundToInt(transform.position.x - gm.gridOffset.x);
        int y = Mathf.RoundToInt(transform.position.y - gm.gridOffset.y);

        if (x >= 0 && x < gm.width && y >= 0 && y < gm.height)
        {
            
            if (new Vector2Int(x, y) == gm.sourcePosition)
            {
                BackToTray();
                return;
            }

            
            if (gm.placedItems[x, y] != null && gm.placedItems[x, y] != this)
            {
                BackToTray();
                return;
            }

            transform.position = new Vector3(x + gm.gridOffset.x, y + gm.gridOffset.y, 0);
            RegisterToGrid(x, y);
        }
        else
        {
            
            BackToTray();
        }
        
        if (SoundEffectsManager.Instance != null && snapSound != null)
        {
            SoundEffectsManager.Instance.Play(snapSound);
        }
        
        gm.UpdatePower(gm.sourcePosition);
        UpdateVisuals();
    }

    void BackToTray()
    {
        ClearOldPosition();
        transform.position = startPosition;
        SetPowered(false);
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
}