using System;
using System.Linq;

using System.Xml.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using RMGS.Args;

namespace RMGS.Core
{
    public abstract class Model
    {
        protected Model() { }

        protected Model(int weight, int height, bool periodic)
        {
            FMX = weight;
            FMY = height;
            this.periodic = periodic;
        }
        public Result Result { get; set; }
        protected Argument argument;

        protected bool[][] wave;

        protected int[][][] propagator;
        int[][][] compatible;
        protected int[] observed;

        (int, int)[] stack;
        int stacksize;

        protected Random random;
        protected int FMX, FMY, T;
        protected bool periodic;

        protected double[] weights;
        double[] weightLogWeights;

        int[] sumsOfOnes;
        double sumOfWeights, sumOfWeightLogWeights, startingEntropy;
        double[] sumsOfWeights, sumsOfWeightLogWeights, entropies;

        void Init()
        {
            wave = new bool[FMX * FMY][];
            compatible = new int[wave.Length][][];
            for (int i = 0; i < wave.Length; i++)
            {
                wave[i] = new bool[T];
                compatible[i] = new int[T][];
                for (int t = 0; t < T; t++) compatible[i][t] = new int[4];
            }

            weightLogWeights = new double[T];
            sumOfWeights = 0;
            sumOfWeightLogWeights = 0;

            for (int t = 0; t < T; t++)
            {
                weightLogWeights[t] = weights[t] * Math.Log(weights[t]);
                sumOfWeights += weights[t];
                sumOfWeightLogWeights += weightLogWeights[t];
            }

            startingEntropy = Math.Log(sumOfWeights) - sumOfWeightLogWeights / sumOfWeights;

            sumsOfOnes = new int[FMX * FMY];
            sumsOfWeights = new double[FMX * FMY];
            sumsOfWeightLogWeights = new double[FMX * FMY];
            entropies = new double[FMX * FMY];

            stack = new (int, int)[wave.Length * T];
            stacksize = 0;
        }

        bool? Observe()
        {
            double min = 1E+3;
            int argmin = -1;

            for (int i = 0; i < wave.Length; i++)
            {
                if (OnBoundary(i % FMX, i / FMX)) continue;

                int amount = sumsOfOnes[i];
                if (amount == 0) return false;

                double entropy = entropies[i];
                if (amount > 1 && entropy <= min)
                {
                    double noise = 1E-6 * random.NextDouble();
                    if (entropy + noise < min)
                    {
                        min = entropy + noise;
                        argmin = i;
                    }
                }
            }

            if (argmin == -1)
            {
                observed = new int[FMX * FMY];
                for (int i = 0; i < wave.Length; i++) for (int t = 0; t < T; t++) if (wave[i][t]) { observed[i] = t; break; }
                return true;
            }

            double[] distribution = new double[T];
            for (int t = 0; t < T; t++) distribution[t] = wave[argmin][t] ? weights[t] : 0;
            int r = distribution.Random(random.NextDouble());

            bool[] w = wave[argmin];
            for (int t = 0; t < T; t++) if (w[t] != (t == r)) Ban(argmin, t);

            return null;
        }

        protected void Propagate()
        {
            while (stacksize > 0)
            {
                var e1 = stack[stacksize - 1];
                stacksize--;

                int i1 = e1.Item1;
                int x1 = i1 % FMX, y1 = i1 / FMX;

                for (int d = 0; d < 4; d++)
                {
                    int dx = DX[d], dy = DY[d];
                    int x2 = x1 + dx, y2 = y1 + dy;
                    if (OnBoundary(x2, y2)) continue;

                    if (x2 < 0) x2 += FMX;
                    else if (x2 >= FMX) x2 -= FMX;
                    if (y2 < 0) y2 += FMY;
                    else if (y2 >= FMY) y2 -= FMY;

                    int i2 = x2 + y2 * FMX;
                    int[] p = propagator[d][e1.Item2];
                    int[][] compat = compatible[i2];

                    for (int l = 0; l < p.Length; l++)
                    {
                        int t2 = p[l];
                        int[] comp = compat[t2];

                        comp[d]--;
                        if (comp[d] == 0) Ban(i2, t2);
                    }
                }
            }
        }

        public bool Run(int seed, int limit)
        {
            if (wave == null) Init();

            Clear();
            random = new Random(seed);

            // Constrain.
            for (int i = 0; i < argument.Constraints.Count; ++i)
            {
                if (argument.Constraints[i] is RealtimeConstraint)
                {
                    RealtimeConstraint rc = (RealtimeConstraint)argument.Constraints[i];
                    bool[] w = wave[rc.BanPosition];
                    for (int t = 0; t < T; t++) if (w[t] != (t == 6)) Ban(rc.BanPosition, t);
                    Propagate();
                }
            }

            // Observe-Propagate Process.
            for (int l = 0; l < limit || limit == 0; l++)
            {
                bool? result = Observe();
                if (result != null)
                {
                    Result.Fill(wave, observed, weights);
                    return (bool)result;
                }
                Propagate();
            }

            // To send the result out.
            Result.Fill(wave, observed, weights);
            return true;
        }

        protected void Ban(int i, int t)

        {
            wave[i][t] = false;

            int[] comp = compatible[i][t];
            for (int d = 0; d < 4; d++) comp[d] = 0;
            stack[stacksize] = (i, t);
            stacksize++;

            sumsOfOnes[i] -= 1;
            sumsOfWeights[i] -= weights[t];
            sumsOfWeightLogWeights[i] -= weightLogWeights[t];

            double sum = sumsOfWeights[i];
            entropies[i] = Math.Log(sum) - sumsOfWeightLogWeights[i] / sum;
        }

        protected virtual void Clear()
        {
            for (int i = 0; i < wave.Length; i++)
            {
                for (int t = 0; t < T; t++)
                {
                    wave[i][t] = true;
                    for (int d = 0; d < 4; d++) compatible[i][t][d] = propagator[opposite[d]][t].Length;
                }

                sumsOfOnes[i] = weights.Length;
                sumsOfWeights[i] = sumOfWeights;
                sumsOfWeightLogWeights[i] = sumOfWeightLogWeights;
                entropies[i] = startingEntropy;
            }
        }

        protected abstract bool OnBoundary(int x, int y);

        protected static int[] DX = { -1, 0, 1, 0 };
        protected static int[] DY = { 0, 1, 0, -1 };
        static int[] opposite = { 2, 3, 0, 1 };
    }
}
