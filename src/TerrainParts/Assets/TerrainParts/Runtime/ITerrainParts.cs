namespace TerrainParts
{
    public interface ITerrainParts
    {
        ToolCategory GetToolCategory();
        int GetTextureLayerIndex();
        void GetRect(out float minX, out float minZ, out float maxX, out float maxZ);
        void Setup(float unitPerPixel);
        float GetHeight(float worldX, float worldZ, float currentHeight);
        float GetAlpha(float worldX, float worldZ, float currentAlpha);
        int GetLayer();
        int GetOrderInLayer();
    }
}
