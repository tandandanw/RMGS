using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType
{
    Single,
    Multiple
}

public class BuildingTile : ScriptableObject
{
    public TileType Type;
    public Sprite[] SpriteSequence;

    public BuildingTile()
    {
        Type = TileType.Multiple;
        SpriteSequence = null;
    }

    public BuildingTile(TileType type, Sprite[] spriteSequence)
    {
        Type = type;
        SpriteSequence = spriteSequence;
    }
}
