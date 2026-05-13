using UnityEngine;

public class TargetMarker : MonoBehaviour
{
    
    public Direction inputDirection;

    void Start()
    {
        GridManager gm = FindObjectOfType<GridManager>();
        if (gm != null)
        {
            
            Vector2Int pos = new Vector2Int(
                Mathf.RoundToInt(transform.position.x - gm.gridOffset.x),
                Mathf.RoundToInt(transform.position.y - gm.gridOffset.y)
            );

            
            gm.RegisterTarget(pos, inputDirection);
        }
    }
}