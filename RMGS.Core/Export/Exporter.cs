using RMGS.Args;

namespace RMGS.Export
{
    public class Exporter
    {
        public Exporter(Argument argument, Result result)
        {
            this.argument = argument;
            this.result = result;
        }

        public (Argument, Result) ExportToConsole()
        {
            if (argument.Platform != Platform.RMGSConsole) return (null, null);
            return (argument, result);
        }

        public TinyResult ExportToUnity()
        {
            if (!(argument.Platform == Platform.RMGSGUI || argument.Platform == Platform.Game)) return null;
            return new TinyResult(argument.Width, argument.Height, argument.PatternSize, argument.ChunkAmount, result.Observed, result.Patterns);
        }

        private Argument argument;
        private Result result;
    }
}
