namespace ClaudeOfTanks.Runtime
{
    internal static class TankRunningGearStations
    {
        private static readonly float[] Puma =
            { 1.791f, 1.009f, 0.247f, -0.68f, -1.43f, -2.173f };
        private static readonly float[] PumaS1 =
            { 2.178f, 1.359f, 0.531f, -0.306f, -1.152f, -1.98f };
        private static readonly float[] Bradley =
            { 1.88f, 1.13f, 0.38f, -0.37f, -1.12f, -1.87f };
        private static readonly float[] Bmp2 =
            { 1.506f, 0.786f, 0.066f, -0.654f, -1.374f, -2.094f };
        private static readonly float[] Bmp3 =
            { 1.79f, 1.04f, 0.055f, -0.62f, -1.315f, -2.15f };
        private static readonly float[] Upior =
            { 1.577f, 0.978f, 0.345f, -0.435f, -1.032f, -1.628f };
        private static readonly float[] Bmpt2 =
            { 0.883f, 0.107f, -0.669f, -1.445f, -2.221f, -2.997f };
        private static readonly float[] AbramsM1 =
            { 2.19f, 1.46f, 0.73f, 0f, -0.73f, -1.46f, -2.19f };
        private static readonly float[] M1A3 =
            { 2.25f, 1.5f, 0.75f, 0f, -0.75f, -1.5f, -2.25f };
        private static readonly float[] AbramsX =
        {
            2.1674f, 1.3713f, 0.6533f, -0.0648f,
            -0.7828f, -1.5012f, -2.2189f
        };
        private static readonly float[] T62Obr1975 =
        {
            2.235f, 1.297f, 0.293f, -0.791f, -1.933f
        };
        private static readonly float[] T64BV1 =
        {
            1.875f, 1.125f, 0.4f,
            -0.325f, -1.075f, -1.775f
        };
        private static readonly float[] T72B3M =
        {
            -2.9f, -2.238f, -1.456f,
            -0.674f, 0.108f, 0.89f
        };

        public static float PumaAt(int index) => Puma[index];
        public static float PumaS1At(int index) => PumaS1[index];
        public static float BradleyAt(int index) => Bradley[index];
        public static float Bmp2At(int index) => Bmp2[index];
        public static float Bmp3At(int index) => Bmp3[index];
        public static float UpiorAt(int index) => Upior[index];
        public static float Bmpt2At(int index) => Bmpt2[index];
        public static float AbramsM1At(int index) => AbramsM1[index];
        public static float M1A3At(int index) => M1A3[index];
        public static float AbramsXAt(int index) => AbramsX[index];
        public static float T62Obr1975At(int index) =>
            T62Obr1975[index];
        public static float T64BV1At(int index) =>
            T64BV1[index];
        public static float T72B3MAt(int index) =>
            T72B3M[index];
    }
}
