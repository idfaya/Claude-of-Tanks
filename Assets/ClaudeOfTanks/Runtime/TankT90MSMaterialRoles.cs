using System;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90MSMaterialRoles
    {
        public static TankT90MaterialApplicator.Role? Resolve(
            string name)
        {
            if (string.Equals(
                    name,
                    "T90MS-UnditchingLog",
                    StringComparison.Ordinal))
            {
                return TankT90MaterialApplicator.Role.Wood;
            }
            if (string.Equals(
                    name,
                    "T90MS-SpareTrackLink",
                    StringComparison.Ordinal))
            {
                return TankT90MaterialApplicator.Role.Track;
            }
            if ((name.IndexOf(
                     "Window",
                     StringComparison.Ordinal) >= 0 &&
                 name.IndexOf(
                     "WindowSlot",
                     StringComparison.Ordinal) < 0) ||
                string.Equals(
                    name,
                    "T90MS-TagilTowerWorkLight",
                    StringComparison.Ordinal))
            {
                return TankT90MaterialApplicator.Role.Glass;
            }
            if (string.Equals(
                    name,
                    "Painted-T90MS-FrontMudFlapSupport",
                    StringComparison.Ordinal))
            {
                return TankT90MaterialApplicator.Role.Hull;
            }
            if (string.Equals(
                    name,
                    "T90MS-FrontMudFlapRidge",
                    StringComparison.Ordinal))
            {
                return TankT90MaterialApplicator.Role.Dark;
            }
            if (name.IndexOf(
                    "MudFlap",
                    StringComparison.Ordinal) >= 0)
            {
                return TankT90MaterialApplicator.Role.Rubber;
            }
            if (string.Equals(
                    name,
                    "Painted-T90MS-GunBootSection",
                    StringComparison.Ordinal))
            {
                return TankT90MaterialApplicator.Role.Canvas;
            }
            if (string.Equals(
                    name,
                    "T90MS-2A46M5MuzzleBoreDisc",
                    StringComparison.Ordinal))
            {
                return TankT90MaterialApplicator.Role.Shadow;
            }
            if (name.IndexOf(
                    "2A46M5SleeveRing",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "2A46M5FumeBand",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "2A46M5MuzzleBore",
                    StringComparison.Ordinal) >= 0)
            {
                return TankT90MaterialApplicator.Role.Dark;
            }
            return null;
        }
    }
}
