using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal sealed class TankGunPitchRig
    {
        private static readonly string[] GunTokens =
        {
            "MainGun",
            "GunAssembly",
            "GunPlant",
            "GunTube",
            "GunBarrel",
            "CannonAssembly",
            "CannonTube",
            "BarrelPlant",
            "GunMantlet",
            "Mantlet"
        };

        private static readonly string[] ExcludedTokens =
        {
            "MachineGun",
            "Coax",
            "Smoke",
            "Roof",
            "Remote",
            "Rws"
        };

        private readonly Vector3 _pivot;
        private readonly PitchPart[] _parts;

        private TankGunPitchRig(
            Vector3 pivot,
            PitchPart[] parts)
        {
            _pivot = pivot;
            _parts = parts;
        }

        public static TankGunPitchRig Build(
            Transform turret,
            VehicleDefinition definition)
        {
            if (turret == null)
                throw new ArgumentNullException(
                    nameof(turret));
            Vector3 pivot =
                definition?.armor?.gunPivot !=
                    null
                    ? definition.armor
                        .gunPivot.ToVector3()
                    : Vector3.zero;
            HashSet<string> gunFollow =
                GunFollowNames(definition);
            Transform[] all =
                turret.GetComponentsInChildren<
                    Transform>(true);
            List<Transform> candidates =
                new List<Transform>();
            for (int i = 0; i < all.Length; i++)
            {
                Transform part = all[i];
                if (part == turret)
                    continue;
                if (IsCandidate(
                        part.name,
                        gunFollow))
                {
                    candidates.Add(part);
                }
            }
            List<PitchPart> parts =
                new List<PitchPart>();
            for (int i = 0;
                i < candidates.Count;
                i++)
            {
                Transform part = candidates[i];
                if (HasCandidateAncestor(
                        part,
                        turret,
                        gunFollow))
                {
                    continue;
                }
                parts.Add(
                    new PitchPart
                    {
                        Transform = part,
                        Position =
                            part.localPosition,
                        Rotation =
                            part.localRotation
                    });
            }
            return new TankGunPitchRig(
                pivot,
                parts.ToArray());
        }

        public void Apply(float pitchRad)
        {
            Quaternion rotation =
                Quaternion.Euler(
                    -pitchRad *
                        Mathf.Rad2Deg,
                    0f,
                    0f);
            for (int i = 0;
                i < _parts.Length;
                i++)
            {
                PitchPart part = _parts[i];
                if (part.Transform == null)
                    continue;
                part.Transform.localPosition =
                    _pivot +
                    rotation *
                    (part.Position -
                     _pivot);
                part.Transform.localRotation =
                    rotation *
                    part.Rotation;
            }
        }

        private static HashSet<string> GunFollowNames(
            VehicleDefinition definition)
        {
            HashSet<string> names =
                new HashSet<string>(
                    StringComparer.Ordinal);
            ArmorPlateDefinition[] plates =
                definition?.armor?.turretPlates;
            if (plates == null) return names;
            for (int i = 0;
                i < plates.Length;
                i++)
            {
                ArmorPlateDefinition plate =
                    plates[i];
                if (plate != null &&
                    plate.gunFollow &&
                    !string.IsNullOrEmpty(
                        plate.name))
                {
                    names.Add(
                        "Armor-" +
                        plate.name);
                }
            }
            return names;
        }

        private static bool HasCandidateAncestor(
            Transform part,
            Transform turret,
            HashSet<string> gunFollow)
        {
            Transform ancestor = part.parent;
            while (ancestor != null &&
                ancestor != turret)
            {
                if (IsCandidate(
                        ancestor.name,
                        gunFollow))
                {
                    return true;
                }
                ancestor = ancestor.parent;
            }
            return false;
        }

        private static bool IsCandidate(
            string name,
            HashSet<string> gunFollow)
        {
            if (name == "Gun" ||
                gunFollow.Contains(name))
            {
                return true;
            }
            for (int i = 0;
                i < ExcludedTokens.Length;
                i++)
            {
                if (name.IndexOf(
                        ExcludedTokens[i],
                        StringComparison
                            .OrdinalIgnoreCase) >=
                    0)
                {
                    return false;
                }
            }
            for (int i = 0;
                i < GunTokens.Length;
                i++)
            {
                if (name.IndexOf(
                        GunTokens[i],
                        StringComparison
                            .OrdinalIgnoreCase) >=
                    0)
                {
                    return true;
                }
            }
            return false;
        }

        private sealed class PitchPart
        {
            public Transform Transform;
            public Vector3 Position;
            public Quaternion Rotation;
        }
    }
}
