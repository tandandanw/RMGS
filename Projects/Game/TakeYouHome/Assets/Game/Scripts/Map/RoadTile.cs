using UnityEngine.Tilemaps;

public enum RoadTileType
{
    Straight = 0,
    Crosswalk = 1,
    Cross = 2,
    Tbranch = 3,
    Corner = 4,
    Empty = 5
}

public class RoadTile : Tile
{
    public RoadTileType type { get; set; }

    public RoadTile() : base()
    {
    }
}
