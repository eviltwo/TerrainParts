namespace TerrainParts
{
    [System.Flags]
    public enum ToolCategory
    {
        None = 0,
        Height = 1 << 1,
        Texture = 1 << 2,
        Hole = 1 << 3,
    }

    public static class ToolCategoryExtention
    {
        public const ToolCategory Everything = (ToolCategory)~0;

        public static bool HasFlagAll(this ToolCategory self, ToolCategory flag)
        {
            return (self & flag) == flag;
        }

        public static bool HasFlagAny(this ToolCategory self, ToolCategory flag)
        {
            return (self & flag) != 0;
        }
    }
}
