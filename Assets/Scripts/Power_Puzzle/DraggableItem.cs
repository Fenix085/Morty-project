using UnityEngine;

public class DraggableItem : MonoBehaviour
{
    private Vector3 offset;
    private bool dragging;
    public ChannelType type;
    public int rotation;
    public bool isPowered;
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetPowered(bool value)
    {
        isPowered = value;
        sr.color = value ? Color.yellow : Color.white;
    }

    void OnMouseDown()
    {
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

        for (int x = 0; x < gm.width; x++)
        {
            for (int y = 0; y < gm.height; y++)
            {
                if (gm.placedItems[x, y] == this)
                    gm.placedItems[x, y] = null;
            }
        }
    }
    void SnapToGrid()
    {
        int x = Mathf.RoundToInt(transform.position.x);
        int y = Mathf.RoundToInt(transform.position.y);

        GridManager gm = FindObjectOfType<GridManager>();

        if (gm == null) return;

        if (x >= 0 && x < gm.width && y >= 0 && y < gm.height)
        {
            transform.position = new Vector3(x, y, 0);
            RegisterToGrid(x, y);
        }
    }
    void RegisterToGrid(int x, int y)
    {
        GridManager gm = FindObjectOfType<GridManager>();
        if (gm == null) return;

        gm.placedItems[x, y] = this;
    }
}