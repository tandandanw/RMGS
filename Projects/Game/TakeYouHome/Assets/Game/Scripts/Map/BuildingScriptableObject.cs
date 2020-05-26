using UnityEngine;


/// <summary>
/// Abstract layer of building tile.
/// </summary>
public class BuildingScriptableObject : ScriptableObject
{
    public enum TileType
    {
        Single,
        Multiple
    }

    public TileType Type;
    public Sprite[] SpriteSequence;

    public BuildingScriptableObject() { }
}
