namespace TerrainParts
{
    public interface ITerrainParts
    {
        TerrainPartsBasicData GetBasicData();
        void GetRect(out float minX, out float minZ, out float maxX, out float maxZ);
        void Setup(float unitPerPixel);
        bool TryGetHeight(float worldX, float worldZ, out float height, out float alpha);
        bool TryGetAlpha(float worldX, float worldZ, out float alpha);
    }
}
