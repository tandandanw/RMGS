using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TaskPlanner : MonoBehaviour
{
    public GameObject StartingPointPrefab;
    public GameObject TerminalPointPrefab;

    public void StartNewRound(UIController uiController, GameObject player, LevelData levelData, Tilemap map, MapInfo mapInfo)
    {
        this.levelData = levelData;
        this.player = player;
        this.map = map;
        this.mapInfo = mapInfo;
        this.uiController = uiController;

        uiController.SetTaskNumber(0);
        StartCoroutine(StartTaskPlanning());
    }

    /// <summary>
    /// while 
    ///   find a starting point
    ///   wait player approach.
    ///   if player come
    ///     find a terminal point
    ///     wait player come
    ///     add task num
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartTaskPlanning()
    {
        GameObject currentStartingPoint = null;
        GameObject currentTerminalPoint = null;
        yield return new WaitForSeconds(5);

        var taxi = player.GetComponent<Taxi>();
        while (true)
        {
            Vector2Int startingPoint = MapPointPicker.PickStartingPoint(map, getPlayerPoint());

            // If accepted, break;
            if (currentStartingPoint) DestroyImmediate(currentStartingPoint);
            currentStartingPoint = Instantiate(StartingPointPrefab);
            currentStartingPoint.transform.position = new Vector3Int(startingPoint.x, startingPoint.y, 0);
            yield return new WaitForSeconds(1);

            // Wait for accepting.
            isIn = false;
            yield return StartCoroutine(WaitingPlayerToPoint(startingPoint, 3, 500));
            if (isIn)
            {
                taxi.PlayStartingHintSound();
                taxi.IsForhire = false;

                // Find a illeage terminal point.
                Vector2Int terminalPoint = MapPointPicker.PickTerminalPoint(map, startingPoint);
                if (currentTerminalPoint) DestroyImmediate(currentTerminalPoint);
                currentTerminalPoint = Instantiate(TerminalPointPrefab);
                currentTerminalPoint.transform.position = new Vector3Int(terminalPoint.x, terminalPoint.y, 0);

                // Set UI.
                List<Sprite> ls = new List<Sprite>();
                Tile[] tiles = MapPointPicker.PickPointsFromNearby8(map, mapInfo, 3, terminalPoint);
                Array.ForEach<Tile>(tiles, t => ls.Add(t.sprite));
                uiController.DestinationChange(ls.ToArray());

                // Find nearest landmark.
                var lmp = MapPointPicker.PickNearestLandmark(mapInfo, terminalPoint);
                uiController.SetLandmarkHint(lmp.name.Replace("landmark_", ""));

                // Wait player's coming.
                isIn = false;
                yield return StartCoroutine(WaitingPlayerToPoint(terminalPoint, 4));
                if (isIn)
                {
                    uiController.AddTaskNumber();
                    taxi.PlayTerminalHintSound();
                    taxi.IsForhire = true;
                    if (currentTerminalPoint) DestroyImmediate(currentTerminalPoint);
                }
            }

            // TODO: if level ended, break.

            yield return new WaitForSeconds(5);
        }
    }

    private IEnumerator WaitingPlayerToPoint(Vector2Int destPoint, float distance, int interval = int.MaxValue)
    {
        while (interval-- > 0)
        {
            // Debug.Log(getPlayerPoint() + "=>" + destPoint + " = " + Vector2Int.Distance(getPlayerPoint(), destPoint) + " | " + player.GetComponent<Taxi>().IsOpendoor);
            if (Vector2Int.Distance(getPlayerPoint(), destPoint) < distance && player.GetComponent<Taxi>().IsOpendoor == true)
            {
                isIn = true;
                interval = -1;
                yield break;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private Vector2Int getPlayerPoint()
    {
        return new Vector2Int((int)player.transform.position.x, (int)player.transform.position.y);
    }

    private UIController uiController;
    private GameObject player;
    private LevelData levelData;
    private MapInfo mapInfo;
    private Tilemap map;
    private bool isIn;
}
