using UnityEngine;

public class TargetMarker : MonoBehaviour
{
    
    public Direction inputDirection;

    void Start()
    {
        GridManager gm = FindObjectOfType<GridManager>();
        if (gm != null)
        {
            gm.targetPosition = new Vector2Int(
                Mathf.RoundToInt(transform.position.x),
                Mathf.RoundToInt(transform.position.y)
            );
            gm.targetDirection = inputDirection;
        }
    }
}