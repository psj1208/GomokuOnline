using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

class MyEditorScript
{
    static string[] SCENES = FindEnabledEditorScenes();

    static void PerformBuild()
    {
        BuildPipeline.BuildPlayer(FindEnabledEditorScenes(), @"D:\\GomokuVersion\\Gomoku.exe", BuildTarget.StandaloneWindows, BuildOptions.None);
    }
    private static string[] FindEnabledEditorScenes()
    {
        List<string> EditorScenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;
            EditorScenes.Add(scene.path);
        }
        return EditorScenes.ToArray();
    }
}