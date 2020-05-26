using UnityEngine;
using UnityEngine.Tilemaps;

using RMGS.Core;
using RMGS.Args;
using RMGS.Import;
using RMGS.Export;
using RMGS.GUI;
using System.Collections.Generic;

[RequireComponent(typeof(Tilemap))]
public class RMGSGenerator : MonoBehaviour
{
    public Tilemap Tilemap { get => tilemap; }

    private Tilemap tilemap;
    private RMGSData rmgsData;
    private TinyResult result;
    private Importer importer;

    private int[,] fillLayerSize = new int[4, 2] { { 4, 2 }, { 2, 2 }, { 2, 1 }, { 1, 1 } };
    private bool noFillers = true;
    private BuildingTile[] fillers;

    /// <summary>
    /// Get the closest cell point of a given world point.
    /// </summary>
    /// <param name="world"></param>
    /// <returns></returns>
    public Vector3 GetCellPosition(Vector3 world)
    {
        Vector3Int cell = tilemap.layoutGrid.WorldToCell(world);
        return tilemap.layoutGrid.CellToWorld(cell);
    }

    private void Init()
    {
        if (tilemap == null)
        {
            tilemap = GetComponent<Tilemap>();

            // Insure position always zero for screen points picking.
            tilemap.layoutGrid.transform.position = Vector3.zero;
            tilemap.transform.position = Vector3.zero;
        }
        if (rmgsData == null)
        {
            rmgsData = RMGSData.instance();
        }
        // Read config files.
        if (importer == null)
        {
            importer = new Importer(rmgsData.PathPattern, rmgsData.PathConstraint, Platform.RMGSGUI);
            if (importer == null) Debug.Log("> IMPORTER IS NULL.");
            else
            {
                Argument argument = importer.Import();
                // For patterns UI of InspectorUI.
                rmgsData.Patterns.Clear();
                foreach (Pattern p in importer.Argument.Patterns)
                    rmgsData.Patterns.Add(new TinyPattern(p.Name, (float)p.Weight));
            }
        }
    }

    private void Fill()
    {
        if (noFillers)
        {
            var tmp = new List<BuildingTile>();
            for (int i = 0; i < fillLayerSize.GetLength(0); ++i)
            {
                int w = fillLayerSize[i, 0];
                int h = fillLayerSize[i, 1];

                if ((w == 1) && (h == 1))
                {
                    for (int j = 1; j <= 3; ++j)
                    {
                        var buildingTile = ScriptableObject.CreateInstance<BuildingTile>();
                        buildingTile.Type = TileType.Single;
                        buildingTile.SpriteSequence = new Sprite[] { Resources.Load<Sprite>($"Buildings/building_unit1_{j}") };
                        tmp.Add(buildingTile);
                    }
                }
                else
                {
                    var buildingTile = ScriptableObject.CreateInstance<BuildingTile>();
                    buildingTile.Type = TileType.Multiple;
                    buildingTile.SpriteSequence = Resources.LoadAll<Sprite>($"Buildings/building_unit{w * h}");
                    tmp.Add(buildingTile);
                }
            }
            fillers = tmp.ToArray();
            Debug.Log(tmp.Count);
            noFillers = false;
        }

        // Fill by layers.
        var available = new Dictionary<Vector2, bool>();
        List<Vector2> banPoints = rmgsData.BanPoints;
        foreach (Vector2 p in banPoints) available[p] = true;

        // Pick a point => Check its legality => Setting tile.
        int[] remains = { 1, 1, 1 };
        for (int i = 0; i < banPoints.Count; ++i)
        {
            if (available[banPoints[i]])
            {
                int x = (int)banPoints[i].x;
                int y = (int)banPoints[i].y;

                // Try unit 8,4,2 then 1 in turns.
                bool notSet = true;
                for (int j = 0; j < 3 && notSet; ++j)
                {
                    if (Random.Range(1, 4) == 3) continue;
                    if (remains[j] > 0)
                    {
                        bool isLeagal = true;
                        for (int h = fillLayerSize[j, 1] - 1; h >= 0 && isLeagal; --h)
                            for (int w = 0; w < fillLayerSize[j, 0]; ++w)
                            {
                                var newpoint = new Vector2(x + w, y + h);
                                if (!available.ContainsKey(newpoint) || !available[newpoint]) { isLeagal = false; break; }
                            }

                        if (isLeagal)
                        {
                            int k = 0;
                            for (int h = fillLayerSize[j, 1] - 1; h >= 0 && isLeagal; --h)
                                for (int w = 0; w < fillLayerSize[j, 0]; ++w)
                                {
                                    int newx = x + w;
                                    int newy = y + h;
                                    available[new Vector2(newx, newy)] = false;
                                    var tmpTile = ScriptableObject.CreateInstance<Tile>();
                                    tmpTile.sprite = fillers[j].SpriteSequence[k++];
                                    tilemap.SetTile(new Vector3Int(newx, newy, 0), tmpTile);
                                }
                            remains[j] -= 1;
                            notSet = false;
                        }
                    }
                }
                if (notSet)
                {
                    int r = Random.Range(3, 6);
                    var t = ScriptableObject.CreateInstance<Tile>();
                    t.sprite = fillers[r].SpriteSequence[0];
                    tilemap.SetTile(new Vector3Int(x, y, 0), t);
                }
            }
        }
    }

    public void Generate()
    {
        Init();
        if (importer == null) Debug.Log("> IMPORTER IS NULL.");
        else
        {
            // Setting weights.
            for (int i = 0; i < importer.Argument.Patterns.Length; ++i)
                importer.Argument.Patterns[i].Weight = rmgsData.Patterns[i].Weight;

            // Remove all the real-time constraints.
            int index = 0; ;
            for (; index < importer.Argument.Constraints.Count; ++index)
                if (importer.Argument.Constraints[index] is RealtimeConstraint)
                    break;
            importer.Argument.Constraints.RemoveRange(index, importer.Argument.Constraints.Count - index);

            // Setting ban points as real-time constraint.
            int width = importer.Argument.Width;
            int height = importer.Argument.Height;
            foreach (Vector2 p in rmgsData.BanPoints)
            {
                int x = (int)p.x;
                int y = height - 1 - (int)p.y;
                importer.Argument.Constraints.Add(new RealtimeConstraint(x + width * y));
            }

            // Run WFC.
            var model = new TileModel(importer.Argument, false, false);
            var random = new System.Random();
            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            for (int k = 0; k < 1; k++)
            {
                int seed = random.Next();
                bool finished = model.Run(seed, 0);
                if (finished)
                {
                    Debug.Log("> GENERATION DONE");
                    var exporter = new Exporter(importer.Argument, model.Result);
                    result = exporter.ExportToUnity();
                    var guiExporter = new GUIExporter(result, ref tilemap, rmgsData.PathPattern);
                    guiExporter.Export();

                    // Fill ban points.
                    Fill();
                    break;
                }
                else Debug.Log("> CONTRADICTION");
            }
            watch.Stop();
            Debug.Log($"> TIME ELAPSE: {watch.ElapsedMilliseconds} ms)");
        }
    }

    private void OnEnable() => Init();
}
