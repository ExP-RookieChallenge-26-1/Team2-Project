using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class CommandLineAndroidBuild
{
    public static void BuildApk()
    {
        var outputPath = GetArgumentValue("-buildOutput");
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new InvalidOperationException("Missing -buildOutput argument.");
        }

        var scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            throw new InvalidOperationException("No enabled scenes found in EditorBuildSettings.");
        }

        EditorUserBuildSettings.buildAppBundle = false;
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

        if (string.IsNullOrWhiteSpace(PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android)))
        {
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.DefaultCompany.ExpDemo2");
        }

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException($"Android APK build failed: {report.summary.result}");
        }
    }

    private static string GetArgumentValue(string name)
    {
        var args = Environment.GetCommandLineArgs();
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == name)
            {
                return args[i + 1];
            }
        }

        return null;
    }
}
