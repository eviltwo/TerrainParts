using System.Collections.Generic;
using UnityEngine;

namespace TerrainParts.Editor
{
    public class TerrainTexturePainter
    {
        private readonly Terrain _terrain;

        public TerrainTexturePainter(Terrain terrain)
        {
            _terrain = terrain;
        }

        public void Paint(IEnumerable<ITerrainParts> parts)
        {
            var terrainData = _terrain.terrainData;
            var layerCount = terrainData.alphamapLayers;
            var resolution = terrainData.alphamapResolution;
            var alphaMaps = new float[resolution, resolution, layerCount];
            for (var x = 0; x < resolution; x++)
            {
                for (var y = 0; y < resolution; y++)
                {
                    for (var layer = 0; layer < layerCount; layer++)
                    {
                        alphaMaps[x, y, layer] = layer == 0 ? 1 : 0;
                    }
                }
            }

            foreach (var part in parts)
            {
                var basicData = part.GetBasicData();
                var partLayer = basicData.TextureLayerIndex;
                if (partLayer >= layerCount)
                {
                    continue;
                }
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
                        if (part.TryGetAlpha(worldX, worldZ, out var resultAlpha))
                        {
                            var currentAlpha = alphaMaps[pixelZ, pixelX, partLayer];
                            var mergedAlpha = Mathf.Clamp01(currentAlpha + resultAlpha);
                            Blend(alphaMaps, pixelX, pixelZ, layerCount, partLayer, mergedAlpha);
                        }
                    }
                }
            }

            for (var x = 0; x < resolution; x++)
            {
                for (var y = 0; y < resolution; y++)
                {
                    var totalAlpha = 0f;
                    for (var layer = 1; layer < layerCount; layer++)
                    {
                        totalAlpha += alphaMaps[y, x, layer];
                    }
                    alphaMaps[y, x, 0] = Mathf.Clamp01(1 - totalAlpha);
                }
            }

            terrainData.SetAlphamaps(0, 0, alphaMaps);
        }

        public static void Blend(float[,,] alphaMap, int x, int z, int layerCount, int targetLayer, float tagetAlpha)
        {
            alphaMap[z, x, targetLayer] = Mathf.Clamp01(tagetAlpha);
            var targetOthersAlpha = 1 - tagetAlpha;
            var totalOthersAlpha = 0f;
            for (var layer = 0; layer < layerCount; layer++)
            {
                if (layer == targetLayer)
                {
                    continue;
                }
                totalOthersAlpha += alphaMap[z, x, layer];
            }

            if (totalOthersAlpha == 0)
            {
                return;
            }

            var multiplier = targetOthersAlpha / totalOthersAlpha;
            for (var layer = 0; layer < layerCount; layer++)
            {
                if (layer == targetLayer)
                {
                    continue;
                }
                alphaMap[z, x, layer] *= multiplier;
            }
        }
    }
}
