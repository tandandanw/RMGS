using System;
using System.Collections.Generic;

using RMGS.Args;

namespace RMGS.Core
{
    public class TileModel : Model
    {
        public TileModel(Argument argument, bool periodic, bool black) : base(argument.Width, argument.Height, periodic)
        {
            Result = new Result(black, periodic, argument.ChunkAmount);

            var tilenames = new List<(string, int)>();
            var tempStationary = new List<double>();

            List<int[]> action = new List<int[]>();
            Dictionary<string, int> firstOccurrence = new Dictionary<string, int>();

            foreach (var pattern in argument.Patterns)
            {
                string tilename = pattern.Name;

                Func<int, int> a, b;
                int cardinality;

                Symmetry sym = pattern.Symmetry;
                if (sym == Symmetry.L)
                {
                    cardinality = 4;
                    a = i => (i + 1) % 4;
                    b = i => i % 2 == 0 ? i + 1 : i - 1;
                }
                else if (sym == Symmetry.T)
                {
                    cardinality = 4;
                    a = i => (i + 1) % 4;
                    b = i => i % 2 == 0 ? i : 4 - i;
                }
                else if (sym == Symmetry.I)
                {
                    cardinality = 2;
                    a = i => 1 - i;
                    b = i => i;
                }
                /* else if (sym == '\\')
                 {
                    cardinality = 2;
                    a = i => 1 - i;
                    b = i => 1 - i;
                 }*/
                // else if (sym == Symmetry.X)
                else
                {
                    cardinality = 1;
                    a = i => i;
                    b = i => i;
                }

                T = action.Count;
                firstOccurrence.Add(tilename, T);

                int[][] map = new int[cardinality][];
                for (int t = 0; t < cardinality; t++)
                {
                    map[t] = new int[8];

                    map[t][0] = t;
                    map[t][1] = a(t);
                    map[t][2] = a(a(t));
                    map[t][3] = a(a(a(t)));
                    map[t][4] = b(t);
                    map[t][5] = b(a(t));
                    map[t][6] = b(a(a(t)));
                    map[t][7] = b(a(a(a(t))));

                    for (int s = 0; s < 8; s++) map[t][s] += T;

                    action.Add(map[t]);
                }

                tilenames.Add((tilename, cardinality));

                for (int t = 0; t < cardinality; t++) tempStationary.Add(pattern.Weight);
            }

            T = action.Count;
            weights = tempStationary.ToArray();
            Result.Patterns = tilenames.ToArray();

            propagator = new int[4][][];
            var tempPropagator = new bool[4][][];
            for (int d = 0; d < 4; d++)
            {
                tempPropagator[d] = new bool[T][];
                propagator[d] = new int[T][];
                for (int t = 0; t < T; t++) tempPropagator[d][t] = new bool[T];
            }

            foreach (var constraint in argument.Constraints)
            {
                if (constraint is PresetConstraint)
                {
                    var presetConstraint = (PresetConstraint)constraint;
                    int L = action[firstOccurrence[presetConstraint.Left.Item1]][presetConstraint.Left.Item2], D = action[L][1];
                    int R = action[firstOccurrence[presetConstraint.Right.Item1]][presetConstraint.Right.Item2], U = action[R][1];

                    tempPropagator[0][R][L] = true;
                    tempPropagator[0][action[R][6]][action[L][6]] = true;
                    tempPropagator[0][action[L][4]][action[R][4]] = true;
                    tempPropagator[0][action[L][2]][action[R][2]] = true;

                    tempPropagator[1][U][D] = true;
                    tempPropagator[1][action[D][6]][action[U][6]] = true;
                    tempPropagator[1][action[U][4]][action[D][4]] = true;
                    tempPropagator[1][action[D][2]][action[U][2]] = true;
                }
            }

            for (int t2 = 0; t2 < T; t2++) for (int t1 = 0; t1 < T; t1++)
                {
                    tempPropagator[2][t2][t1] = tempPropagator[0][t1][t2];
                    tempPropagator[3][t2][t1] = tempPropagator[1][t1][t2];
                }

            List<int>[][] sparsePropagator = new List<int>[4][];
            for (int d = 0; d < 4; d++)
            {
                sparsePropagator[d] = new List<int>[T];
                for (int t = 0; t < T; t++) sparsePropagator[d][t] = new List<int>();
            }

            for (int d = 0; d < 4; d++) for (int t1 = 0; t1 < T; t1++)
                {
                    List<int> sp = sparsePropagator[d][t1];
                    bool[] tp = tempPropagator[d][t1];

                    for (int t2 = 0; t2 < T; t2++) if (tp[t2]) sp.Add(t2);

                    int ST = sp.Count;
                    propagator[d][t1] = new int[ST];
                    for (int st = 0; st < ST; st++) propagator[d][t1][st] = sp[st];
                }

            base.argument = argument;
        }

        protected override bool OnBoundary(int x, int y) => !periodic && (x < 0 || y < 0 || x >= FMX || y >= FMY);
    }
}
