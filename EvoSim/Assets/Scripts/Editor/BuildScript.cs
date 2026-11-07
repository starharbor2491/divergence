using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Evo.Editor
{
    /// <summary>Builds the WebGL player into Builds/WebGL (tools/build-webgl.sh calls BuildWebGL).</summary>
    public static class BuildScript
    {
        [MenuItem("Divergence/Build WebGL")]
        public static void BuildWebGL()
        {
            ProjectSetup.Apply();
            string root = Directory.GetParent(Application.dataPath).Parent.FullName;
            string output = Path.Combine(root, "Builds", "WebGL");
            if (Directory.Exists(output)) Directory.Delete(output, true);
            var options = new BuildPlayerOptions
            {
                scenes = new[] { ProjectSetup.ScenePath },
                locationPathName = output,
                target = BuildTarget.WebGL,
                options = BuildOptions.None,
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            Debug.Log("BuildScript: " + summary.result + ", " + summary.totalSize / (1024 * 1024) + " MB, " + summary.totalTime);
            if (summary.result != BuildResult.Succeeded)
            {
                Debug.LogError("BuildScript: Build failed with " + summary.totalErrors + " errors");
                EditorApplication.Exit(1);
            }
        }
    }
}
