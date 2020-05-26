using UnityEngine;
using UnityEditor;

public class RMGSWindow : EditorWindow
{
    [MenuItem("Window/RMGS Settings %g")]

    static void Init()
    {
        RMGSWindow window = (RMGSWindow)EditorWindow.GetWindow(typeof(RMGSWindow));
        window.titleContent.text = "RMGS Settings";
        window.ShowUtility();
    }

    void OnGUI()
    {
        GUILayout.Label("Random Map Generation System GUI", EditorStyles.boldLabel);
        GUILayout.Label("Pattern Path", GUILayout.ExpandWidth(false));
        GUILayout.BeginHorizontal();
        GUILayout.TextField(RMGSData.instance().PathPattern);
        if (GUILayout.Button("Browse", GUILayout.ExpandWidth(false)))
        {
            RMGSData.instance().PathPattern = EditorUtility.OpenFolderPanel("Path to pattern folder", RMGSData.instance().PathPattern, Application.dataPath);
        }
        GUILayout.EndHorizontal();

        GUILayout.Label("Constraint File Path", GUILayout.ExpandWidth(false));
        GUILayout.BeginHorizontal();
        GUILayout.TextField(RMGSData.instance().PathConstraint);
        if (GUILayout.Button("Browse", GUILayout.ExpandWidth(false)))
        {
            RMGSData.instance().PathConstraint = EditorUtility.OpenFilePanel("Path to constraint file", RMGSData.instance().PathConstraint, "xml");
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.HelpBox("These are RMGS-GUI global settings", MessageType.Info);
    }
}
