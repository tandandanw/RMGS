using System.Collections.Generic;
using UnityEngine;

public class LandmarkInfo
{
    public LandmarkInfo(Vector2Int pos, string name)
    {
        this.pos = pos;
        this.name = name;
    }

    public Vector2Int pos;
    public string name;
}

public class MapInfo : ScriptableObject
{
    public int length { get; set; }
    public List<LandmarkInfo> Landmarks { get => landmarks; }

    public MapInfo()
    {
        landmarks = new List<LandmarkInfo>();
    }

    public void AddLandmark(in Vector2Int point, in string name)
    {
        Landmarks.Add(new LandmarkInfo(point, name));
    }

    private List<LandmarkInfo> landmarks;

}
