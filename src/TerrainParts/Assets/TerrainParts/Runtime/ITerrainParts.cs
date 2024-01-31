using UnityEngine;

namespace TerrainParts
{
    public interface ITerrainParts
    {
        void GetRect(out float minX, out float minZ, out float maxX, out float maxZ);
        void Setup(Terrain terrain);
        float GetHeight(float worldX, float worldZ, float currentHeight);
        int GetLayer();
        int GetOrderInLayer();
    }
}
