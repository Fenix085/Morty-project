using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 6;
    public int height = 6;

    public GameObject tilePrefab;

    public Tile[,] grid;

    public DraggableItem[,] placedItems;

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        grid = new Tile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x, y, 0);
                GameObject obj = Instantiate(tilePrefab, pos, Quaternion.identity, transform);

                Tile tile = obj.GetComponent<Tile>();
                tile.gridPos = new Vector2Int(x, y);

                grid[x, y] = tile;
                placedItems = new DraggableItem[width, height];
            }
        }
    }

}