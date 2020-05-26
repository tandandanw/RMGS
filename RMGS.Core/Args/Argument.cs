using System;
using System.Collections.Generic;

namespace RMGS.Args
{
    [Flags]
    public enum Platform { RMGSGUI = 1, RMGSConsole = 2, Game = 3 }

    public class Argument
    {
        public string SetName { get; set; }

        public int PatternSize { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int ChunkAmount { get; set; }
        public Pattern[] Patterns { get; set; }
        public List<Constraint> Constraints { get; set; }

        public string PathPattern { get; set; } = ".";
        public string PathConstraint { get; set; } = ".";
        public string PathOutput { get; set; } = ".";
        public bool IsSlience { get; set; } = false;

        public Platform Platform { get; set; } = Platform.RMGSConsole;

        public Argument(Platform platform)
        {
            Platform = platform;
        }

        public Argument(string pathPattern, string pathConstraint, Platform platform)
        {
            PathPattern = pathPattern;
            PathConstraint = pathConstraint;
            Platform = platform;
        }
        public Argument(string pathPattern, string pathConstraint, string pathOutput, bool isSlience, Platform platform)
        {
            PathPattern = pathPattern;
            PathConstraint = pathConstraint;
            PathOutput = pathOutput;
            IsSlience = isSlience;
            Platform = platform;
        }
        public Argument(string pathPattern, string pathConstraint, string pathOutput, bool isSlience, string setName,
            int tilesize, int width, int height, int chunkAmount, Pattern[] patterns, Platform platform)
        {
            PathPattern = pathPattern;
            PathConstraint = pathConstraint;
            PathOutput = pathOutput;
            IsSlience = isSlience;
            SetName = setName;
            PatternSize = tilesize;
            Width = width;
            Height = height;
            ChunkAmount = chunkAmount;
            Patterns = patterns;
            Platform = platform;
        }
    }
}
