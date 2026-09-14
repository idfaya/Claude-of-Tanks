namespace ClaudeOfTanks.Runtime
{
    public sealed partial class MapStructureRuntime
    {
        private static void AddFramedWindow(
            BuildingShape shape,
            float x,
            float y,
            float z,
            float width,
            float height)
        {
            Box(
                shape,
                shape.Details,
                x,
                y,
                z,
                width,
                height,
                0.08f);
            float frame = 0.09f;
            for (int side = -1; side <= 1; side += 2)
                Box(
                    shape,
                    shape.Roofs,
                    x + side * (width * 0.5f +
                        frame * 0.5f),
                    y,
                    z + 0.03f,
                    frame,
                    height + 0.2f,
                    0.11f);
            Box(
                shape,
                shape.Roofs,
                x,
                y + height * 0.5f + frame,
                z + 0.03f,
                width + frame * 2f,
                frame,
                0.11f);
            Box(
                shape,
                shape.Roofs,
                x,
                y - height * 0.5f - frame,
                z + 0.03f,
                width + frame * 2f,
                frame,
                0.14f);
        }

        private static void AddLonghouseDetails(
            BuildingShape shape)
        {
            PitchedBox(
                shape,
                shape.Details,
                0f,
                shape.Height * 0.96f,
                0f,
                0.28f,
                0.28f,
                shape.Depth * 1.04f,
                0f);
            AddSideWindows(shape, 4);
        }

        private static void AddSideWindows(
            BuildingShape shape,
            int count)
        {
            for (int i = 0; i < count; i++)
                for (int side = -1;
                    side <= 1;
                    side += 2)
                    Box(
                        shape,
                        shape.Details,
                        side * shape.Width * 0.505f,
                        shape.Height * 0.34f,
                        -shape.Depth * 0.38f +
                            shape.Depth * 0.76f *
                            (i + 0.5f) / count,
                        0.1f,
                        shape.Height * 0.18f,
                        shape.Depth * 0.11f);
        }
    }
}
