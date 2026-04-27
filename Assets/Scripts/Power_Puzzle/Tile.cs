using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int gridPos;

    public bool isOccupied = false;
    public bool isPowered = false;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetPowered(bool value)
    {
        isPowered = value;
        sr.color = value ? Color.yellow : Color.gray;
    }
}