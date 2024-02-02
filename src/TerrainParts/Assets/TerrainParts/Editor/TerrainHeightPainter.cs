using System.Collections.Generic;
using UnityEngine;

namespace TerrainParts.Editor
{
    public class TerrainHeightPainter
    {
        private readonly Terrain _terrain;

        public TerrainHeightPainter(Terrain terrain)
        {
            _terrain = terrain;
        }

        public void Paint(IEnumerable<ITerrainParts> parts)
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

            foreach (var part in parts)
            {
                part.GetRect(out var minX, out var minZ, out var maxX, out var maxZ);
                minX = Mathf.Max(minX - _terrain.transform.position.x, 0);
                minZ = Mathf.Max(minZ - _terrain.transform.position.z, 0);
                maxX = Mathf.Min(maxX - _terrain.transform.position.x, _terrain.terrainData.size.x);
                maxZ = Mathf.Min(maxZ - _terrain.transform.position.z, _terrain.terrainData.size.z);
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
                        var worldX = _terrain.transform.position.x + (float)pixelX / resolution * terrainData.size.x;
                        var worldZ = _terrain.transform.position.z + (float)pixelZ / resolution * terrainData.size.z;
                        var currentHeight = heightMap[pixelZ, pixelX] * terrainData.size.y + _terrain.transform.position.y;
                        heightMap[pixelZ, pixelX] = (part.GetHeight(worldX, worldZ, currentHeight) - _terrain.transform.position.y) / terrainData.size.y;
                    }
                }
            }

            terrainData.SetHeights(0, 0, heightMap);
        }
    }
}