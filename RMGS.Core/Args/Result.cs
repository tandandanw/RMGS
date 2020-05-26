using System;
using System.Collections.Generic;
using System.Text;

namespace RMGS.Args
{
    public class Result
    {
        public bool Black { get; set; }
        public bool Periodic { get; set; }
        public int ChunkAmount { get; set; }

        public bool[][] Wave { get; set; }
        public int[] Observed { get; set; }
        public double[] Weights { get; set; }

        // (pattern name, cardinality)
        public (string, int)[] Patterns { get; set; }
        public int T { get; set; }

        public Result(bool black, bool periodic, int chunkAmount)
        {
            Black = black;
            Periodic = periodic;
            ChunkAmount = chunkAmount;
        }

        public void Fill(bool[][] wave, int[] observed, double[] weights)
        {
            Wave = wave;
            Observed = observed;
            Weights = weights;
        }
    }

    public class TinyResult
    {
        public int Width { get; }
        public int Height { get; }
        public int Tilesize { get; }
        public int ChunkAmount { get; }
        public int[] Observed { get; }
        public (string, int)[] Patterns { get; }

        public TinyResult(int width, int height, int tilesize, int chunkAmount, int[] observed, (string, int)[] patterns)
        {
            Width = width;
            Height = height;
            Tilesize = tilesize;
            ChunkAmount = chunkAmount;
            Observed = observed;
            Patterns = patterns;
        }
    }
}
