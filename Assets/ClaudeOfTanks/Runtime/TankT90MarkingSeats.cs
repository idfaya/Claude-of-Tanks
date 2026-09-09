using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90MarkingSeats
    {
        public static Transform Build(
            string id,
            Transform parent,
            string tacticalNumber)
        {
            if (string.IsNullOrEmpty(tacticalNumber)) return null;
            switch (id)
            {
                case "t90":
                    return BuildT90(parent, tacticalNumber);
                case "t90a":
                    return BuildT90A(parent, tacticalNumber);
                case "t90a_vladimir":
                    return BuildVladimir(parent, tacticalNumber);
                case "t90a_burlak":
                    return BuildBurlak(parent, tacticalNumber);
                case "t90sm":
                    return BuildT90SM(parent, tacticalNumber);
                case "t90ms":
                    return BuildT90MS(parent, tacticalNumber);
                default:
                    throw new ArgumentException(
                        "Unsupported T-90 marking seat id: " + id,
                        nameof(id));
            }
        }

        private static Transform BuildT90(
            Transform parent,
            string number)
        {
            return TankTacticalNumberFactory.BuildRussianSet(
                "T90",
                parent,
                number,
                I("Insignia", 0.23f,
                    V(-1.0333622f, 0.3992434f, -1.3922033f),
                    Q(-0.0676667f, -0.7611755f,
                        -0.0571143f, 0.6424726f)),
                D("Designation", 0.23f,
                    V(-1.4004122f, 0.3985853f, 0.3175893f),
                    Q(-0.0211688f, -0.6511015f,
                        -0.0246539f, 0.7582947f)));
        }

        private static Transform BuildT90A(
            Transform parent,
            string number)
        {
            return TankTacticalNumberFactory.BuildRussianSet(
                "T90A",
                parent,
                number,
                I("Insignia", 0.23f,
                    V(1.6185539f, 0.10675f, -1.5836288f),
                    Q(0f, 0.7176335f, 0f, 0.6964209f)),
                D("Designation", 0.23f,
                    V(1.2155189f, 0.4222885f, 0.0083077f),
                    Q(-0.2304606f, 0.7025491f,
                        0.2098583f, 0.6397438f)));
        }

        private static Transform BuildVladimir(
            Transform parent,
            string number)
        {
            return TankTacticalNumberFactory.BuildRussianSet(
                "T90AVladimir",
                parent,
                number,
                I("Insignia", 0.23f,
                    V(-1.3826577f, 0.2355f, -0.691368f),
                    Q(0f, -0.6782645f, 0f, 0.7348178f)),
                D("Designation", 0.23f,
                    V(-1.1937633f, 0.4791295f, -0.8909543f),
                    Q(-0.1116375f, -0.714071f,
                        -0.1067518f, 0.6828204f)));
        }

        private static Transform BuildBurlak(
            Transform parent,
            string number)
        {
            return TankTacticalNumberFactory.BuildRussianSet(
                "T90ABurlak",
                parent,
                number,
                I("Insignia", 0.23f,
                    V(0.9859912f, 0.4226153f, -1.5825224f),
                    Q(-0.0709102f, 0.7976612f,
                        0.0530336f, 0.59657f)),
                D("Designation", 0.23f,
                    V(1.0533363f, 0.252985f, -1.2382426f),
                    Q(0.1494883f, 0.7627854f,
                        -0.1209944f, 0.6173913f)));
        }

        private static Transform BuildT90SM(
            Transform parent,
            string number)
        {
            return TankTacticalNumberFactory.BuildRussianSet(
                "T90SM",
                parent,
                number,
                D("Designation-Right", 0.26f,
                    V(1.4863963f, 0.3038142f, -0.05f),
                    Q(-0.2387832f, 0.6655694f,
                        0.2387832f, 0.6655694f)),
                D("Designation-Left", 0.26f,
                    V(-1.4863963f, 0.3038142f, -0.05f),
                    Q(-0.2387832f, -0.6655694f,
                        -0.2387832f, 0.6655694f)),
                I("Insignia", 0.24f,
                    V(-1.141f, 0.3824f, -1.33472f),
                    Q(0f, -0.7071068f, 0f, 0.7071068f)));
        }

        private static Transform BuildT90MS(
            Transform parent,
            string number)
        {
            return TankTacticalNumberFactory.BuildRussianSet(
                "T90MS",
                parent,
                number,
                I("Insignia", 0.24f,
                    V(0.9610577f, 0.6983223f, -0.8166105f),
                    Q(-0.3780458f, 0.6207669f,
                        0.3572425f, 0.586607f)),
                D("Designation", 0.24f,
                    V(1.0750685f, 0.5469447f, -1.2616064f),
                    Q(-0.1696276f, 0.7701519f,
                        0.1322612f, 0.6004993f)));
        }

        private static TankTacticalMarkingSeat I(
            string name,
            float size,
            Vector3 position,
            Quaternion rotation)
        {
            return new TankTacticalMarkingSeat(
                name,
                TankTacticalMarkingKind.Insignia,
                size,
                position,
                rotation);
        }

        private static TankTacticalMarkingSeat D(
            string name,
            float size,
            Vector3 position,
            Quaternion rotation)
        {
            return new TankTacticalMarkingSeat(
                name,
                TankTacticalMarkingKind.Designation,
                size,
                position,
                rotation);
        }

        private static Vector3 V(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }

        private static Quaternion Q(
            float x,
            float y,
            float z,
            float w)
        {
            return new Quaternion(x, y, z, w);
        }
    }
}
