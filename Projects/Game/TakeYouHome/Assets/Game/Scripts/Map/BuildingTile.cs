using UnityEngine;
using UnityEngine.Tilemaps;

public enum BuildingTileType
{
    Terrain = 0,
    Building = 1,
    Landmark = 2
}

public class BuildingTile : Tile
{
    public BuildingTileType type { get; set; }
    public BuildingTile() : base()
    {

    }
}
