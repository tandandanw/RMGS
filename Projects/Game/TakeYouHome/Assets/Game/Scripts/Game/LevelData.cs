using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Store runtime status of a level.
/// </summary>
public class LevelData : ScriptableObject
{
    public LevelData() : base() { }

    public int CompletionNum { get; set; } = 0;

    public Sprite[] DestinationSprites { get; set; }
}
