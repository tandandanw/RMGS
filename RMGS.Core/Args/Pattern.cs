using System;
using System.Collections.Generic;
using System.Text;

namespace RMGS.Args
{
    [Flags]
    public enum Symmetry { L = 1, T = 2, I = 3, X = 4 }

    public class Pattern
    {
        public string Name { get; set; }
        public double Weight { get; set; }
        public Symmetry Symmetry { get; set; }

        public Pattern(string name, double weight, Symmetry symmetry)
        {
            Name = name;
            Weight = weight;
            Symmetry = symmetry;
        }
    }
}
