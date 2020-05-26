using System;
using System.Collections.Generic;
using System.Xml.Linq;

using RMGS.Args;
using RMGS.Core;

namespace RMGS.Import
{
    public class Importer
    {
        public Argument Argument { get; set; }

        public Importer(XElement xroot, Platform platform)
        {
            this.xroot = xroot;
            Argument = new Argument(platform);
        }

        public Importer(string pathPattern, string pathConstraint, Platform platform)
        {
            Argument = new Argument(pathPattern, pathConstraint, platform);
        }

        public Importer(string pathPattern, string pathConstraint, string pathOutput, bool isSlience, Platform platform)
        {
            Argument = new Argument(pathPattern, pathConstraint, pathOutput, isSlience, platform);
        }

        public Argument Import()
        {
            // Read arguments.
            if (xroot == null)
            {
                try
                {
                    xroot = XDocument.Load(Argument.PathConstraint).Root;
                }
                catch (System.IO.FileNotFoundException ex)
                {
                    Console.WriteLine("> CONSTRAINT FILE NOT FOUND. {0}", ex.Message);
                    throw;
                }
            }

            Argument.SetName = xroot.Get("setName", "rmgs");
            Argument.PatternSize = xroot.Get("patternsize", 16);
            Argument.Width = xroot.Get("width", 10);
            Argument.Height = xroot.Get("height", 10);
            Argument.ChunkAmount = xroot.Get("chunkAmount", 8);

            // Read patterns.
            Symmetry str2symmetry(string s)
            {
                switch (s)
                {
                    case "L": return Symmetry.L;
                    case "T": return Symmetry.T;
                    case "I": return Symmetry.I;
                    case "X": return Symmetry.X;
                    default: return Symmetry.X;
                }
            };

            var patterns = new List<Pattern>();
            foreach (XElement xtile in xroot.Element("patterns").Elements("pattern"))
            {
                patterns.Add(new Pattern(
                    xtile.Get<string>("name"),
                    xtile.Get<double>("weight"),
                    str2symmetry(xtile.Get<string>("symmetry"))
                ));
            }
            Argument.Patterns = patterns.ToArray();

            // Read constraint.
            Argument.Constraints = new List<Constraint>();
            foreach (XElement xtile in xroot.Element("constraints").Elements("constraint"))
            {
                string[] left = xtile.Get<string>("left").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string[] right = xtile.Get<string>("right").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                Argument.Constraints.Add(new PresetConstraint(
                    (left[0], (left.Length == 1 ? 0 : int.Parse(left[1]))),
                    (right[0], (right.Length == 1 ? 0 : int.Parse(right[1])))
                ));
            }

            return Argument;
        }

        public XElement xroot;
    }
}
