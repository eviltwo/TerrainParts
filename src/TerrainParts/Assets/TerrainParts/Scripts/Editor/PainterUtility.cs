using UnityEngine;

namespace TerrainParts.Editor
{
    public static class PainterUtility
    {
        public static void CalculatePixelRange(
            float worldMinX,
            float worldMinZ,
            float worldMaxX,
            float worldMaxZ,
            Terrain terrain,
            Vector2Int pixelResolution,
            out Vector2Int pixelBase,
            out Vector2Int pixelSize)
        {
            var terrainData = terrain.terrainData;
            var localMinX = Mathf.Max(worldMinX - terrain.transform.position.x, 0);
            var localMinZ = Mathf.Max(worldMinZ - terrain.transform.position.z, 0);
            var localMaxX = Mathf.Min(worldMaxX - terrain.transform.position.x, terrainData.size.x);
            var localMaxZ = Mathf.Min(worldMaxZ - terrain.transform.position.z, terrainData.size.z);
            pixelBase = new Vector2Int(
                Mathf.CeilToInt(localMinX / terrainData.size.x * pixelResolution.x),
                Mathf.CeilToInt(localMinZ / terrainData.size.z * pixelResolution.y));
            var pixelMax = new Vector2Int(
                Mathf.CeilToInt(localMaxX / terrainData.size.x * pixelResolution.x),
                Mathf.CeilToInt(localMaxZ / terrainData.size.z * pixelResolution.y));
            pixelSize = pixelMax - pixelBase;
        }

        public static Vector3 PixelToWorld(Vector2Int pixel, Vector2Int pixelResolution, Terrain terrain)
        {
            var terrainData = terrain.terrainData;
            return new Vector3(
                terrain.transform.position.x + (float)pixel.x / pixelResolution.x * terrainData.size.x,
                terrain.transform.position.y,
                terrain.transform.position.z + (float)pixel.y / pixelResolution.y * terrainData.size.z);
        }
    }
}
