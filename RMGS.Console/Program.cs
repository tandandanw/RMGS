using System;
using System.Reflection;

using RMGS.Core;
using RMGS.Args;
using RMGS.Import;
using RMGS.Export;

namespace RMGS.Console
{
    class Program
    {
        private static readonly int vmajor = 0, vminor = 0, vpatch = 0;

        private static string pwd = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        private static string pathPattern = pwd, pathConstraint = pwd, pathOutput = pwd;
        private static bool isSlience = true;

        static int Main(string[] args)
        {
            System.Console.Clear();
            System.Console.WriteLine("Ramdom Map Generation System v{0}.{1}.{2}", vmajor, vminor, vpatch);

            // Print RMGS information.
            if (args.Length == 0)
            {
                System.Console.WriteLine("RMGS is consist of RMGS-Core, RMGS-Console, RMGS-GUI(for Unity).");
                System.Console.WriteLine("\nRMGS Console Usage\n");
                System.Console.WriteLine("\trmgs [options] <path> ... ");
                System.Console.WriteLine("\nGeneral\n");
                System.Console.WriteLine("\trmgs -p <path-to-pattern-folder> -c <path-to-constraint-file>\n" +
                                         "\trmgs -p <path-to-pattern-folder> -c <path-to-constraint-file> -o <path-to-output-file>");
                System.Console.WriteLine("\nMore details during executing, use option -d \n");
                System.Console.WriteLine("\trmgs -p <path-to-pattern-folder> -c <path-to-constraint-file> -d\n" +
                                         "\trmgs -p <path-to-pattern-folder> -c <path-to-constraint-file> -o <path-to-output-file> -d");
            }
            // Read paths of argument.
            else
            {
                for (var i = 0; i < args.Length; ++i)
                {
                    if (args[i][0] != '-') { System.Console.WriteLine("rmgs error : invalid option(s)."); return -1; };
                    switch (args[i][1])
                    {
                        case 'p': pathPattern = args[++i]; break;
                        case 'c': pathConstraint = args[++i]; break;
                        case 'o': pathOutput = args[++i]; break;
                        case 'd': isSlience = false; break;
                    }
                }
                // Run RMGS to get the result.
                var importer = new Importer(pathPattern, pathConstraint, pathOutput, isSlience, Platform.RMGSConsole);
                Argument argument = importer.Import();
                var model = new TileModel(argument, false, false);
                var random = new Random();
                for (int k = 0; k < 3; k++)
                {
                    System.Console.Write("> ");
                    int seed = random.Next();
                    bool finished = model.Run(seed, 0);
                    if (finished)
                    {
                        System.Console.WriteLine("GENERATION DONE.");
                        var exporter = new Exporter(importer.Argument, model.Result);
                        (Argument, Result) res = exporter.ExportToConsole();
                        var consoleExporter = new ConsoleExporter(res.Item1, res.Item2);
                        consoleExporter.Export();
                        break;
                    }
                    else System.Console.WriteLine("CONTRADICTION.");
                }
            }
            return 0;
        }
    }

}
