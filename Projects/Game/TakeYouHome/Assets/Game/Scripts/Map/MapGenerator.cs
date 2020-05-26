using System.Collections.Generic;
using UnityEngine;

using RMGS.Args;
using UnityEngine.Tilemaps;
using Cinemachine;

/// <summary>
/// Charge for the actual map showing job.
/// </summary>
public class MapGenerator : MonoBehaviour
{
    public TinyResult Result { get => result; set => result = value; }
    public MapInfo MapInfo { get => mapInfo; }
    public Tilemap Map { get => tilemap; }

    public void Generate(in AssetBundle roadAssetBundle, in AssetBundle[] fillersAssetBundle)
    {
        if (result == null)
        {
            Debug.Log("> RESULT IS NULL, GENERATE FAILED.");
            return;
        }

        if (0 != PresetMap())
        {
            Debug.Log("> RESULT IS NULL, GENERATE FAILED.");
            return;
        }

        CreateRoadTiles(roadAssetBundle);
        SetRoadTiles();

        CreateFillerTiles(fillersAssetBundle);
        SetFillerTiles();

        SetMapBoundary();
    }

    private void SetMapBoundary()
    {
        var path = new Vector2[8] {
            new Vector2( -1,-1),
            new Vector2( -1,result.Height+1),
            new Vector2( -1,result.Height+1),
            new Vector2( result.Width+1,result.Height+1),
            new Vector2( result.Width+1,result.Height+1),
            new Vector2( result.Width+1,-1),
            new Vector2( result.Width+1,-1),
            new Vector2( -1,-1)
        };

        /*
        Debug.Log(path[0]);
        Debug.Log(path[1]);
        Debug.Log(path[2]);
        Debug.Log(path[3]);
        */

        var confinerCollider = GetComponent<EdgeCollider2D>();
        confinerCollider.points = path;
        var cf = FindObjectOfType<CinemachineConfiner>();
        cf.m_BoundingShape2D = confinerCollider;
    }

    private int PresetMap()
    {
        tilemap = GetComponent<Tilemap>();
        if (tilemap == null)
        {
            Debug.Log("> TILEMAP IS NULL, GENERATE FAILED.");
            return -1;
        }

        tilemap.ClearAllTiles();
        tilemap.layoutGrid.transform.position = Vector3.zero;
        tilemap.transform.position = Vector3.zero;

        return 0;
    }

    private void CreateRoadTiles(in AssetBundle roadAssetBundle)
    {
        if (roadTiles != null) return;
        roadTiles = new List<Tile>();

        // Load the asssetbundle where road sprites locate.
        if (roadAssetBundle == null)
        {
            Debug.Log("> ROAD ASSETBUNDLE LOADING FAILED.");
            return;
        }

        // Create tile instances.
        int T = 0;
        foreach (var pattern in result.Patterns)
        {
            var sprite = roadAssetBundle.LoadAsset<Sprite>(pattern.Item1);

            var srcTile = ScriptableObject.CreateInstance<RoadTile>();
            // srcTile.name = pattern.Item1;
            srcTile.sprite = sprite;
            srcTile.colliderType = Tile.ColliderType.None;

            int index = pattern.Item1.LastIndexOf("_") + 1;
            srcTile.type = roadEnumMap[pattern.Item1.Substring(index)];

            roadTiles.Add(srcTile);

            for (int t = 1; t < pattern.Item2; t++)
            {
                var tempTile = ScriptableObject.CreateInstance<RoadTile>();
                // tempTile.name = srcTile.name;
                tempTile.sprite = sprite;
                tempTile.colliderType = Tile.ColliderType.None;
                tempTile.type = srcTile.type;
                var m = tempTile.transform;
                m.SetTRS(Vector3.zero, Quaternion.Euler(0f, 0f, 90f * t), Vector3.one);
                tempTile.transform = m;
                roadTiles.Add(tempTile);
            }

            T += pattern.Item2;
        }

        roadAssetBundle.Unload(false);
    }

    /// <summary>
    /// Show roads on the map, meanwhile collect empty points for building filling.
    /// </summary>
    private void SetRoadTiles()
    {
        emptyPoints = new List<Vector2Int>();

        // Show the road on tilemap.
        if (result.Observed != null)
        {
            for (int x = 0; x < result.Width; ++x)
                for (int y = 0; y < result.Height; ++y)
                {
                    var point = new Vector2Int(x, y);
                    if (result.Observed[x + y * result.Width] == 14) // L I I I T X => (4 + 2 + 2 + 4 + 1) = 15
                    {
                        emptyPoints.Add(point);
                        continue;
                    }
                    tilemap.SetTile(ToMappoint(point), roadTiles[result.Observed[x + y * result.Width]]);
                }
        }
    }

    private void CreateFillerTiles(in AssetBundle[] fillerAssetBundles)
    {
        if (fillerTiles != null) return;
        fillerTiles = new List<List<BuildingScriptableObject>>();

        for (int i = 0; i < 4; ++i)
        {
            fillerTiles.Add(new List<BuildingScriptableObject>());
            if (fillerTiles[i].Count == 0)
            {
                //var assetBundle = assetBundleUtility.LoadAssetBundle($"fillers.unit{fillersize[i]}");
                var assetBundle = fillerAssetBundles[i];
                if (assetBundle == null)
                {
                    Debug.Log("> UNIT1 ASSETBUNDLE LOADING FAILED.");
                    return;
                }

                string[] names = assetBundle.GetAllAssetNames();
                for (int j = 0; j < names.Length; ++j)
                {
                    var tile = ScriptableObject.CreateInstance<BuildingScriptableObject>();
                    tile.SpriteSequence = assetBundle.LoadAssetWithSubAssets<Sprite>(names[j]);

                    int s = names[j].LastIndexOf("/") + 1;
                    int e = names[j].LastIndexOf(".");
                    tile.name = names[j].Substring(s, e - s);

                    if (tile.SpriteSequence.Length == 1) tile.Type = BuildingScriptableObject.TileType.Single;
                    else tile.Type = BuildingScriptableObject.TileType.Multiple;

                    fillerTiles[i].Add(tile);
                }
                assetBundle.Unload(false);
            }
        }
    }

    /// <summary>
    /// Fill empty points with terrain, buildings and landmarks. 
    /// Send map info to game logic for picking points.
    /// </summary>
    private void SetFillerTiles()
    {
        // 1. Divide regions then set landmarks first (save landmark points to mapinfo).
        // 2. Set buildings.
        // 3. Set boundaries.
        var available = new Dictionary<Vector2, bool>();
        foreach (Vector2 p in emptyPoints) available[p] = true;

        mapInfo = ScriptableObject.CreateInstance<MapInfo>();
        mapInfo.length = result.Width;

        #region Divide regions and set region filled marks.
        int regions = result.ChunkAmount;
        int regionRow = 2;
        int regionColumn = regions / regionRow;
        if (regions % regionRow != 0) regionColumn += 1;

        int unitWidth = result.Width / regionColumn;
        int unitHeight = result.Height / regionRow;

        var regionMarks = new List<bool>();
        for (int i = 0; i < regions; ++i) regionMarks.Add(false);
        bool IsRegionFilled(int x, int y)
        {
            int w = x / unitWidth;
            int h = y / unitHeight;
            // Debug.Log($"{x},{y} : {w},{h}");
            int index = Mathf.Clamp(h + w * regionRow, 0, regions - 1);
            return regionMarks[index];
        }
        void SetRegionFilled(int x, int y)
        {
            int w = x / unitWidth;
            int h = y / unitHeight;
            int index = Mathf.Clamp(h + w * regionRow, 0, regions - 1);
            regionMarks[index] = true;
        }
        #endregion

        #region Fillling.
        int landmarksLeft = regions;
        HashSet<int> includedLandmarkIndex = new HashSet<int>();
        for (int i = 0; i < emptyPoints.Count; ++i)
        {
            if (available[emptyPoints[i]])
            {
                int x = emptyPoints[i].x;
                int y = emptyPoints[i].y;

                bool notSet = true;
                // 0 : landmarks, 1 : 4unit, 2 : 2unit, 3 : 1unit
                for (int j = 0; j < 3 && notSet; ++j)
                {
                    if (Random.Range(1, 4) == 3) continue;
                    if (j == 0 && (landmarksLeft <= 0 || IsRegionFilled(x, y))) continue;
                    bool isLeagal = true;
                    // Check all tiles in the building size whether them are leagal.
                    for (int h = fillerDimension[j, 1] - 1; h >= 0 && isLeagal; --h)
                        for (int w = 0; w < fillerDimension[j, 0]; ++w)
                        {
                            var newpoint = new Vector2(x + w, y + h);
                            if (!available.ContainsKey(newpoint) || !available[newpoint]) { isLeagal = false; break; }
                        }

                    if (isLeagal)
                    {
                        // Set tile(s) accroding the building size.
                        int k = 0;
                        int r = Random.Range(0, fillerTiles[j].Count);
                        if (j == 0)
                        {
                            // Insure each landmark is placed once.
                            while (includedLandmarkIndex.Contains(r) == true)
                                r = Random.Range(0, fillerTiles[j].Count);
                        }
                        for (int h = 0; h < fillerDimension[j, 1] && isLeagal; ++h)
                            for (int w = 0; w < fillerDimension[j, 0]; ++w)
                            {
                                int newx = x + w;
                                int newy = y + h;
                                var newPoint = new Vector2Int(newx, newy);

                                var tile = ScriptableObject.CreateInstance<BuildingTile>();

                                tile.sprite = fillerTiles[j][r].SpriteSequence[k++];
                                tile.name = fillerTiles[j][r].name;
                                tile.type = buildingEnumMap[tile.name[0]];
                                tile.colliderType = Tile.ColliderType.Grid;
                                tilemap.SetTile(ToMappoint(newPoint), tile);

                                available[newPoint] = false;
                            }
                        if (j == 0)
                        {
                            landmarksLeft -= 1;
                            SetRegionFilled(x, y);
                            mapInfo.AddLandmark(new Vector2Int(x, y), fillerTiles[j][r].name);
                            includedLandmarkIndex.Add(r);
                        }
                        notSet = false;
                    }
                }
                // Finally fill with 1-unit building/terrain.
                if (notSet)
                {
                    int r = Random.Range(0, fillerTiles[3].Count);
                    var tile = ScriptableObject.CreateInstance<BuildingTile>();

                    var m = tile.transform;
                    m.SetTRS(Vector3.zero, Quaternion.Euler(0, 0, 90f * Random.Range(0, 4)), Vector3.one);
                    tile.transform = m;

                    tile.name = fillerTiles[3][r].name;
                    tile.sprite = fillerTiles[3][r].SpriteSequence[0];
                    tile.type = buildingEnumMap[tile.name[0]];
                    tilemap.SetTile(ToMappoint(emptyPoints[i]), tile);
                    available[emptyPoints[i]] = false;
                }
            }
        }
        #endregion

        #region Set circular expressway.
        int length = mapInfo.length;
        var expresswayTile = roadTiles[14];
        for (int i = -1; i < length; ++i)
        {
            tilemap.SetTile(new Vector3Int(-1, i, 0), expresswayTile);
            tilemap.SetTile(new Vector3Int(i, length, 0), expresswayTile);
        }
        for (int i = 0; i <= length; ++i)
        {
            tilemap.SetTile(new Vector3Int(length, i, 0), expresswayTile);
            tilemap.SetTile(new Vector3Int(i, -1, 0), expresswayTile);
        }
        #endregion
    }

    /// <summary>
    /// Transform in-image coord to tilemap coord. y-coord needs to be flipped.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    private Vector3Int ToMappoint(Vector2Int p) => new Vector3Int(p.x, result.Height - p.y - 1, 0);

    private TinyResult result;
    private MapInfo mapInfo;

    private Tilemap tilemap;
    private List<Tile> roadTiles;
    private List<List<BuildingScriptableObject>> fillerTiles;
    private List<Vector2Int> emptyPoints;

    private int[] fillersize = { 8, 4, 2, 1 };
    private int[,] fillerDimension = new int[4, 2] { { 4, 2 }, { 2, 2 }, { 2, 1 }, { 1, 1 } };
    private Dictionary<char, BuildingTileType> buildingEnumMap = new Dictionary<char, BuildingTileType>() {
        { 't', BuildingTileType.Terrain },
        { 'b', BuildingTileType.Building },
        { 'l', BuildingTileType.Landmark }
    };
    private Dictionary<string, RoadTileType> roadEnumMap = new Dictionary<string, RoadTileType>() {
        { "straight", RoadTileType.Straight },
        { "crosswalk", RoadTileType.Crosswalk },
        { "cross", RoadTileType.Cross },
        { "tbranch", RoadTileType.Tbranch },
        { "corner", RoadTileType.Corner },
        { "empty", RoadTileType.Empty }
    };
}
