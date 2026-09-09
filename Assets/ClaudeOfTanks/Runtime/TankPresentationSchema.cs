using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal enum TankPresentationTarget
    {
        Root,
        Turret,
        GunFittings
    }

    internal enum TankPresentationColor
    {
        Base,
        Dark,
        Glass,
        ShtoraGlass,
        Wood,
        Black
    }

    internal enum TankPresentationLengthMode
    {
        Fixed,
        GunLengthPlus,
        GunLengthMinus
    }

    internal readonly struct TankPresentationPart
    {
        public readonly string Name;
        public readonly PrimitiveType Type;
        public readonly TankPresentationTarget Target;
        public readonly Vector3 Position;
        public readonly Vector3 Scale;
        public readonly Vector3 Rotation;
        public readonly TankPresentationColor Color;
        public readonly float ColorMultiplier;
        public readonly TankPresentationLengthMode PositionZMode;
        public readonly float PositionZValue;
        public readonly TankPresentationLengthMode ScaleYMode;
        public readonly float ScaleYValue;
        public readonly float ScaleYMinimum;

        public TankPresentationPart(
            string name,
            PrimitiveType type,
            TankPresentationTarget target,
            Vector3 position,
            Vector3 scale,
            Vector3 rotation,
            TankPresentationColor color,
            float colorMultiplier = 1f,
            TankPresentationLengthMode positionZMode =
                TankPresentationLengthMode.Fixed,
            float positionZValue = 0f,
            TankPresentationLengthMode scaleYMode =
                TankPresentationLengthMode.Fixed,
            float scaleYValue = 0f,
            float scaleYMinimum = 0f)
        {
            Name = name;
            Type = type;
            Target = target;
            Position = position;
            Scale = scale;
            Rotation = rotation;
            Color = color;
            ColorMultiplier = colorMultiplier;
            PositionZMode = positionZMode;
            PositionZValue = positionZValue;
            ScaleYMode = scaleYMode;
            ScaleYValue = scaleYValue;
            ScaleYMinimum = scaleYMinimum;
        }
    }

    internal sealed class TankPresentationSchema
    {
        public delegate void ExtraBuilder(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color);

        public readonly string MarkerName;
        public readonly string GunFittingsRootName;
        public readonly float GunLengthFallback;
        public readonly string[] HiddenNames;
        public readonly string[] HiddenPrefixes;
        public readonly TankPresentationPart[] Parts;
        public readonly ExtraBuilder ExtraBuild;

        public TankPresentationSchema(
            string markerName,
            string gunFittingsRootName,
            float gunLengthFallback,
            string[] hiddenNames,
            string[] hiddenPrefixes,
            TankPresentationPart[] parts,
            ExtraBuilder extraBuild = null)
        {
            MarkerName = markerName;
            GunFittingsRootName = gunFittingsRootName;
            GunLengthFallback = gunLengthFallback;
            HiddenNames = hiddenNames;
            HiddenPrefixes = hiddenPrefixes;
            Parts = parts;
            ExtraBuild = extraBuild;
        }
    }

    internal static class TankPresentationGenerator
    {
        public static void Build(
            TankPresentationSchema schema,
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            if (schema == null) return;
            HideTargets(root, schema);

            if (!string.IsNullOrEmpty(schema.MarkerName))
            {
                GameObject marker = new GameObject(schema.MarkerName);
                marker.transform.SetParent(root, false);
            }

            Transform gunFittings = null;
            float gunLength = TankAuthoredDetails.ResolveGunLength(
                definition,
                schema.GunLengthFallback);
            for (int index = 0;
                index < schema.Parts.Length;
                index++)
            {
                TankPresentationPart part = schema.Parts[index];
                Transform parent = ResolveParent(
                    part.Target,
                    root,
                    turret,
                    schema.GunFittingsRootName,
                    ref gunFittings);
                if (parent == null) continue;

                Vector3 position = part.Position;
                position.z = ResolveLength(
                    part.PositionZMode,
                    position.z,
                    part.PositionZValue,
                    gunLength,
                    0f);
                Vector3 scale = part.Scale;
                scale.y = ResolveLength(
                    part.ScaleYMode,
                    scale.y,
                    part.ScaleYValue,
                    gunLength,
                    part.ScaleYMinimum);

                Transform transform = TankDetailGeometry.Part(
                    part.Name,
                    part.Type,
                    parent,
                    position,
                    scale,
                    ResolveColor(part, color));
                transform.localRotation =
                    Quaternion.Euler(part.Rotation);
            }
            schema.ExtraBuild?.Invoke(root, turret, definition, color);
        }

        private static Transform ResolveParent(
            TankPresentationTarget target,
            Transform root,
            Transform turret,
            string gunFittingsRootName,
            ref Transform gunFittings)
        {
            if (target == TankPresentationTarget.Root) return root;
            if (target == TankPresentationTarget.Turret) return turret;

            if (gunFittings != null) return gunFittings;
            Transform gun = turret.Find("Gun");
            if (gun == null) return null;
            gunFittings = TankDetailGeometry.GunFittingsRoot(
                gun,
                gunFittingsRootName);
            return gunFittings;
        }

        private static float ResolveLength(
            TankPresentationLengthMode mode,
            float fixedValue,
            float value,
            float gunLength,
            float minimum)
        {
            if (mode == TankPresentationLengthMode.GunLengthPlus)
                return gunLength + value;
            if (mode == TankPresentationLengthMode.GunLengthMinus)
                return Math.Max(minimum, gunLength - value);
            return fixedValue;
        }

        private static Color ResolveColor(
            TankPresentationPart part,
            Color baseColor)
        {
            Color color;
            switch (part.Color)
            {
                case TankPresentationColor.Dark:
                    color = new Color(0.055f, 0.065f, 0.05f);
                    break;
                case TankPresentationColor.Glass:
                    color = new Color(0.025f, 0.08f, 0.075f);
                    break;
                case TankPresentationColor.ShtoraGlass:
                    color = new Color(0.5f, 0.07f, 0.035f);
                    break;
                case TankPresentationColor.Wood:
                    color = new Color(0.2f, 0.12f, 0.065f);
                    break;
                case TankPresentationColor.Black:
                    color = Color.black;
                    break;
                default:
                    color = baseColor;
                    break;
            }
            return color * part.ColorMultiplier;
        }

        private static void HideTargets(
            Transform root,
            TankPresentationSchema schema)
        {
            Transform[] parts =
                root.GetComponentsInChildren<Transform>(true);
            for (int index = 0;
                index < parts.Length;
                index++)
            {
                if (ShouldHide(parts[index].name, schema))
                    Hide(parts[index]);
            }
        }

        private static bool ShouldHide(
            string name,
            TankPresentationSchema schema)
        {
            for (int index = 0;
                index < schema.HiddenNames.Length;
                index++)
            {
                if (string.Equals(
                    name,
                    schema.HiddenNames[index],
                    StringComparison.Ordinal))
                    return true;
            }
            for (int index = 0;
                index < schema.HiddenPrefixes.Length;
                index++)
            {
                if (name.StartsWith(
                    schema.HiddenPrefixes[index],
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
