using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class MapPointPicker
{
    public static LandmarkInfo PickNearestLandmark(in MapInfo mapInfo, Vector2Int point)
    {
        // Debug.Log("POINT: " + point);
        List<LandmarkInfo> landmarks = mapInfo.Landmarks;
        float minDistance = float.MaxValue;
        int minIndex = 0;
        for (int i = 0; i < landmarks.Count; ++i)
        {
            float distance = Vector2Int.Distance(point, landmarks[i].pos);
            // Debug.Log(landmarks[i].name + ", " + landmarks[i].pos + ", " + distance);
            if (minDistance >= distance)
            {
                minIndex = i;
                minDistance = distance;
            }
        }
        return landmarks[minIndex];
    }

    /// <summary>
    /// Parameter num indicates how many points should be picked (num <= 8).
    /// </summary>
    /// <param name="mapInfo"></param>
    /// <param name="num"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    public static Tile[] PickPointsFromNearby8(in Tilemap map, in MapInfo mapInfo, int num, Vector2Int point)
    {
        // Picking buildings first, then roads.
        List<Tile> tiles = new List<Tile>();
        bool isBuildingRunout = false;
        int i = 0;
        while (tiles.Count < num)
        {
            int x = dx[i] + point.x;
            int y = dy[i] + point.y;

            var tile = map.GetTile(new Vector3Int(x, y, 0));

            if (isBuildingRunout)
            {
                if ((tile is RoadTile) && (Random.Range(0, 3) == 0))
                    tiles.Add(map.GetTile(new Vector3Int(x, y, 0)) as Tile);

            }
            else
            {
                if (tile is BuildingTile)
                    tiles.Add(map.GetTile(new Vector3Int(x, y, 0)) as Tile);
            }


            if (++i == 8)
            {
                isBuildingRunout = true;
                i = 0;
            }
        }
        return tiles.ToArray();
    }

    /// <summary>
    /// A starting point has to be next to a road tile (reachable).
    /// </summary>
    /// <param name="map"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    public static Vector2Int PickStartingPoint(in Tilemap map, Vector2Int point)
    {
        int allCounter = 30;
        int lapPickingCounter = 5;
        int lap = Random.Range(1, 5);
        do
        {
            if (lapPickingCounter > 0)
            {
                int x = point.x + Random.Range(-lap, lap + 1);
                int y = point.y + Random.Range(-lap, lap + 1);

                if (x < 0 || x > map.size.x || y < 0 || y > map.size.y) break;

                for (int i = 0; i < 8; i += 2)
                {
                    int nx = dx[i] + point.x;
                    int ny = dy[i] + point.y;

                    if (nx < 0 || nx > map.size.x || ny < 0 || ny > map.size.y) continue;

                    var tile = map.GetTile(new Vector3Int(nx, ny, 0));
                    if (tile is RoadTile)
                        return new Vector2Int(x, y);
                }
            }
            else
            {
                lapPickingCounter = 5;
                lap = Random.Range(1, 5);
            }
        } while (--allCounter > 0);
        return new Vector2Int(lap + Random.Range(0, 5), lap + Random.Range(0, 5));
    }

    /// <summary>
    /// A terminalPoint has to be:
    /// 1. keeping a distance from the starting point.
    /// 2. a building tile.
    /// 3. next to a road tile (reachable).
    /// </summary>
    /// <param name="map"></param>
    /// <param name="point"></param>
    /// <param name="start"></param>
    /// <returns></returns>
    public static Vector2Int PickTerminalPoint(in Tilemap map, Vector2Int start)
    {
        int lapPickingCounter = 35;
        int minLap = Mathf.Max(
            Mathf.Min(start.x, map.size.x - start.x),
            Mathf.Min(start.y, map.size.y - start.y));
        int maxLap = Mathf.Min(
            Mathf.Max(start.x, map.size.x - start.x),
            Mathf.Max(start.y, map.size.y - start.y));

        do
        {
            int x = start.x;
            int y = start.y;

            if (Random.Range(0, 2) == 0)
            {
                x += Random.Range(minLap, maxLap + 1);
                y += Random.Range(minLap, maxLap + 1);
            }
            else
            {
                x -= Random.Range(minLap, maxLap + 1);
                y -= Random.Range(minLap, maxLap + 1);
            }

            if (x < 0 || x > map.size.x || y < 0 || y > map.size.y ||
                ((map.GetTile(new Vector3Int(x, y, 0)) is BuildingTile) == false)) continue;

            for (int i = 0; i < 8; i += 2)
            {
                int nx = dx[i] + start.x;
                int ny = dy[i] + start.y;

                if (nx < 0 || nx > map.size.x || ny < 0 || ny > map.size.y) continue;

                var nearTile = map.GetTile(new Vector3Int(nx, ny, 0));
                if (nearTile is RoadTile)
                    return new Vector2Int(x, y);
            }

            if (lapPickingCounter <= 0)
            {
                minLap += Random.Range(-3, 3);
                maxLap += Random.Range(-3, 3);
                lapPickingCounter = 20;
            }

        } while (--lapPickingCounter > 0);
        return Vector2Int.zero;
    }

    // 8 directions (colockwise) : UP => RIGHT => DOWN => LEFT
    private static int[] dx = { 0, 1, 1, 1, 0, -1, -1, -1 };
    private static int[] dy = { 1, 1, 0, -1, -1, -1, 0, 1 };
}
