using UnityEngine;

public enum TileType
{
    Normal,
    Item,
    Healing,
    Danger,
    Point,
    Trophy
}

public class BoardTile : MonoBehaviour
{
    public TileType tileType;
    public Transform standPoint;
    public bool canSpawnTrophy = false;

    [Header("Pathing")]
    public BoardTile nextTile;
    public BoardTile branchTile;

    public bool HasBranch()
    {
        return branchTile != null;
    }
}