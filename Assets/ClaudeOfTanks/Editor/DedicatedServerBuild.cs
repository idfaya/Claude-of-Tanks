using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace ClaudeOfTanks.Editor
{
    public static class DedicatedServerBuild
    {
        [MenuItem("Claude of Tanks/Build Dedicated Server/macOS")]
        public static void BuildMacOS()
        {
            Build(
                BuildTarget.StandaloneOSX,
                "Build/Server/macOS/ClaudeOfTanksServer.app");
        }

        [MenuItem("Claude of Tanks/Build Headless Player/macOS")]
        public static void BuildHeadlessMacOS()
        {
            BuildHeadlessPlayer(
                BuildTarget.StandaloneOSX,
                "Build/Server/macOS/ClaudeOfTanksHeadless.app");
        }

        public static void BuildFromCommandLine()
        {
            string[] args = Environment.GetCommandLineArgs();
            string targetName = Value(args, "--cot-build-target") ?? "linux";
            string output = Value(args, "--cot-build-output");
            BuildTarget target;
            if (string.Equals(targetName, "linux", StringComparison.OrdinalIgnoreCase))
            {
                target = BuildTarget.StandaloneLinux64;
                if (string.IsNullOrEmpty(output))
                    output = "Build/Server/Linux/ClaudeOfTanksServer";
            }
            else if (string.Equals(targetName, "macos", StringComparison.OrdinalIgnoreCase))
            {
                target = BuildTarget.StandaloneOSX;
                if (string.IsNullOrEmpty(output))
                    output = "Build/Server/macOS/ClaudeOfTanksServer.app";
            }
            else
            {
                throw new ArgumentException("Unsupported dedicated build target: " + targetName);
            }
            if (HasFlag(args, "--cot-build-player"))
                BuildHeadlessPlayer(target, output);
            else
                Build(target, output);
        }

        public static BuildReport Build(BuildTarget target, string output)
        {
            return BuildInternal(
                target,
                output,
                StandaloneBuildSubtarget.Server,
                BuildOptions.StrictMode);
        }

        public static BuildReport BuildHeadlessPlayer(BuildTarget target, string output)
        {
            return BuildInternal(
                target,
                output,
                StandaloneBuildSubtarget.Player,
                BuildOptions.StrictMode);
        }

        private static BuildReport BuildInternal(
            BuildTarget target,
            string output,
            StandaloneBuildSubtarget subtarget,
            BuildOptions buildOptions)
        {
            if (!BuildPipeline.IsBuildTargetSupported(
                    BuildPipeline.GetBuildTargetGroup(target),
                    target))
            {
                throw new NotSupportedException(
                    "Unity build support is not installed for " + target + ".");
            }
            if (string.IsNullOrWhiteSpace(output))
                throw new ArgumentException("Dedicated build output is required.", nameof(output));
            string[] scenes = EnabledScenes();
            if (scenes.Length == 0)
                throw new InvalidOperationException("No enabled scenes are configured.");
            string fullOutput = Path.GetFullPath(output);
            string directory = Path.GetDirectoryName(fullOutput);
            Directory.CreateDirectory(directory);
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = fullOutput,
                target = target,
                subtarget = (int)subtarget,
                options = buildOptions
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Dedicated server build failed: " + report.summary.result);
            }
            return report;
        }

        private static string[] EnabledScenes()
        {
            List<string> scenes = new List<string>();
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                EditorBuildSettingsScene scene = EditorBuildSettings.scenes[i];
                if (scene.enabled && !string.IsNullOrEmpty(scene.path))
                    scenes.Add(scene.path);
            }
            return scenes.ToArray();
        }

        private static string Value(string[] args, string name)
        {
            string prefix = name + "=";
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith(prefix, StringComparison.Ordinal))
                    return args[i].Substring(prefix.Length);
                if (args[i] == name && i + 1 < args.Length)
                    return args[i + 1];
            }
            return null;
        }

        private static bool HasFlag(string[] args, string name)
        {
            for (int i = 0; i < args.Length; i++)
                if (args[i] == name) return true;
            return false;
        }
    }
}
