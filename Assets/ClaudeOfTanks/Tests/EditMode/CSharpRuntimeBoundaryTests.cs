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
                "Vehicle model TS is reference-only during C# translation.");
        }

        [Test]
        public void BaseT90HasNoLegacyPrimitiveOwners()
        {
            string projectRoot =
                Directory.GetParent(Application.dataPath).FullName;
            string runtime = Path.Combine(
                projectRoot,
                "Assets/ClaudeOfTanks/Runtime");
            foreach (string legacy in new[]
                {
                    "TankT90HullDetails.cs",
                    "TankT90TurretDetails.cs",
                    "TankT90GunDetails.cs"
                })
            {
                Assert.That(
                    File.Exists(Path.Combine(runtime, legacy)),
                    Is.False,
                    legacy);
            }
            foreach (string active in new[]
                {
                    "TankT90PresentationSchema.cs",
                    "TankT90TranslatedExtras.cs",
                    "TankT90TranslatedGearPads.cs",
                    "TankT90TranslatedSuspension.cs",
                    "TankT90RevolutionDetails.cs",
                    "TankT90GunTranslation.cs"
                })
            {
                string text = File.ReadAllText(
                    Path.Combine(runtime, active));
                Assert.That(
                    text.Contains("TankDetailGeometry." + "Part("),
                    Is.False,
                    active);
                Assert.That(
                    text.Contains("PrimitiveType." + "Cylinder"),
                    Is.False,
                    active);
            }
        }

        [Test]
        public void MigratedT90AHullAndTurretAvoidLegacyPrimitives()
        {
            string projectRoot =
                Directory.GetParent(Application.dataPath).FullName;
            string runtime = Path.Combine(
                projectRoot,
                "Assets/ClaudeOfTanks/Runtime");
            foreach (string active in new[]
                {
                    "TankT90AHullDetails.cs",
                    "TankT90ATurretDetails.cs",
                    "TankT90ATurretRevolutionDetails.cs",
                    "TankT90AGunDetails.cs",
                    "TankFittingShapeFactory.cs",
                    "TankMachineGunSpec.cs",
                    "TankPintleMachineGunFactory.cs",
                    "TankLinkedTrackShapeFactory.cs",
                    "TankMudguardShapeFactory.cs",
                    "TankT90ABurlakFamilyDetails.cs",
                    "TankT90ABurlakTurretDetails.cs",
                    "TankT90ABurlakTurretEquipmentDetails.cs",
                    "TankT90ABurlakGunDetails.cs",
                    "TankT90SMFamilyDetails.cs",
                    "TankT90SMRunningGearDetails.cs",
                    "TankT90SMHullArmorDetails.cs",
                    "TankT90SMSternDetails.cs",
                    "TankT90SMTurretDetails.cs",
                    "TankT90SMTurretArmorDetails.cs",
                    "TankT90SMTurretEquipmentDetails.cs",
                    "TankT90SMBustleDetails.cs",
                    "TankT90SMGunDetails.cs",
                    "TankT90SMTurretFinalDetails.cs",
                    "TankT90MSFamilyDetails.cs",
                    "TankT90MSRunningGearDetails.cs",
                    "TankT90MSHullArmorDetails.cs",
                    "TankT90MSSternDetails.cs",
                    "TankT90MSTurretDetails.cs",
                    "TankT90MSTurretArmorDetails.cs",
                    "TankT90MSSurfaceArmorFactory.cs",
                    "TankT90MSBustleDetails.cs",
                    "TankT90AVladimirFamilyDetails.cs",
                    "TankT90AVladimirHullEquipmentDetails.cs",
                    "TankT90AVladimirTurretDetails.cs",
                    "TankT90AVladimirGunDetails.cs"
                })
            {
                string text = File.ReadAllText(
                    Path.Combine(runtime, active));
                Assert.That(
                    text.Contains("TankDetailGeometry." + "Part("),
                    Is.False,
                    active);
                Assert.That(
                    text.Contains("PrimitiveType." + "Cylinder"),
                    Is.False,
                    active);
            }
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
