using System.Collections.Generic;
using UnityEngine;

public class TinyPattern
{
    public string Name { get; set; }
    public float Weight { get; set; }

    public TinyPattern(string name, float weight)
    {
        Name = name;
        Weight = weight;
    }
}

public class RMGSData
{
    public string PathPattern { get => pathPattern; set => pathPattern = value; }

    public string PathConstraint { get => pathConstraint; set => pathConstraint = value; }

    public List<Vector2> BanPoints { get; set; }
    public List<TinyPattern> Patterns { get; set; }

    public static RMGSData instance()
    {
        if (_instance == null)
        {
            _instance = new RMGSData();
        }
        return _instance;
    }

    private RMGSData()
    {
        pathPattern = $"Assets/RMGSGUI/Resources/Sample/Patterns/";
        pathConstraint = $"Assets/RMGSGUI/Resources/Sample/Constraints.xml";
        BanPoints = new List<Vector2>();
        Patterns = new List<TinyPattern>();
    }

    private static RMGSData _instance;

    private string pathPattern;

    private string pathConstraint;
}
