using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class Vector2Comparer : IComparer<Vector2>
{
    public int Compare(Vector2 v1, Vector2 v2)
    {
        if (v1.x == v2.x)
        {
            if (v1.y == v2.y) return 0;
            else return v1.y < v2.y ? 1 : -1;
        }
        else return v1.x < v2.x ? 1 : -1;
    }
}

[CustomEditor(typeof(RMGSGenerator))]
public class RMGSGeneratorEditor : Editor
{
    private RMGSGenerator rmgsGenerator;

    private float range = 1.0f;
    private bool isOn = false;
    private Vector3 point = Vector3.zero;
    private List<Vector2> points = RMGSData.instance().BanPoints;
    private List<TinyPattern> patterns = RMGSData.instance().Patterns;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        rmgsGenerator = (RMGSGenerator)target;

        Color defaultColor = GUI.color;

        GUILayout.Label("Set Pattern Weights", EditorStyles.boldLabel);
        GUILayout.BeginScrollView(Vector2.zero);
        if (patterns != null)
        {
            for (int i = 0; i < patterns.Count; ++i)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(patterns[i].Name, GUILayout.Width(50));
                var sprite = Resources.Load<Sprite>($"Sample/Patterns/{patterns[i].Name}");
                GUILayout.Box(sprite.texture);
                GUILayout.Space(10);
                patterns[i].Weight = GUILayout.HorizontalSlider(patterns[i].Weight, 0, 1.0f);
                GUILayout.Space(10);
                GUILayout.TextField(patterns[i].Weight.ToString("0.0"), GUILayout.Width(30));
                GUILayout.Space(10);
                GUILayout.EndHorizontal();
            }
            GUI.color = Color.white;
            if (GUILayout.Button("Default"))
            {
                foreach (var p in patterns) p.Weight = 1;
            }
        }
        GUILayout.EndScrollView();

        GUILayout.Label("Constraint Points", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        GUI.color = Color.green;
        if (GUILayout.Button("Point Selecting Mode")) isOn = !isOn;
        GUI.color = defaultColor;
        GUILayout.Box((isOn ? "ON" : "OFF"));
        GUI.color = Color.red;
        if (GUILayout.Button("Clear")) points.Clear();
        GUILayout.EndHorizontal();

        GUI.color = Color.cyan;
        if (GUILayout.Button("Generate"))
            rmgsGenerator.Generate();

        GUI.color = defaultColor;
    }

    private void OnSceneGUI()
    {
        if (!isOn) return;

        Handles.BeginGUI();
        GUI.color = Color.green;
        GUI.Box(new Rect(10, 10, 150, 20), "Point Selecting Mode");
        Handles.EndGUI();

        if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
        {
            // The purpos of generating a plane is for physics raycast, because a tilemap can't be hit by a ray.
            var p = new Plane(rmgsGenerator.Tilemap.transform.TransformDirection(Vector3.forward), rmgsGenerator.Tilemap.transform.position);
            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            float dist;
            if (p.Raycast(ray, out dist))
            {
                // Transform to the actual cell point.
                point = ray.origin + (ray.direction.normalized * dist);
                point = rmgsGenerator.GetCellPosition(point);

                // If current the clicked point exists in the list, remove it; or add it to the lis.
                Vector2 newpoint = new Vector2(point.x, point.y);
                Vector2Comparer vc = new Vector2Comparer();
                int index = points.BinarySearch(newpoint, vc);
                if (index < 0) points.Insert(~index, newpoint);
                else points.RemoveAt(index);
            }
        }

        // Must be update every invoke.
        // Traverse clicked tiles and draw them.
        for (int i = 0; i < points.Count; ++i)
        {
            var p = points[i];
            Vector3[] verts = new Vector3[]
            {
                    new Vector3(p.x, p.y, 0),
                    new Vector3(p.x + range, p.y, 0),
                    new Vector3(p.x + range, p.y + range, 0),
                    new Vector3(p.x, p.y + range, 0)
            };
            Handles.DrawSolidRectangleWithOutline(verts,
                new Color(.729f, .945f, .631f, 0.8f),
                new Color(0, 0, 0, 1)
                );
        }
    }
}
