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
            var resolution = new Vector2Int(terrainData.heightmapResolution, terrainData.heightmapResolution);
            var heightMap = terrainData.GetHeights(0, 0, resolution.x, resolution.y);
            for (var x = 0; x < resolution.x; x++)
            {
                for (var y = 0; y < resolution.y; y++)
                {
                    heightMap[y, x] = 0;
                }
            }

            foreach (var part in parts)
            {
                var basicData = part.GetBasicData();
                part.GetRect(out var minX, out var minZ, out var maxX, out var maxZ);
                PainterUtility.CalculatePixelRange(minX, minZ, maxX, maxZ, _terrain, resolution, out var pixelBase, out var pixelSize);
                for (var x = 0; x < pixelSize.x; x++)
                {
                    for (var y = 0; y < pixelSize.y; y++)
                    {
                        var pixelPos = new Vector2Int(pixelBase.x + x, pixelBase.y + y);
                        var worldPos = PainterUtility.PixelToWorld(pixelPos, resolution, _terrain);
                        if (part.TryGetHeight(worldPos.x, worldPos.z, out var resultHeight, out var resultAlpha))
                        {
                            resultHeight -= _terrain.transform.position.y;
                            var currentHeight = heightMap[pixelPos.y, pixelPos.x] * terrainData.size.y;
                            var mergedHeight = TerrainPartsUtility.MergeHeight(currentHeight, resultHeight, basicData.HeightWriteCondition);
                            var smoothedHeight = Mathf.Lerp(currentHeight, mergedHeight, resultAlpha);
                            heightMap[pixelPos.y, pixelPos.x] = smoothedHeight / terrainData.size.y;
                        }

                    }
                }
            }

            terrainData.SetHeights(0, 0, heightMap);
        }
    }
}
