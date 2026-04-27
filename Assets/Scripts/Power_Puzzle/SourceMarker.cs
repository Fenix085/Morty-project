using UnityEngine;

public class SourceMarker : MonoBehaviour
{
    void Start()
    {
        GridManager gm = FindObjectOfType<GridManager>();
        Vector2Int pos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y)
        );

        gm.placedItems[pos.x, pos.y] = null;
    }
}