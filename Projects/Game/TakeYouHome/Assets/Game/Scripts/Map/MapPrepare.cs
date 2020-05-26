using UnityEngine;
using System.Xml.Linq;

using RMGS.Args;
using RMGS.Import;
using RMGS.Export;
using RMGS.Core;
using System.IO;

/// <summary>
/// Generating map data which contains the result of wfc algorithm.
/// </summary>
public class MapPrepare
{
    public TinyResult Result { get => result; }
    public MapPrepare(int level, in TextAsset constraintsText)
    {
        this.level = level;
        this.constraintsText = constraintsText;
    }

    public int Start()
    {
        if (0 == Import())
        {
            return Prepare();
        }
        return -1;
    }

    private int Import()
    {
        if (constraintsText == null)
        {
            Debug.Log("> CONSTRAINTS TEXT IS NULL.");
            return -1;
        }

        var textReader = new StringReader(constraintsText.text);
        XElement xroot = XElement.Load(textReader);

        // assetBundle.Unload(false);

        importer = new Importer(xroot, Platform.Game);
        if (importer == null)
        {
            Debug.Log("> IMPORTER IS NULL.");
            return -2;
        }
        else
        {
            argument = importer.Import();
            if (argument == null)
            {
                Debug.Log("> IMPORTING FAILED.");
                return -3;
            }
            return 0;
        }
    }

    /// <summary>
    /// Run RMGS.
    /// </summary>
    private int Prepare()
    {
        // Setting arguments.
        argument.ChunkAmount = level;
        if (level == 1) argument.Width = argument.Height = 20;
        else argument.Width = argument.Height = level % 2 == 0 ? 20 + 10 * ((level - 1) / 2) + 6 : 20 + 10 * (level / 2);

        // Run RMGS.
        var model = new TileModel(argument, false, false);
        var random = new System.Random();
        // TODO: How many times to run?
        for (int i = 0; i < 10; ++i)
        {
            int seed = random.Next();
            bool finished = model.Run(seed, 0);
            if (finished)
            {
                Debug.Log("> GENERATION DONE.");
                var exporter = new Exporter(importer.Argument, model.Result);
                result = exporter.ExportToUnity();
                break;
            }
            else Debug.Log("> CONTRADICTION.");
        }

        return 0;
    }

    private int level;
    private TextAsset constraintsText;

    private Importer importer;
    private Argument argument;

    private TinyResult result;
}
