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
            var resolution = new Vector2Int(terrainData.alphamapWidth, terrainData.alphamapHeight);
            var alphaMaps = new float[resolution.y, resolution.x, layerCount];
            for (var x = 0; x < resolution.x; x++)
            {
                for (var y = 0; y < resolution.y; y++)
                {
                    for (var layer = 0; layer < layerCount; layer++)
                    {
                        alphaMaps[y, x, layer] = layer == 0 ? 1 : 0;
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
                PainterUtility.CalculatePixelRange(minX, minZ, maxX, maxZ, _terrain, resolution, out var pixelBase, out var pixelSize);
                for (var x = 0; x < pixelSize.x; x++)
                {
                    for (var y = 0; y < pixelSize.y; y++)
                    {
                        var pixelPos = new Vector2Int(pixelBase.x + x, pixelBase.y + y);
                        var worldPos = PainterUtility.PixelToWorld(pixelPos, resolution, _terrain);
                        if (part.TryGetAlpha(worldPos.x, worldPos.z, out var resultAlpha))
                        {
                            resultAlpha *= basicData.TextureLayerStrength;
                            var currentAlpha = alphaMaps[pixelPos.y, pixelPos.x, partLayer];
                            var mergedAlpha = Mathf.Clamp01(currentAlpha + resultAlpha);
                            Blend(alphaMaps, pixelPos, layerCount, partLayer, mergedAlpha);
                        }
                    }
                }
            }

            for (var x = 0; x < resolution.x; x++)
            {
                for (var y = 0; y < resolution.y; y++)
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

        public static void Blend(float[,,] alphaMap, Vector2Int pixelPos, int layerCount, int targetLayer, float tagetAlpha)
        {
            alphaMap[pixelPos.y, pixelPos.x, targetLayer] = Mathf.Clamp01(tagetAlpha);
            var targetOthersAlpha = 1 - tagetAlpha;
            var totalOthersAlpha = 0f;
            for (var layer = 0; layer < layerCount; layer++)
            {
                if (layer == targetLayer)
                {
                    continue;
                }
                totalOthersAlpha += alphaMap[pixelPos.y, pixelPos.x, layer];
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
                alphaMap[pixelPos.y, pixelPos.x, layer] *= multiplier;
            }
        }
    }
}
