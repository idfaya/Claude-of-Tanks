using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class CSharpRuntimeBoundaryTests
    {
        private static readonly string[] RuntimeRoots =
        {
            "Runtime",
            "Simulation",
            "Network",
            "Server",
            "WebRTC",
            "Tests/PlayMode"
        };

        [Test]
        public void RuntimeAndPlayModeTestsDoNotLaunchNodeOrTypeScript()
        {
            string projectRoot =
                Directory.GetParent(Application.dataPath).FullName;
            string sourceRoot =
                Path.Combine(projectRoot, "Assets/ClaudeOfTanks");
            string[] forbidden =
            {
                "System.Diagnostics." + "Process",
                "Process" + "StartInfo",
                "server/signalingServer" + ".ts",
                "ResolveNode" + "Executable"
            };
            List<string> violations = new List<string>();
            for (int rootIndex = 0;
                rootIndex < RuntimeRoots.Length;
                rootIndex++)
            {
                string root =
                    Path.Combine(sourceRoot, RuntimeRoots[rootIndex]);
                string[] files = Directory.GetFiles(
                    root,
                    "*.cs",
                    SearchOption.AllDirectories);
                for (int fileIndex = 0;
                    fileIndex < files.Length;
                    fileIndex++)
                {
                    string text = File.ReadAllText(files[fileIndex]);
                    for (int tokenIndex = 0;
                        tokenIndex < forbidden.Length;
                        tokenIndex++)
                    {
                        if (!text.Contains(forbidden[tokenIndex]))
                            continue;
                        violations.Add(
                            Relative(projectRoot, files[fileIndex]) +
                            ": " +
                            forbidden[tokenIndex]);
                    }
                }
            }
            Assert.That(
                violations,
                Is.Empty,
                "Unity runtime/test code must remain C#-native. " +
                "Vehicle model TS is restricted to offline presentation bake.");
        }

        [Test]
        public void UnityAssetsContainNoExecutableTypeScriptSources()
        {
            string projectRoot =
                Directory.GetParent(Application.dataPath).FullName;
            string sourceRoot =
                Path.Combine(projectRoot, "Assets/ClaudeOfTanks");
            List<string> scripts = new List<string>();
            AddFiles(sourceRoot, "*.ts", projectRoot, scripts);
            AddFiles(sourceRoot, "*.js", projectRoot, scripts);
            AddFiles(sourceRoot, "*.mjs", projectRoot, scripts);
            Assert.That(
                scripts,
                Is.Empty,
                "Executable JavaScript/TypeScript must not ship inside Unity assets.");
        }

        [Test]
        public void ContentCatalogIsCanonicalUnityData()
        {
            string projectRoot =
                Directory.GetParent(Application.dataPath).FullName;
            Assert.That(
                File.Exists(Path.Combine(
                    projectRoot,
                    "Assets/ClaudeOfTanks/Resources/Content/" +
                    "content-catalog.json")),
                Is.True);
            Assert.That(
                File.Exists(Path.Combine(
                    projectRoot,
                    "Assets/ClaudeOfTanks/Resources/Generated/" +
                    "content-catalog.json")),
                Is.False);
            Assert.That(
                File.Exists(Path.Combine(
                    projectRoot,
                    "tools/gen-unity-content.mjs")),
                Is.False);
            Assert.That(
                File.Exists(Path.Combine(
                    projectRoot,
                    "tools/gen-unity-presentation-schemas.mjs")),
                Is.False);
            Assert.That(
                Directory.Exists(Path.Combine(
                    projectRoot,
                    "Assets/ClaudeOfTanks/Generated/PresentationSource")),
                Is.False);
            Assert.That(
                Directory.Exists(Path.Combine(
                    projectRoot,
                    "Assets/ClaudeOfTanks/Resources/Generated/" +
                    "TankPresentation")),
                Is.False);
        }

        private static void AddFiles(
            string root,
            string pattern,
            string projectRoot,
            List<string> output)
        {
            string[] files = Directory.GetFiles(
                root,
                pattern,
                SearchOption.AllDirectories);
            for (int index = 0; index < files.Length; index++)
                output.Add(Relative(projectRoot, files[index]));
        }

        private static string Relative(
            string root,
            string path)
        {
            return path.Substring(root.Length + 1)
                .Replace('\\', '/');
        }
    }
}
