using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;

using RMGS.Args;

namespace RMGS.GUI
{
    public class GUIExporter
    {
        public GUIExporter(in TinyResult tinyResult, ref Tilemap tilemap, string pathPattern)
        {
            result = tinyResult;
            this.pathPattern = pathPattern;
            this.tilemap = tilemap;
        }

        private List<Tile> tiles = new List<Tile>();
        private TinyResult result;
        private Tilemap tilemap;
        private string pathPattern;

        public void Export()
        {
            CreateTiles();
            tilemap.ClearAllTiles();

            if (result.Observed != null)
            {
                for (int x = 0; x < result.Width; ++x) for (int y = 0; y < result.Height; ++y)
                        tilemap.SetTile(
                            // Attention : y-coord need to be flip.
                            new Vector3Int(x, result.Height - y - 1, 0),
                            tiles[result.Observed[x + y * result.Width]]
                            );
            }
        }

        private void CreateTiles()
        {
            tiles.Clear();

            int T = 0;
            foreach (var pattern in result.Patterns)
            {
                string path = pathPattern.Substring(
                        pathPattern.IndexOf("/", pathPattern.IndexOf("Resources"), StringComparison.Ordinal) + 1)
                    + pattern.Item1;

                var sprite = Resources.Load<Sprite>(path);

                Tile srcTile = ScriptableObject.CreateInstance<Tile>();
                srcTile.sprite = sprite;
                tiles.Add(srcTile);

                for (int t = 1; t < pattern.Item2; t++)
                {
                    Tile tempTile = ScriptableObject.CreateInstance<Tile>();
                    tempTile.sprite = sprite;
                    var m = tempTile.transform;
                    m.SetTRS(Vector3.zero, Quaternion.Euler(0f, 0f, 90f * t), Vector3.one);
                    tempTile.transform = m;
                    tiles.Add(tempTile);
                }

                T += pattern.Item2;
            }
        }
    }
}
