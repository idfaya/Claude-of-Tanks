using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankBakedPresentationCatalog
    {
        private const string ResourceRoot =
            "Generated/TankPresentation/";

        private static readonly string[] HiddenNames =
        {
            "Hull",
            "UpperHull",
            "Turret",
            "Gun",
            "Armor-upper_glacis",
            "Armor-lower_front",
            "Armor-hull_side_upper_R",
            "Armor-hull_side_upper_L",
            "Armor-hull_side_lower_R",
            "Armor-hull_side_lower_L",
            "Armor-skirt_rubber_R",
            "Armor-skirt_rubber_L",
            "Armor-track_R",
            "Armor-track_L",
            "Armor-slat_cage",
            "Armor-hull_rear",
            "Armor-hull_roof",
            "Armor-turret_cheek_R",
            "Armor-turret_cheek_L",
            "Armor-mantlet",
            "Armor-turret_side_R",
            "Armor-turret_side_L",
            "Armor-turret_bustle",
            "Armor-turret_roof",
            "Armor-turret_cupola_01_front",
            "Armor-turret_cupola_01_rear",
            "Armor-turret_cupola_01_right",
            "Armor-turret_cupola_01_left",
            "Armor-turret_cupola_01_top"
        };

        private static readonly string[] HiddenPrefixes =
        {
            "Soviet-",
            "Painted-Soviet-",
            "RunningGear-",
            "RoadWheel-",
            "WheelHub-",
            "SuspensionArm-",
            "SuspensionJoint-",
            "ReturnRoller-",
            "Sprocket-",
            "Idler-",
            "TrackLinks-"
        };

        public static bool TryBuild(
            string id,
            Transform root,
            Transform turret)
        {
            GameObject prefab =
                Resources.Load<GameObject>(ResourceRoot + id);
            if (prefab == null) return false;

            HideTargets(root);
            GameObject marker =
                new GameObject("T90-BakedPresentationPrefab");
            marker.transform.SetParent(root, false);

            GameObject instance =
                UnityEngine.Object.Instantiate(prefab);
            instance.name = "T90-BakedPresentationInstance";
            instance.transform.SetParent(root, false);
            CloneMaterials(instance.transform);
            MoveChildren(
                instance.transform.Find("Turret"),
                turret);
            MoveGunChildren(
                instance.transform.Find("GunFittings"),
                turret);
            return true;
        }

        private static void CloneMaterials(Transform instance)
        {
            Renderer[] renderers =
                instance.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Material source = renderers[i].sharedMaterial;
                if (source != null)
                    renderers[i].sharedMaterial =
                        new Material(source);
            }
        }

        private static void MoveGunChildren(
            Transform source,
            Transform turret)
        {
            if (source == null) return;
            Transform gun = turret.Find("Gun");
            if (gun == null) return;
            Transform fittings =
                TankDetailGeometry.GunFittingsRoot(
                    gun,
                    "T90-BakedGunFittings");
            MoveChildren(source, fittings);
        }

        private static void MoveChildren(
            Transform source,
            Transform destination)
        {
            if (source == null || destination == null) return;
            while (source.childCount > 0)
            {
                Transform child = source.GetChild(0);
                child.SetParent(destination, false);
            }
        }

        private static void HideTargets(Transform root)
        {
            Transform[] parts =
                root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < parts.Length; i++)
            {
                if (ShouldHide(parts[i].name))
                    Hide(parts[i]);
            }
        }

        private static bool ShouldHide(string name)
        {
            for (int i = 0; i < HiddenNames.Length; i++)
            {
                if (string.Equals(
                    name,
                    HiddenNames[i],
                    StringComparison.Ordinal))
                    return true;
            }
            for (int i = 0; i < HiddenPrefixes.Length; i++)
            {
                if (name.StartsWith(
                    HiddenPrefixes[i],
                    StringComparison.Ordinal))
                    return true;
            }
            return false;
        }

        private static void Hide(Transform part)
        {
            Renderer renderer =
                part == null
                    ? null
                    : part.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;
        }
    }
}
