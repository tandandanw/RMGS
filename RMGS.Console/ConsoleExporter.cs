using System;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

using RMGS.Args;

namespace RMGS.Console
{
    class ConsoleExporter
    {
        public ConsoleExporter(Argument argument, Result result)
        {
            this.argument = argument;
            this.result = result;
        }

        public void Export()
        {
            int FMX = argument.Width, FMY = argument.Height;
            int tilesize = argument.PatternSize;

            // Create tiles.
            var tiles = new List<Color[]>();

            Color[] newtile(Func<int, int, Color> f)
            {
                Color[] result = new Color[tilesize * tilesize];
                for (int y = 0; y < tilesize; y++) for (int x = 0; x < tilesize; x++) result[x + y * tilesize] = f(x, y);
                return result;
            };

            Color[] rotate(Color[] array) => newtile((x, y) => array[tilesize - 1 - y + x * tilesize]);

            int T = 0;
            foreach (var pattern in result.Patterns)
            {
                Bitmap bitmap = new Bitmap($"{argument.PathPattern}/{pattern.Item1}.png");
                tiles.Add(newtile((x, y) => bitmap.GetPixel(x, y)));

                for (int t = 1; t < pattern.Item2; t++)
                    tiles.Add(rotate(tiles[T + t - 1]));
                T += pattern.Item2;
            }

            // Make a Bitmap to store the result.
            Bitmap output = new Bitmap(FMX * tilesize, FMY * tilesize);
            int[] bitmapData = new int[output.Height * output.Width];

            if (result.Observed != null)
            {
                for (int x = 0; x < FMX; x++) for (int y = 0; y < FMY; y++)
                    {
                        Color[] tile = tiles[result.Observed[x + y * FMX]];
                        for (int yt = 0; yt < tilesize; yt++) for (int xt = 0; xt < tilesize; xt++)
                            {
                                Color c = tile[xt + yt * tilesize];
                                bitmapData[x * tilesize + xt + (y * tilesize + yt) * FMX * tilesize] =
                                    unchecked((int)0xff000000 | (c.R << 16) | (c.G << 8) | c.B);
                            }
                    }
            }
            else
            {
                System.Console.WriteLine("> NULL");
                for (int x = 0; x < FMX; x++) for (int y = 0; y < FMY; y++)
                    {
                        bool[] a = result.Wave[x + y * FMX];
                        int amount = (from b in a where b select 1).Sum();
                        double lambda = 1.0 / (from t in Enumerable.Range(0, result.T) where a[t] select result.Weights[t]).Sum();

                        for (int yt = 0; yt < tilesize; yt++) for (int xt = 0; xt < tilesize; xt++)
                            {
                                if (result.Black && amount == result.T) bitmapData[x * tilesize + xt + (y * tilesize + yt) * FMX * tilesize] = unchecked((int)0xff000000);
                                else
                                {
                                    double r = 0, g = 0, b = 0;
                                    for (int t = 0; t < result.T; t++) if (a[t])
                                        {
                                            Color c = tiles[t][xt + yt * tilesize];
                                            r += (double)c.R * result.Weights[t] * lambda;
                                            g += (double)c.G * result.Weights[t] * lambda;
                                            b += (double)c.B * result.Weights[t] * lambda;
                                        }

                                    bitmapData[x * tilesize + xt + (y * tilesize + yt) * FMX * tilesize] =
                                        unchecked((int)0xff000000 | ((int)r << 16) | ((int)g << 8) | (int)b);
                                }
                            }
                    }
            }

            var bits = output.LockBits(new Rectangle(0, 0, output.Width, output.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            System.Runtime.InteropServices.Marshal.Copy(bitmapData, 0, bits.Scan0, bitmapData.Length);
            output.UnlockBits(bits);

            // Save multiple results.
            string filename = $"{argument.PathOutput}/{argument.SetName}";
            if (File.Exists(filename + ".png"))
            {
                int i = 1;
                while (File.Exists($"{filename}({i}).png")) ++i;
                filename = $"{filename}({i})";
            }
            output.Save($"{filename}.png");
        }

        private Argument argument;
        private Result result;
    }
}
