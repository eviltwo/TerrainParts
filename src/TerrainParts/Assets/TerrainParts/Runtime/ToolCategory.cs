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
}
