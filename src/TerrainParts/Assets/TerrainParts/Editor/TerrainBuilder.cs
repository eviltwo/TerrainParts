using System.Collections.Generic;
using UnityEngine;

namespace TerrainParts.Editor
{
    public class TerrainBuilder
    {
        private readonly Terrain _terrain;

        public TerrainBuilder(Terrain terrain)
        {
            _terrain = terrain;
        }

        public void Build(IReadOnlyList<ITerrainParts> parts)
        {
            var terrainData = _terrain.terrainData;
            var resolution = terrainData.heightmapResolution;
            var heightMap = terrainData.GetHeights(0, 0, resolution, resolution);
            for (var x = 0; x < resolution; x++)
            {
                for (var y = 0; y < resolution; y++)
                {
                    heightMap[x, y] = 0;
                }
            }

            var partsCount = parts.Count;
            for (int i = 0; i < partsCount; i++)
            {
                var p = parts[i];
                p.GetRect(out var minX, out var minZ, out var maxX, out var maxZ);
                minX = Mathf.Max(minX, _terrain.transform.position.x);
                minZ = Mathf.Max(minZ, _terrain.transform.position.z);
                maxX = Mathf.Min(maxX, _terrain.transform.position.x + _terrain.terrainData.size.x);
                maxZ = Mathf.Min(maxZ, _terrain.transform.position.z + _terrain.terrainData.size.z);
                var baseX = Mathf.CeilToInt(minX / terrainData.size.x * resolution);
                var baseZ = Mathf.CeilToInt(minZ / terrainData.size.z * resolution);
                var exX = Mathf.CeilToInt(maxX / terrainData.size.x * resolution);
                var exZ = Mathf.CeilToInt(maxZ / terrainData.size.z * resolution);
                var xResolution = exX - baseX;
                var zResolution = exZ - baseZ;
                for (var x = 0; x < xResolution; x++)
                {
                    for (var z = 0; z < zResolution; z++)
                    {
                        var pixelX = baseX + x;
                        var pixelZ = baseZ + z;
                        var worldX = (float)pixelX / resolution * terrainData.size.x;
                        var worldZ = (float)pixelZ / resolution * terrainData.size.z;
                        var currentHeight = heightMap[pixelZ, pixelX] * terrainData.size.y;
                        heightMap[pixelZ, pixelX] = p.GetHeight(worldX, worldZ, currentHeight) / terrainData.size.y;
                    }
                }
            }

            terrainData.SetHeights(0, 0, heightMap);
        }
    }
}
