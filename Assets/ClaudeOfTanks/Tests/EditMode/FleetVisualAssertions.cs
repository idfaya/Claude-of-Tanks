using System.Linq;
using ClaudeOfTanks.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    internal static class FleetVisualAssertions
    {
        public static int ValidPlateCount(
            VehicleDefinition definition)
        {
            return ValidPlateCount(
                    definition?.armor?.hullPlates) +
                ValidPlateCount(
                    definition?.armor?.turretPlates);
        }

        public static int ValidModulePartCount(
            VehicleDefinition definition,
            string moduleId = null)
        {
            ArmorModuleDefinition[] modules =
                definition?.armor?.modules;
            if (modules == null) return 0;
            int count = 0;
            for (int i = 0; i < modules.Length; i++)
            {
                ArmorModuleDefinition module = modules[i];
                if (module == null ||
                    (moduleId != null &&
                     module.module != moduleId) ||
                    module.parts == null)
                {
                    continue;
                }
                count += module.parts.Count(
                    ValidModulePart);
            }
            return count;
        }

        public static void AssertAuthoredRig(
            VehicleDefinition definition,
            Transform root,
            Transform turret)
        {
            if (definition?.armor?.turretPivot != null)
            {
                Assert.That(
                    Vector3.Distance(
                        turret.localPosition,
                        definition.armor.turretPivot
                            .ToVector3()),
                    Is.LessThan(0.0001f),
                    definition.id);
            }
            Transform gun = turret.Find("Gun");
            Assert.That(gun, Is.Not.Null, definition.id);
            if (definition?.armor?.gunBarrel != null &&
                definition.armor.gunBarrel.lengthM > 0.1f)
            {
                Assert.That(
                    gun.localScale.z,
                    Is.EqualTo(
                        definition.armor.gunBarrel.lengthM)
                        .Within(0.0001f),
                    definition.id);
                float diameter =
                    definition.armor.gunBarrel.radiusM * 2f;
                Assert.That(
                    gun.localScale.x,
                    Is.EqualTo(diameter)
                        .Within(0.0001f),
                    definition.id);
                Assert.That(
                    gun.localScale.y,
                    Is.EqualTo(diameter)
                        .Within(0.0001f),
                    definition.id);
                Vector3 pivot =
                    definition.armor.gunPivot.ToVector3();
                Assert.That(
                    gun.localPosition.z -
                        gun.localScale.z * 0.5f,
                    Is.EqualTo(pivot.z).Within(0.0001f),
                    definition.id);
            }
            Renderer[] renderers =
                root.GetComponentsInChildren<Renderer>();
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);
            Assert.That(
                bounds.size.x,
                Is.GreaterThan(1f),
                definition.id);
            Assert.That(
                bounds.size.z,
                Is.GreaterThan(2f),
                definition.id);
        }

        public static void AssertRenderableMeshes(
            string id,
            Transform root)
        {
            MeshFilter[] filters =
                root.GetComponentsInChildren<MeshFilter>();
            for (int i = 0; i < filters.Length; i++)
            {
                Mesh mesh = filters[i].sharedMesh;
                Assert.That(mesh, Is.Not.Null, id);
                Assert.That(
                    mesh.vertexCount,
                    Is.GreaterThanOrEqualTo(3),
                    id + "/" + filters[i].name);
                Assert.That(
                    mesh.bounds.size.sqrMagnitude,
                    Is.GreaterThan(0f),
                    id + "/" + filters[i].name);
            }
        }

        private static int ValidPlateCount(
            ArmorPlateDefinition[] plates)
        {
            return plates == null
                ? 0
                : plates.Count(plate =>
                    plate?.verts != null &&
                    plate.verts.Length >= 3);
        }

        private static bool ValidModulePart(
            ArmorModulePartDefinition part)
        {
            if (part?.min == null ||
                part.max == null ||
                part.min.Length < 3 ||
                part.max.Length < 3)
            {
                return false;
            }
            return part.max[0] - part.min[0] >
                    0.003f &&
                part.max[1] - part.min[1] >
                    0.003f &&
                part.max[2] - part.min[2] >
                    0.003f;
        }
    }
}
