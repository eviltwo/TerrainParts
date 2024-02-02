namespace TerrainParts
{
    public interface ITerrainParts
    {
        TerrainPartsBasicData GetBasicData();
        void GetRect(out float minX, out float minZ, out float maxX, out float maxZ);
        void Setup(float unitPerPixel);
        bool GetHeight(float worldX, float worldZ, out float height, out float alpha);
        float GetAlpha(float worldX, float worldZ, float currentAlpha);
    }
}
