using UnityEngine;

public class SourceMarker : MonoBehaviour
{
    public Direction outputDirection;

    void Start()
    {
        GridManager gm = FindObjectOfType<GridManager>();
        if (gm == null) return;

        Vector2Int pos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y)
        );

        gm.sourcePosition = pos;
        gm.sourceDirection = outputDirection;
        
        if (pos.x >= 0 && pos.x < gm.width && pos.y >= 0 && pos.y < gm.height)
        {
            gm.placedItems[pos.x, pos.y] = null;
        }
        gm.UpdatePower(pos);
    }
}