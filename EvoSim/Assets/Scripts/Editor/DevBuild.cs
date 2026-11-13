using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Evo.Editor
{
    /// <summary>A quick macOS player for checking the interface on the desktop (tools/shots.sh). Not part of the release.</summary>
    public static class DevBuild
    {
        public static void BuildMac()
        {
            ProjectSetup.Apply();
            string root = Directory.GetParent(Application.dataPath).Parent.FullName;
            string output = Path.Combine(root, "Builds", "MacDev", "Divergence.app");
            var options = new BuildPlayerOptions
            {
                scenes = new[] { ProjectSetup.ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.None,
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            Debug.Log("DevBuild: " + report.summary.result);
            if (report.summary.result != BuildResult.Succeeded) EditorApplication.Exit(1);
        }
    }
}
